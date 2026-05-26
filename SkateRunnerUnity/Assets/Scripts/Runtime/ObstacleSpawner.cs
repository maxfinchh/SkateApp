using System.Collections.Generic;
using UnityEngine;

public sealed class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float laneWidth = 2.2f;
    [SerializeField] private float spawnAhead = 55f;
    [SerializeField] private float spawnStep = 8f;
    [SerializeField] private Material obstacleMaterial;
    [SerializeField] private Material railMaterial;
    [SerializeField] private Material coinMaterial;

    private readonly List<GameObject> spawned = new();
    private float nextSpawnZ;

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
    }

    private void Update()
    {
        if (player == null)
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
            if (spawned[i] == null || spawned[i].transform.position.z < player.position.z - 20f)
            {
                if (spawned[i] != null)
                {
                    Destroy(spawned[i]);
                }

                spawned.RemoveAt(i);
            }
        }
    }

    public void ResetRun()
    {
        foreach (GameObject obj in spawned)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawned.Clear();
        nextSpawnZ = 18f;
    }

    private void SpawnRow(float z)
    {
        int pattern = Random.Range(0, 4);
        int lane = Random.Range(-1, 2);

        if (pattern == 0)
        {
            SpawnObstacle(lane, z);
            SpawnCoin(-lane, z + 2f);
        }
        else if (pattern == 1)
        {
            SpawnRail(lane, z);
        }
        else
        {
            SpawnCoin(lane, z);
            SpawnCoin(Mathf.Clamp(lane + 1, -1, 1), z + 2.5f);
        }
    }

    private void SpawnObstacle(int lane, float z)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Construction Barrier";
        obstacle.transform.position = new Vector3(lane * laneWidth, 0.55f, z);
        obstacle.transform.localScale = new Vector3(1.5f, 1.1f, 0.7f);
        obstacle.AddComponent<RunnerObstacle>();
        ApplyMaterial(obstacle, obstacleMaterial);
        spawned.Add(obstacle);
    }

    private void SpawnRail(int lane, float z)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Grind Rail";
        rail.transform.position = new Vector3(lane * laneWidth, 0.75f, z + 2f);
        rail.transform.localScale = new Vector3(0.35f, 0.18f, 5.5f);
        BoxCollider collider = rail.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        rail.AddComponent<GrindRail>();
        ApplyMaterial(rail, railMaterial);
        spawned.Add(rail);
    }

    private void SpawnCoin(int lane, float z)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Coin";
        coin.transform.position = new Vector3(lane * laneWidth, 1.25f, z);
        coin.transform.localScale = Vector3.one * 0.45f;
        SphereCollider collider = coin.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        coin.AddComponent<CoinPickup>();
        ApplyMaterial(coin, coinMaterial);
        spawned.Add(coin);
    }

    private static void ApplyMaterial(GameObject obj, Material material)
    {
        if (material != null && obj.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = material;
        }
    }
}
