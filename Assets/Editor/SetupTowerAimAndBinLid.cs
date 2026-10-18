using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupTowerAimAndBinLid
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Tower Aim and Automatic Bin Lid")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (string prefix in new[] { "", "VR_" })
        {
            Aim("Assets/" + prefix + "RoomRapidTower.prefab", "CapLauncherVisual");
            Aim("Assets/" + prefix + "RoomLongRangeTower.prefab", "ScrapMachineVisual");
        }
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/RoomScaleDefense.unity");
        if (!scene.isLoaded) return;
        VRRecycleBin bin = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<VRRecycleBin>(true)).FirstOrDefault();
        if (bin == null) return;
        Transform root = bin.transform.parent;
        Transform model = root.Find("DumpsterVisual");
        if (model == null) return;
        DumpsterLid controller = root.GetComponent<DumpsterLid>();
        // Older setup stored identical sampled poses and included mesh objects.
        if (controller != null && controller.lids.Length == 2
            && Quaternion.Angle(controller.closedRotations[0], controller.openRotations[0]) > 30f) return;
        Transform armature = model.Find("LidArmature.R");
        if (armature == null) return;
        Transform[] lids = { armature.Find("Lid.L"), armature.Find("Lid.R") };
        if (lids.Any(t => t == null)) return;
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuksGames/VTDumpster/URP/Prefab/VTDumpster_01am_green.prefab");
        Transform sourceArmature = source.transform.Find("LidArmature.R");
        if (sourceArmature == null) return;
        Quaternion[] closed = lids.Select(t => sourceArmature.Find(t.name).localRotation).ToArray();
        // The lids extend along local +Z from their rear hinge. Negative X lifts them.
        Quaternion[] opened = closed.Select(q => q * Quaternion.Euler(-100f, 0f, 0f)).ToArray();
        foreach (Animator animator in model.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (Animation animation in model.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
        foreach (SkinnedMeshRenderer mesh in model.GetComponentsInChildren<SkinnedMeshRenderer>(true)) mesh.updateWhenOffscreen = true;
        if (controller == null) controller = root.gameObject.AddComponent<DumpsterLid>();
        controller.manualPreview = false;
        controller.lids = lids;
        controller.closedRotations = closed;
        controller.openRotations = opened;
        for (int i = 0; i < lids.Length; i++) lids[i].localRotation = closed[i];
        // The previously static imported lid must now be movable.
        foreach (Transform part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.isStatic = false;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Projectile towers now aim their visual pivot; dumpster opens within 3m and closes beyond 3.5m.");
    }

    private static void Aim(string path, string visualName)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            TowerShooting shooting = root.GetComponent<TowerShooting>();
            if (shooting == null || shooting.aimPivot != null) return;
            Transform visual = root.transform.Find(visualName);
            if (visual == null || shooting.firePoint == null) return;
            Transform pivot = new GameObject("AimPivot").transform;
            pivot.SetParent(root.transform, false);
            visual.SetParent(pivot, true);
            shooting.firePoint.SetParent(pivot, true);
            foreach (Transform part in visual.GetComponentsInChildren<Transform>(true)) part.gameObject.isStatic = false;
            shooting.aimPivot = pivot;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
