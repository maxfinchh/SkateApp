#if UNITY_EDITOR
using UnityEditor;
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

        GameObject systems = new GameObject("Systems");
        GestureInput gestureInput = systems.AddComponent<GestureInput>();
        SkateRunnerGameManager manager = systems.AddComponent<SkateRunnerGameManager>();
        ObstacleSpawner spawner = systems.AddComponent<ObstacleSpawner>();

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
        playerSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSo = new SerializedObject(manager);
        managerSo.FindProperty("player").objectReferenceValue = playerController;
        managerSo.FindProperty("spawner").objectReferenceValue = spawner;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("player").objectReferenceValue = player.transform;
        spawnerSo.FindProperty("obstacleMaterial").objectReferenceValue = obstacle;
        spawnerSo.FindProperty("railMaterial").objectReferenceValue = rail;
        spawnerSo.FindProperty("coinMaterial").objectReferenceValue = coin;
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

    private static void CreateGround(Material asphalt, Material lanePaint)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.position = new Vector3(0f, -0.08f, 70f);
        road.transform.localScale = new Vector3(8f, 0.1f, 180f);
        road.GetComponent<Renderer>().sharedMaterial = asphalt;

        for (int lane = -1; lane <= 1; lane++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"Lane Center {lane}";
            stripe.transform.position = new Vector3(lane * 2.2f, 0.01f, 70f);
            stripe.transform.localScale = new Vector3(0.06f, 0.04f, 180f);
            stripe.GetComponent<Renderer>().sharedMaterial = lanePaint;
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
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
