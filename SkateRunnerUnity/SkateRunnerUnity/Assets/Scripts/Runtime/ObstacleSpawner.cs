using System.Collections.Generic;
using UnityEngine;

public sealed class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float laneWidth = 2.2f;
    [SerializeField] private float spawnAhead = 55f;
    [SerializeField] private float spawnStep = 7f;
    [SerializeField] private float longFeatureSpacingBuffer = 4f;
    [SerializeField] private Material obstacleMaterial;
    [SerializeField] private Material railMaterial;
    [SerializeField] private Material coinMaterial;
    [SerializeField] private Material rampMaterial;
    [SerializeField] private Material markerMaterial;
    [SerializeField] private Material featureMaterial;
    [SerializeField] private Material stairMaterial;
    [SerializeField] private Material hazardMaterial;

    private readonly List<GameObject> spawned = new();
    private readonly Dictionary<string, Stack<GameObject>> pools = new();
    private Transform poolRoot;
    private float nextSpawnZ;
    private float reservedPhysicalSpawnUntilZ;
    private bool spawningEnabled = true;

    private void Awake()
    {
        if (player == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        poolRoot = new GameObject("Spawn Pools").transform;
        poolRoot.SetParent(transform);
        poolRoot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!spawningEnabled || player == null)
        {
            return;
        }

        while (nextSpawnZ < player.position.z + spawnAhead)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += spawnStep;
        }

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null)
            {
                spawned.RemoveAt(i);
                continue;
            }

            if (!spawned[i].activeSelf || spawned[i].transform.position.z < player.position.z - 20f)
            {
                ReleaseSpawnedAt(i);
            }
        }
    }

    public void ResetRun()
    {
        ClearSpawned();
        nextSpawnZ = 18f;
        reservedPhysicalSpawnUntilZ = 0f;
        spawningEnabled = true;
    }

    public void StartFinalBonus(float rampZ)
    {
        spawningEnabled = false;
        ClearSpawned();
        SpawnFinalRamp(rampZ);
        SpawnBonusMarkers(rampZ);
    }

    private void SpawnRow(float z)
    {
        int pattern = Random.Range(0, 10);
        int lane = Random.Range(-1, 2);

        if (z < reservedPhysicalSpawnUntilZ)
        {
            SpawnRewardOnlyRow(lane, z);
            return;
        }

        if (pattern == 0)
        {
            SpawnObstacle(lane, z);
            SpawnCoin(-lane, z + 2f);
        }
        else if (pattern == 1)
        {
            SpawnRail(lane, z);
        }
        else if (pattern == 2)
        {
            SpawnObstacle(-1, z);
            SpawnObstacle(1, z + 1.2f);
            SpawnCoin(0, z + 3f);
        }
        else if (pattern == 3)
        {
            SpawnCoin(lane, z);
            SpawnCoin(lane, z + 1.8f);
            SpawnCoin(lane, z + 3.6f);
        }
        else if (pattern == 4)
        {
            SpawnKickerRamp(lane, z);
        }
        else if (pattern == 5)
        {
            SpawnBounceStairs(lane, z);
        }
        else if (pattern == 6)
        {
            SpawnManualPad(lane, z);
        }
        else if (pattern == 7)
        {
            SpawnNegativePickup(lane, z);
            SpawnCoin(-lane, z + 2.2f);
        }
        else
        {
            SpawnCoin(lane, z);
            SpawnCoin(Mathf.Clamp(lane + 1, -1, 1), z + 2.5f);
        }
    }

    private void SpawnObstacle(int lane, float z)
    {
        int obstacleType = Random.Range(0, 4);
        string obstacleName = "Construction Barrier";
        Vector3 position = new Vector3(lane * laneWidth, 0.85f, z);
        Vector3 scale = new Vector3(1.55f, 1.7f, 0.85f);

        if (obstacleType == 1)
        {
            obstacleName = "Parked Scooter";
            position.y = 0.45f;
            scale = new Vector3(1.25f, 0.9f, 1.5f);
        }
        else if (obstacleType == 2)
        {
            obstacleName = "Trash Can Stack";
            position.y = 0.7f;
            scale = new Vector3(1.25f, 1.4f, 1.0f);
        }
        else if (obstacleType == 3)
        {
            obstacleName = "Security Blocker";
            position.y = 1.0f;
            scale = new Vector3(1.8f, 2.0f, 0.55f);
        }
        GameObject obstacle = GetPooledObject("Obstacle", CreateObstacleObject);
        obstacle.name = obstacleName;
        obstacle.transform.position = position;
        obstacle.transform.rotation = Quaternion.identity;
        obstacle.transform.localScale = scale;
        obstacle.GetComponent<RunnerObstacle>().ResetHit();
        ApplyMaterial(obstacle, obstacleMaterial);
    }

    private void SpawnRail(int lane, float z)
    {
        GameObject rail = GetPooledObject("Rail", CreateRailObject);
        rail.name = "Grind Rail";
        rail.transform.position = new Vector3(lane * laneWidth, 0.75f, z + 5.1f);
        rail.transform.rotation = Quaternion.identity;
        rail.transform.localScale = new Vector3(0.48f, 0.2f, 13.8f);
        ApplyMaterial(rail, railMaterial);
        ReservePhysicalSpace(z, 13.8f + 5.1f);
    }

    private void SpawnCoin(int lane, float z)
    {
        GameObject coin = GetPooledObject("Coin", CreateCoinObject);
        coin.name = "Coin";
        coin.transform.position = new Vector3(lane * laneWidth, 1.25f, z);
        coin.transform.rotation = Quaternion.identity;
        coin.transform.localScale = Vector3.one * 0.45f;
        ApplyMaterial(coin, coinMaterial);
    }

    private void SpawnKickerRamp(int lane, float z)
    {
        GameObject ramp = GetPooledObject("KickerRamp", CreateKickerRampObject);
        ramp.name = "Kicker Ramp";
        ramp.transform.position = new Vector3(lane * laneWidth, 0.28f, z);
        ramp.transform.rotation = Quaternion.Euler(-16f, 0f, 0f);
        ramp.transform.localScale = new Vector3(1.65f, 0.35f, 2.5f);
        SkateFeature feature = ramp.GetComponent<SkateFeature>();
        feature.Configure(SkateFeatureType.KickerRamp, 180, 10.5f);
        feature.ResetUsed();
        ApplyMaterial(ramp, featureMaterial != null ? featureMaterial : rampMaterial);

        SpawnCoin(lane, z + 2.5f);
        SpawnCoin(lane, z + 4.2f);
    }

    private void SpawnBounceStairs(int lane, float z)
    {
        GameObject root = GetPooledObject("BounceStairs", CreateBounceStairsObject);
        root.name = "Bounce Stairs";
        root.transform.position = new Vector3(lane * laneWidth, 0f, z);
        root.transform.rotation = Quaternion.identity;
        foreach (SkateFeature feature in root.GetComponentsInChildren<SkateFeature>())
        {
            feature.Configure(SkateFeatureType.StairSet, 260, 12.5f);
            feature.ResetUsed();
        }
        ApplyMaterialToChildren(root, stairMaterial != null ? stairMaterial : obstacleMaterial);

        SpawnCoin(lane, z + 3.2f);
    }

    private void SpawnManualPad(int lane, float z)
    {
        GameObject pad = GetPooledObject("ManualPad", CreateManualPadObject);
        pad.name = "Manual Pad";
        pad.transform.position = new Vector3(lane * laneWidth, 0.08f, z + 2f);
        pad.transform.rotation = Quaternion.identity;
        pad.transform.localScale = new Vector3(1.65f, 0.12f, 6.8f);
        SkateFeature feature = pad.GetComponent<SkateFeature>();
        feature.Configure(SkateFeatureType.ManualPad, 180, 0f);
        feature.ResetUsed();
        ApplyMaterial(pad, markerMaterial != null ? markerMaterial : railMaterial);

        SpawnCoin(lane, z + 1.2f);
        SpawnCoin(lane, z + 2.8f);
        SpawnCoin(lane, z + 4.4f);
        SpawnCoin(lane, z + 6.0f);
        ReservePhysicalSpace(z, 6.8f + 2f);
    }

    private void SpawnRewardOnlyRow(int lane, float z)
    {
        SpawnCoin(lane, z);
        if (Random.value > 0.45f)
        {
            SpawnCoin(Mathf.Clamp(lane + (lane <= 0 ? 1 : -1), -1, 1), z + 2.4f);
        }
    }

    private void ReservePhysicalSpace(float startZ, float featureLength)
    {
        reservedPhysicalSpawnUntilZ = Mathf.Max(
            reservedPhysicalSpawnUntilZ,
            startZ + featureLength + longFeatureSpacingBuffer);
    }

    private void SpawnNegativePickup(int lane, float z)
    {
        GameObject pickup = GetPooledObject("NegativePickup", CreateNegativePickupObject);
        pickup.name = Random.Range(0, 5) switch
        {
            0 => "Loose Gravel",
            1 => "Wet Paint",
            2 => "Sketchy Crack",
            3 => "Security Cone",
            _ => "Mud Patch"
        };
        pickup.transform.position = new Vector3(lane * laneWidth, 0.11f, z + 0.9f);
        pickup.transform.rotation = Quaternion.identity;
        pickup.transform.localScale = new Vector3(1.55f, 0.22f, 1.7f);
        SkateFeature feature = pickup.GetComponent<SkateFeature>();
        feature.Configure(SkateFeatureType.NegativePickup, 0, 0f);
        feature.ResetUsed();
        ApplyMaterial(pickup, hazardMaterial != null ? hazardMaterial : markerMaterial);
    }

    private void SpawnFinalRamp(float z)
    {
        GameObject ramp = GetPooledObject("FinalRamp", CreateFinalRampObject);
        ramp.name = "Final Bonus Mega Ramp";
        ramp.transform.position = new Vector3(0f, 0.45f, z);
        ramp.transform.rotation = Quaternion.Euler(-18f, 0f, 0f);
        ramp.transform.localScale = new Vector3(5.6f, 0.5f, 9f);
        ApplyMaterial(ramp, rampMaterial != null ? rampMaterial : obstacleMaterial);

        for (int i = 0; i < 8; i++)
        {
            SpawnCoin(0, z - 8f + i * 2f);
        }
    }

    private void SpawnBonusMarkers(float rampZ)
    {
        for (int i = 1; i <= 5; i++)
        {
            GameObject marker = GetPooledObject("BonusMarker", CreateBonusMarkerObject);
            marker.name = $"{i * 10}m Bonus Marker";
            marker.transform.position = new Vector3(0f, 0.05f, rampZ + i * 10f);
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = new Vector3(5.8f, 0.08f, 0.22f);
            ApplyMaterial(marker, markerMaterial != null ? markerMaterial : coinMaterial);
        }
    }

    private void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            ReleaseSpawnedAt(i);
        }

        spawned.Clear();
    }

    public void ReleaseToPool(PooledObject pooledObject)
    {
        if (pooledObject == null)
        {
            return;
        }

        GameObject obj = pooledObject.gameObject;
        if (!obj.activeSelf)
        {
            return;
        }

        spawned.Remove(obj);
        ReleaseObject(obj);
    }

    private GameObject GetPooledObject(string key, System.Func<GameObject> createObject)
    {
        if (!pools.TryGetValue(key, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            pools[key] = pool;
        }

        GameObject obj = pool.Count > 0 ? pool.Pop() : createObject();
        PooledObject pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = obj.AddComponent<PooledObject>();
        }

        pooledObject.ConfigurePool(this, key);
        obj.transform.SetParent(null);
        obj.SetActive(true);
        spawned.Add(obj);
        return obj;
    }

    private void ReleaseSpawnedAt(int index)
    {
        GameObject obj = spawned[index];
        spawned.RemoveAt(index);
        ReleaseObject(obj);
    }

    private void ReleaseObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        ResetPooledComponents(obj);

        if (obj.TryGetComponent(out PooledObject pooledObject) && !string.IsNullOrEmpty(pooledObject.PoolKey))
        {
            if (!pools.TryGetValue(pooledObject.PoolKey, out Stack<GameObject> pool))
            {
                pool = new Stack<GameObject>();
                pools[pooledObject.PoolKey] = pool;
            }

            obj.SetActive(false);
            obj.transform.SetParent(poolRoot);
            pool.Push(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    private static void ResetPooledComponents(GameObject obj)
    {
        foreach (RunnerObstacle obstacle in obj.GetComponentsInChildren<RunnerObstacle>(true))
        {
            obstacle.ResetHit();
        }

        foreach (SkateFeature feature in obj.GetComponentsInChildren<SkateFeature>(true))
        {
            feature.ResetUsed();
        }
    }

    private GameObject CreateObstacleObject()
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.GetComponent<BoxCollider>().isTrigger = true;
        obstacle.AddComponent<RunnerObstacle>();
        return obstacle;
    }

    private GameObject CreateRailObject()
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.GetComponent<BoxCollider>().isTrigger = true;
        rail.AddComponent<GrindRail>();
        return rail;
    }

    private GameObject CreateCoinObject()
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.GetComponent<SphereCollider>().isTrigger = true;
        coin.AddComponent<CoinPickup>();
        return coin;
    }

    private GameObject CreateKickerRampObject()
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.GetComponent<BoxCollider>().isTrigger = true;
        ramp.AddComponent<SkateFeature>();
        return ramp;
    }

    private GameObject CreateManualPadObject()
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.GetComponent<BoxCollider>().isTrigger = true;
        pad.AddComponent<SkateFeature>();
        return pad;
    }

    private GameObject CreateNegativePickupObject()
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.GetComponent<BoxCollider>().isTrigger = true;
        pickup.AddComponent<SkateFeature>();
        return pickup;
    }

    private GameObject CreateFinalRampObject()
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.GetComponent<Collider>().enabled = false;
        return ramp;
    }

    private GameObject CreateBonusMarkerObject()
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.GetComponent<Collider>().enabled = false;
        return marker;
    }

    private GameObject CreateBounceStairsObject()
    {
        GameObject root = new GameObject("Bounce Stairs");

        for (int i = 0; i < 4; i++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = $"Stair Step {i + 1}";
            step.transform.SetParent(root.transform);
            step.transform.localPosition = new Vector3(0f, 0.08f + i * 0.08f, i * 0.55f);
            step.transform.localScale = new Vector3(1.55f, 0.16f, 0.52f);
            step.GetComponent<Collider>().enabled = false;
        }

        GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trigger.name = "Bounce Stairs Trigger";
        trigger.transform.SetParent(root.transform);
        trigger.transform.localPosition = new Vector3(0f, 0.8f, 0.95f);
        trigger.transform.localScale = new Vector3(1.8f, 1.45f, 2.6f);
        trigger.GetComponent<MeshRenderer>().enabled = false;
        trigger.GetComponent<BoxCollider>().isTrigger = true;
        trigger.AddComponent<SkateFeature>();
        return root;
    }

    private static void ApplyMaterial(GameObject obj, Material material)
    {
        if (material != null && obj.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void ApplyMaterialToChildren(GameObject obj, Material material)
    {
        if (material == null)
        {
            return;
        }

        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                renderer.sharedMaterial = material;
            }
        }
    }
}
