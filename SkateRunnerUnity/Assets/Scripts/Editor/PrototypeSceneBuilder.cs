#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrototypeSceneBuilder
{
    [MenuItem("Skate Runner/Build Prototype Scene")]
    public static void BuildPrototypeScene()
    {
        EnsureProjectFolder("Assets/Materials");
        EnsureProjectFolder("Assets/Scenes");

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material asphalt = CreateMaterial("Asphalt", new Color(0.08f, 0.085f, 0.09f));
        Material lanePaint = CreateMaterial("Lane Paint", new Color(0.9f, 0.88f, 0.68f));
        Material board = CreateMaterial("Board", new Color(0.05f, 0.2f, 0.95f));
        Material skater = CreateMaterial("Skater", new Color(0.98f, 0.42f, 0.16f));
        Material obstacle = CreateMaterial("Obstacle", new Color(0.95f, 0.72f, 0.1f));
        Material rail = CreateMaterial("Rail", new Color(0.66f, 0.72f, 0.78f));
        Material coin = CreateMaterial("Coin", new Color(1f, 0.76f, 0.1f));
        Material ramp = CreateMaterial("Final Ramp", new Color(0.05f, 0.45f, 0.95f));
        Material marker = CreateMaterial("Bonus Marker", new Color(0.2f, 1f, 0.65f));
        Material feature = CreateMaterial("Feature Ramp", new Color(0.18f, 0.72f, 1f));
        Material stair = CreateMaterial("Stair Concrete", new Color(0.48f, 0.5f, 0.54f));
        Material hazard = CreateMaterial("Wet Concrete Hazard", new Color(0.32f, 0.9f, 0.88f));

        GameObject systems = new GameObject("Systems");
        GestureInput gestureInput = systems.AddComponent<GestureInput>();
        MissionTracker missionTracker = systems.AddComponent<MissionTracker>();
        PlayerUpgrades upgrades = systems.AddComponent<PlayerUpgrades>();
        ChaserPressure chaser = systems.AddComponent<ChaserPressure>();
        SkateRunnerGameManager manager = systems.AddComponent<SkateRunnerGameManager>();
        HUDController hud = systems.AddComponent<HUDController>();
        ObstacleSpawner spawner = systems.AddComponent<ObstacleSpawner>();

        SerializedObject gestureSo = new SerializedObject(gestureInput);
        gestureSo.FindProperty("minSwipePixels").floatValue = 60f;
        gestureSo.FindProperty("holdSeconds").floatValue = 0.18f;
        gestureSo.FindProperty("diagonalBias").floatValue = 0.45f;
        gestureSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject chaserSo = new SerializedObject(chaser);
        chaserSo.FindProperty("mistakeWindowSeconds").floatValue = 15f;
        chaserSo.FindProperty("mistakesToFail").intValue = 2;
        chaserSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject player = new GameObject("Skater Player");
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        PlayerController playerController = player.AddComponent<PlayerController>();

        GameObject trickRoot = new GameObject("Trick Root");
        trickRoot.transform.SetParent(player.transform);
        trickRoot.transform.localPosition = Vector3.zero;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Placeholder Skater";
        body.transform.SetParent(trickRoot.transform);
        body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        body.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
        body.GetComponent<Collider>().enabled = false;
        body.GetComponent<Renderer>().sharedMaterial = skater;

        GameObject boardVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boardVisual.name = "Placeholder Skateboard";
        boardVisual.transform.SetParent(trickRoot.transform);
        boardVisual.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        boardVisual.transform.localScale = new Vector3(0.55f, 0.12f, 1.25f);
        boardVisual.GetComponent<Collider>().enabled = false;
        boardVisual.GetComponent<Renderer>().sharedMaterial = board;

        SerializedObject playerSo = new SerializedObject(playerController);
        playerSo.FindProperty("gestureInput").objectReferenceValue = gestureInput;
        playerSo.FindProperty("gameManager").objectReferenceValue = manager;
        playerSo.FindProperty("trickRoot").objectReferenceValue = trickRoot.transform;
        playerSo.FindProperty("boardVisual").objectReferenceValue = boardVisual.transform;
        playerSo.FindProperty("finalBonusSpeedMultiplier").floatValue = 1.35f;
        playerSo.FindProperty("finalLaunchVelocity").floatValue = 36f;
        playerSo.FindProperty("finalTrickBoost").floatValue = 2.8f;
        playerSo.FindProperty("finalTrickForwardBoost").floatValue = 0.75f;
        playerSo.FindProperty("finalTrickCooldownSeconds").floatValue = 0.12f;
        playerSo.FindProperty("minimumFinalAirSeconds").floatValue = 3f;
        playerSo.FindProperty("queuedGrindSeconds").floatValue = 0.45f;
        playerSo.FindProperty("railBumpCooldownSeconds").floatValue = 0.85f;
        playerSo.FindProperty("railTrickApproachDistance").floatValue = 17f;
        playerSo.FindProperty("railTrickBehindDistance").floatValue = 5f;
        playerSo.FindProperty("manualPadMinEntryVelocity").floatValue = 1.2f;
        playerSo.FindProperty("manualMissCooldownSeconds").floatValue = 15f;
        playerSo.FindProperty("negativePickupCooldownSeconds").floatValue = 0.75f;
        playerSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSo = new SerializedObject(manager);
        managerSo.FindProperty("player").objectReferenceValue = playerController;
        managerSo.FindProperty("spawner").objectReferenceValue = spawner;
        managerSo.FindProperty("missionTracker").objectReferenceValue = missionTracker;
        managerSo.FindProperty("upgrades").objectReferenceValue = upgrades;
        managerSo.FindProperty("chaser").objectReferenceValue = chaser;
        managerSo.FindProperty("finalBonusTimeoutSeconds").floatValue = 30f;
        managerSo.FindProperty("gameOverRestartDelay").floatValue = 1.35f;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject hudSo = new SerializedObject(hud);
        hudSo.FindProperty("gameManager").objectReferenceValue = manager;
        hudSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("player").objectReferenceValue = player.transform;
        spawnerSo.FindProperty("obstacleMaterial").objectReferenceValue = obstacle;
        spawnerSo.FindProperty("railMaterial").objectReferenceValue = rail;
        spawnerSo.FindProperty("coinMaterial").objectReferenceValue = coin;
        spawnerSo.FindProperty("rampMaterial").objectReferenceValue = ramp;
        spawnerSo.FindProperty("markerMaterial").objectReferenceValue = marker;
        spawnerSo.FindProperty("featureMaterial").objectReferenceValue = feature;
        spawnerSo.FindProperty("stairMaterial").objectReferenceValue = stair;
        spawnerSo.FindProperty("hazardMaterial").objectReferenceValue = hazard;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();

        CreateGround(asphalt, lanePaint);
        CreateCamera(player.transform);
        CreateLight();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Prototype.unity");
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = player;
        Debug.Log("Skate Runner prototype scene built at Assets/Scenes/Prototype.unity");
    }

    [MenuItem("Skate Runner/Configure iOS Prototype Settings")]
    public static void ConfigureIosPrototypeSettings()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.applicationIdentifier = "com.maxfinch.skaterunner";
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity", true)
        };

        BuildTarget target = BuildTarget.iOS;
        BuildTargetGroup group = BuildTargetGroup.iOS;
        EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        Debug.Log("Skate Runner iOS settings configured. Use Build Profiles/File > Build to create the Xcode project.");
    }

    private static void CreateGround(Material asphalt, Material lanePaint)
    {
        const float segmentLength = 48f;
        const int segmentCount = 6;

        for (int segment = 0; segment < segmentCount; segment++)
        {
            GameObject root = new GameObject($"Road Segment {segment + 1}");
            root.transform.position = new Vector3(0f, 0f, segment * segmentLength + segmentLength * 0.5f);

            RoadSegmentLooper looper = root.AddComponent<RoadSegmentLooper>();
            SerializedObject looperSo = new SerializedObject(looper);
            looperSo.FindProperty("segmentLength").floatValue = segmentLength;
            looperSo.FindProperty("segmentCount").intValue = segmentCount;
            looperSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(root.transform);
            road.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            road.transform.localScale = new Vector3(8f, 0.1f, segmentLength);
            road.GetComponent<Renderer>().sharedMaterial = asphalt;

            for (int lane = -1; lane <= 1; lane++)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = $"Lane Center {lane}";
                stripe.transform.SetParent(root.transform);
                stripe.transform.localPosition = new Vector3(lane * 2.2f, 0.01f, 0f);
                stripe.transform.localScale = new Vector3(0.06f, 0.04f, segmentLength);
                stripe.GetComponent<Renderer>().sharedMaterial = lanePaint;
                Object.DestroyImmediate(stripe.GetComponent<Collider>());
            }
        }
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraObj = new GameObject("Follow Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        cameraObj.AddComponent<AudioListener>();
        FollowCamera followCamera = cameraObj.AddComponent<FollowCamera>();

        SerializedObject cameraSo = new SerializedObject(followCamera);
        cameraSo.FindProperty("target").objectReferenceValue = player;
        cameraSo.ApplyModifiedPropertiesWithoutUndo();

        cameraObj.transform.position = new Vector3(0f, 5.5f, -8.5f);
        cameraObj.transform.LookAt(player.position + Vector3.up);
    }

    private static void CreateLight()
    {
        GameObject lightObj = new GameObject("Sun");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        EnsureProjectFolder("Assets/Materials");

        string path = $"Assets/Materials/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        return material;
    }

    private static void EnsureProjectFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
