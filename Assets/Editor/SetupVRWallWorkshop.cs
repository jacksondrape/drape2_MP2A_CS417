using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class SetupVRWallWorkshop
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup VR Wall Workshop")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        string[] names = { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower", "RoomCoolingFan", "RoomDamageAmplifier" };
        string[] titles = { "BOTTLE CAP", "SPRING PUNCH", "SODA SPRAYER", "COOLING FAN", "AMPLIFIER" };
        PurchasedTowerGrab[] prefabs = names.Select(n => CreateTower(n)).ToArray();
        PrepareScrap();
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Transform[] all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            if (all.Any(t => t.name == "VRWallWorkshop")) return;
            Transform board = all.Single(t => t.name == "ScoreboardPanel");
            PlayerResources resources = all.Select(t => t.GetComponent<PlayerResources>()).First(r => r != null);
            GameObject group = new GameObject("VRWallWorkshop");
            group.transform.position = new Vector3(board.position.x, 0f, board.position.z - 1.2f);
            for (int i = 0; i < names.Length; i++)
            {
                TowerDisplayShelf shelf = i == 0 ? all.Select(t => t.GetComponent<TowerDisplayShelf>()).FirstOrDefault(s => s != null) : null;
                if (shelf == null) shelf = new GameObject("Display_" + titles[i]).AddComponent<TowerDisplayShelf>();
                shelf.transform.SetParent(group.transform, false);
                shelf.transform.localPosition = new Vector3((i - 2) * 1.5f, 0f, 0f);
                shelf.towerName = titles[i];
                shelf.resources = resources;
                shelf.towerPrefab = prefabs[i];
                if (shelf.displayPoint == null)
                {
                    shelf.displayPoint = new GameObject("DisplayPoint").transform;
                    shelf.displayPoint.SetParent(shelf.transform, false);
                    shelf.displayPoint.localPosition = Vector3.up * 1.48f;
                    Box("Stand", shelf.transform, new Vector3(0f, 0.37f, 0f), new Vector3(1.1f, 0.74f, 0.9f));
                }
                if (shelf.label == null) shelf.label = Label("Price", shelf.transform, new Vector3(0f, 0.95f, -0.65f));
                shelf.label.rectTransform.sizeDelta = new Vector2(1.4f, 1f);
                shelf.label.fontSize = 0.9f;
                shelf.label.transform.localPosition = new Vector3(0f, 0.95f, -0.65f);
            }
            CreateBin(group.transform, new Vector3(4.7f, 0f, 0f));
            CreateHammer(group.transform, new Vector3(-4.7f, 1.15f, 0f));
            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.name = "NextWaveButton";
            button.transform.SetParent(group.transform, false);
            button.transform.localPosition = new Vector3(0f, 0.8f, -0.85f);
            button.transform.localScale = new Vector3(0.35f, 0.14f, 0.25f);
            button.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BaseGreen.mat");
            VRWaveButton waveButton = button.AddComponent<VRWaveButton>();
            waveButton.waves = all.Select(t => t.GetComponent<SimpleWaveController>()).First(w => w != null);
            waveButton.label = Label("WaveButtonHint", group.transform, new Vector3(0f, 1.2f, -0.95f));
            foreach (DesktopBuildInput input in all.Select(t => t.GetComponent<DesktopBuildInput>()).Where(i => i != null))
                input.enableMouseBuilding = false;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Wall workshop ready: five paid grab displays, loose scrap recycling, machine recycling and upgrade hammer.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    public static PurchasedTowerGrab CreateTower(string name, bool refresh = false)
    {
        string path = "Assets/VR_" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null && !refresh) return existing.GetComponent<PurchasedTowerGrab>();
        GameObject root = PrefabUtility.LoadPrefabContents("Assets/" + name + ".prefab");
        try
        {
            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null) body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.mass = 1f;
            PurchasedTowerGrab grab = root.GetComponent<PurchasedTowerGrab>();
            if (grab == null) grab = root.AddComponent<PurchasedTowerGrab>();
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.useDynamicAttach = true;
            grab.throwOnDetach = false;
            grab.colliders.Clear();
            grab.colliders.Add(root.GetComponent<BoxCollider>());
            grab.vrStatus = Label("VRStatus", root.transform, new Vector3(0f, 1.6f, 0f));
            grab.vrStatus.rectTransform.sizeDelta = new Vector2(3.2f, 2f);
            grab.vrStatus.gameObject.SetActive(false);
            return PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<PurchasedTowerGrab>();
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void PrepareScrap()
    {
        const string path = "Assets/ScrapCan.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.GetComponent<XRGrabInteractable>() != null) return;
            XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.useDynamicAttach = true;
            grab.throwOnDetach = true;
            grab.colliders.Clear();
            grab.colliders.Add(root.GetComponent<BoxCollider>());
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void CreateBin(Transform parent, Vector3 position)
    {
        Transform bin = new GameObject("RecyclingBin").transform;
        bin.SetParent(parent, false);
        bin.localPosition = position;
        Box("Bottom", bin, new Vector3(0f, 0.05f, 0f), new Vector3(1.1f, 0.1f, 1.1f));
        Box("Left", bin, new Vector3(-0.55f, 0.5f, 0f), new Vector3(0.08f, 1f, 1.1f));
        Box("Right", bin, new Vector3(0.55f, 0.5f, 0f), new Vector3(0.08f, 1f, 1.1f));
        Box("Back", bin, new Vector3(0f, 0.5f, 0.55f), new Vector3(1.1f, 1f, 0.08f));
        Box("Front", bin, new Vector3(0f, 0.5f, -0.55f), new Vector3(1.1f, 1f, 0.08f));
        GameObject mouth = new GameObject("RecycleTrigger");
        mouth.transform.SetParent(bin, false);
        mouth.transform.localPosition = Vector3.up * 0.8f;
        BoxCollider trigger = mouth.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(0.95f, 0.65f, 0.95f);
        VRRecycleBin recycling = mouth.AddComponent<VRRecycleBin>();
        recycling.label = Label("RecycleHint", bin, new Vector3(0f, 1.5f, -0.1f));
    }

    private static void CreateHammer(Transform parent, Vector3 position)
    {
        Box("HammerStand", parent, position - Vector3.up * 0.8f, new Vector3(1.1f, 0.7f, 0.8f));
        GameObject root = new GameObject("UpgradeHammer");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = position;
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fantasy_Warhammer/Prefab/warhammer_final_LP 1.prefab");
        if (asset == null) throw new System.InvalidOperationException("Imported hammer prefab not found.");
        GameObject model = Object.Instantiate(asset, root.transform);
        model.name = "HammerVisual";
        foreach (Collider c in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        foreach (Rigidbody b in model.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(b);
        foreach (MonoBehaviour script in model.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        if (bounds.size.x > bounds.size.y && bounds.size.x > bounds.size.z) model.transform.Rotate(0f, 0f, 90f, Space.World);
        else if (bounds.size.z > bounds.size.y) model.transform.Rotate(90f, 0f, 0f, Space.World);
        bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        model.transform.localScale *= 0.8f / Mathf.Max(0.01f, bounds.size.y);
        bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        model.transform.position += root.transform.position - bounds.center;
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = false;
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        BoxCollider handle = root.AddComponent<BoxCollider>();
        handle.center = new Vector3(0f, -0.15f, 0f);
        handle.size = new Vector3(0.12f, 0.5f, 0.12f);
        XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        Transform attach = new GameObject("GripPoint").transform;
        attach.SetParent(root.transform, false);
        attach.localPosition = new Vector3(0f, -0.2f, 0f);
        grab.attachTransform = attach;
        grab.colliders.Clear();
        grab.colliders.Add(handle);
        GameObject head = new GameObject("UpgradeHitHead");
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = Vector3.up * 0.28f;
        BoxCollider hit = head.AddComponent<BoxCollider>();
        hit.isTrigger = true;
        hit.size = new Vector3(0.45f, 0.25f, 0.3f);
        VRUpgradeHammer hammer = head.AddComponent<VRUpgradeHammer>();
        hammer.grab = grab;
        hammer.label = Label("HammerHint", root.transform, Vector3.up * 0.65f);
    }

    private static TMP_Text Label(string name, Transform parent, Vector3 position)
    {
        TextMeshPro label = new GameObject(name).AddComponent<TextMeshPro>();
        label.transform.SetParent(parent, false);
        label.transform.localPosition = position;
        label.fontSize = 1.1f;
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(2.4f, 1f);
        return label;
    }

    private static void Box(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TableFrame.mat");
    }
}
