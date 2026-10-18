using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SyncVRWorkshopModels
{
    private const string Marker = "VRModelSyncOctober15";
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/RoomScaleDefense.unity");
        if (scene.isLoaded && !scene.GetRootGameObjects().Any(r => r.name == Marker)) Sync();
    }

    [MenuItem("Tools/MP2a/Sync Tower Models To VR Displays")]
    public static void Sync()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/RoomScaleDefense.unity");
        if (!scene.isLoaded) { Debug.LogWarning("Open RoomScaleDefense before syncing displays."); return; }
        TowerDisplayShelf[] shelves = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TowerDisplayShelf>(true)).ToArray();
        string[] names = { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower", "RoomCoolingFan", "RoomDamageAmplifier" };
        // Capture the bindings before replacing prefab contents, then rebind the new component IDs.
        string[] paths = shelves.Select(s => AssetDatabase.GetAssetPath(s.towerPrefab)).ToArray();
        foreach (string name in names)
        {
            PurchasedTowerGrab prefab = SetupVRWallWorkshop.CreateTower(name, true);
            string path = "Assets/VR_" + name + ".prefab";
            for (int i = 0; i < shelves.Length; i++)
                if (paths[i] == path) { shelves[i].towerPrefab = prefab; EditorUtility.SetDirty(shelves[i]); }
        }
        foreach (VRWaveButton button in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<VRWaveButton>(true)))
        {
            // Keep the user's location/rotation. The collider travels with the button.
            if (button.waves == null) button.waves = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SimpleWaveController>(true)).FirstOrDefault();
            if (button.colliders.Count == 0 && button.GetComponent<Collider>() != null) button.colliders.Add(button.GetComponent<Collider>());
            EditorUtility.SetDirty(button);
        }
        if (!scene.GetRootGameObjects().Any(r => r.name == Marker))
            SceneManager.MoveGameObjectToScene(new GameObject(Marker), scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Five VR display prefabs synced from source models. Workshop, dumpster and button transforms preserved.");
    }
}
