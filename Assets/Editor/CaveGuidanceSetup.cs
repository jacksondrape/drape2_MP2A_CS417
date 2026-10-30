using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEditor.Events;

// Manual scene setup only; never runs automatically when Unity imports assets.
public static class CaveGuidanceSetup
{
    [MenuItem("Tools/MP2C/Check Portable Guide Toggle %&v")]
    public static void CheckPortableToggle()
    {
        if (!EditorApplication.isPlaying) return;
        var guide = Object.FindFirstObjectByType<CaveGuideTrail>();
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (guide == null || keyboard == null) throw new System.Exception("Guide or keyboard missing.");
        bool original = guide.showTrail;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.G));
        UnityEngine.InputSystem.InputSystem.Update();
        bool pressed = guide.showTrail != original;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState());
        UnityEngine.InputSystem.InputSystem.Update();
        bool released = guide.showTrail != original;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.G));
        UnityEngine.InputSystem.InputSystem.Update();
        bool restored = guide.showTrail == original;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState());
        UnityEngine.InputSystem.InputSystem.Update();
        Directory.CreateDirectory("Logs/Guidance");
        File.WriteAllText("Logs/Guidance/PortableToggleTest.txt", "Pressed=" + pressed + "; release does not toggle=" + released + "; second press restores=" + restored);
        if (!pressed || !released || !restored) throw new System.Exception("Portable guide toggle test failed.");
        Debug.Log("Portable guide toggle PASS: injected G press/release/press. Physical controller still needs testing.");
    }

    public static void CheckOpenRoutes(CaveDefenseProgression p)
    {
        Physics.SyncTransforms();
        foreach (var route in new[] {p.guide.roomRoute,p.guide.starterMineRoute,p.guide.mineARoute,p.guide.mineBRoute,p.guide.startingPlatformRoute})
            for (int i=1;i<route.Length;i++)
            {
                Vector3 start = route[i-1].position; start.y = 1.8f;
                Vector3 end = route[i].position; end.y = 1.8f;
                foreach (var hit in Physics.SphereCastAll(start,.15f,(end-start).normalized,Vector3.Distance(start,end),~0,QueryTriggerInteraction.Ignore))
                    if (hit.collider.name.Contains("Wall") || hit.collider.name.Contains("Panel"))
                        throw new System.Exception("Guide route blocked: " + route[i-1].name + " to " + route[i].name + " by " + hit.collider.name);
            }
    }

    public static void PreviewBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");
        var p = Object.FindFirstObjectByType<CaveDefenseProgression>();
        typeof(CaveGuideTrail).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(p.guide, null);
        Physics.SyncTransforms();
        Camera.main.tag = "Untagged";
        var camera = new GameObject("GuidancePreviewCamera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 75;
        var rt = new RenderTexture(1000,700,24);
        camera.targetTexture = rt;
        var positions = new[] { new Vector3(66.79f,2.85f,-65.51f), new Vector3(78,1.7f,-41),
            new Vector3(64,1.7f,-56), new Vector3(78,1.7f,-38) };
        var looks = new[] { new Vector3(64,1,-60), new Vector3(78,.5f,-48),
            new Vector3(64,.5f,-52), new Vector3(75,.5f,-29) };
        var targets = new[] { p.starterMine, p.starterMine, p.rapidShelf.displayPoint, p.rapidShelf.displayPoint };
        for (int i=0;i<positions.Length;i++)
        {
            camera.transform.position = positions[i];
            camera.transform.LookAt(looks[i]);
            p.guide.Draw(targets[i]);
            camera.Render();
            RenderTexture.active = rt;
            var image = new Texture2D(1000,700,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1000,700),0,0);
            image.Apply();
            File.WriteAllBytes("GuidanceView" + i + ".png", image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }
        // Check the authored routes against actual collider surfaces, not their broad bounds.
        var report = new System.Collections.Generic.List<string>();
        foreach (var route in new[] { p.guide.roomRoute, p.guide.starterMineRoute, p.guide.mineARoute, p.guide.mineBRoute })
        {
            for (int i=1;i<route.Length;i++)
            {
                var start = route[i-1].position; start.y = 1.8f;
                var end = route[i].position; end.y = 1.8f;
                foreach (var hit in Physics.RaycastAll(start, (end-start).normalized, Vector3.Distance(start,end), ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.collider.name.Contains("Platform") && !hit.collider.name.Contains("Ground"))
                        report.Add(route[i-1].name + " -> " + route[i].name + ": " + hit.collider.name + " @ " + hit.point.ToString("F2"));
            }
        }
        File.WriteAllLines("GuidanceRouteObstacles.txt", report);
        var floors = new System.Collections.Generic.List<string>();
        foreach (var position in new[] { positions[0], new Vector3(65,0,-65), new Vector3(64,0,-63), new Vector3(62.8f,0,-69.5f) })
            foreach (var hit in Physics.RaycastAll(new Vector3(position.x,2.8f,position.z), Vector3.down,5,~0,QueryTriggerInteraction.Ignore))
                floors.Add(position.ToString("F2") + " hit " + hit.collider.name + " at " + hit.point.ToString("F2") + " normal=" + hit.normal.ToString("F2"));
        foreach (var zone in new[] {p.zoneA,p.zoneB})
        {
            var data = new SerializedObject(zone);
            floors.Add(zone.name + " removes=" + data.FindProperty("originalObject").objectReferenceValue.name
                + " replaces=" + data.FindProperty("replacementPrefab").objectReferenceValue.name);
        }
        File.WriteAllLines("GuidanceFloorInspection.txt", floors);
        RenderTexture.active = null;
        camera.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camera.gameObject);
    }

    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");
        Apply();
    }

    [MenuItem("Tools/MP2C/Set Up Ground Guidance and Prices")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/MineDefense.unity") throw new System.Exception("Open MineDefense first.");
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        var p = all.Select(t => t.GetComponent<CaveDefenseProgression>()).Single(c => c != null);
        Directory.CreateDirectory("SceneBackups");
        File.Copy(scene.path, "SceneBackups/MineDefense-before-guidance-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
        Undo.RecordObject(p, "Configure guidance");
        p.zoneB.waves = p.waves;
        p.zoneB.requiredClearedWaves = 4;
        EditorUtility.SetDirty(p.zoneB);
        var shelves = all.Select(t => t.GetComponent<TowerDisplayShelf>()).Where(s => s != null).ToArray();
        foreach (var shelf in shelves)
        {
            string name = shelf.towerPrefab.name;
            int price = name.Contains("Rapid") ? 80 : name.Contains("LongRange") ? 110
                : name.Contains("Soda") ? 140 : name.Contains("Cooling") ? 100 : 180;
            int upgrade = name.Contains("Rapid") ? 120 : name.Contains("LongRange") ? 140
                : name.Contains("Soda") ? 160 : name.Contains("Cooling") ? 120 : 180;
            Undo.RecordObject(shelf, "Set tower price");
            shelf.price = price;
            if (shelf.label != null) { shelf.label.text = shelf.towerName + "\nGRAB: " + price + " GOLD"; EditorUtility.SetDirty(shelf.label); }
            EditorUtility.SetDirty(shelf);
            if (name.Contains("Rapid")) p.rapidShelf = shelf;
            string vrPath = AssetDatabase.GetAssetPath(shelf.towerPrefab);
            foreach (string path in new[] { vrPath, vrPath.Replace("VR_Room", "Room") })
            {
                GameObject prefab = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var component in prefab.GetComponents<MonoBehaviour>())
                    {
                        var data = new SerializedObject(component);
                        var cost = data.FindProperty("upgradeCost");
                        if (cost == null) continue;
                        cost.intValue = upgrade;
                        data.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(prefab); }
            }
        }
        p.toolbox = all.Select(t => t.GetComponent<ToolboxPurchase>()).Single(t => t != null);
        p.hammer = all.Select(t => t.GetComponent<VRUpgradeHammer>()).Single(t => t != null);
        p.waveButton = all.Single(t => t.name == "NextWaveButton");
        if (p.guide == null)
        {
            var root = new GameObject("GroundGuidance");
            root.transform.SetParent(p.transform, false);
            p.guide = root.AddComponent<CaveGuideTrail>();
            p.guide.zoneA = p.zoneA;
            p.guide.zoneB = p.zoneB;
            Transform hub = Point(root.transform, "CaveJunction", 62, -60);
            p.guide.roomRoute = Route(root.transform, hub, "Room", new[] {
                new Vector2(64,-56), new Vector2(64,-52), new Vector2(78,-52),
                new Vector2(78,-43), new Vector2(78,-39), new Vector2(75,-29) });
            p.guide.starterMineRoute = Route(root.transform, hub, "StarterMine", new[] {
                new Vector2(67,-60), new Vector2(71,-60), new Vector2(71,-64.5f) });
            p.guide.mineARoute = Route(root.transform, hub, "MineA", new[] {
                new Vector2(62,-66), new Vector2(62.8f,-68.3f), new Vector2(60.9f,-71),
                new Vector2(60.9f,-76), new Vector2(60,-76.5f) });
            p.guide.mineBRoute = Route(root.transform, hub, "MineB", new[] {
                new Vector2(54,-60), new Vector2(53.75f,-55.5f), new Vector2(53,-52),
                new Vector2(52,-49) });
            p.starterMine = p.guide.starterMineRoute.Last();
            p.richerMineA = p.guide.mineARoute.Last();
            p.richerMineB = p.guide.mineBRoute.Last();
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = .09f;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", new Color(.15f, .95f, 1f));
            AssetDatabase.CreateAsset(material, "Assets/Materials/GroundGuidance.mat");
            line.sharedMaterial = material;
            p.guide.line = line;
            p.guideLabel = Label("CurrentTask", root.transform, Vector3.zero, 2.2f);
            p.guideLabel.gameObject.SetActive(false);

            var button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.name = "ToggleGroundGuidance";
            button.transform.SetParent(root.transform);
            button.transform.position = p.waveButton.position + Vector3.right * 1.2f;
            button.transform.localScale = new Vector3(.32f,.15f,.32f);
            button.GetComponent<Renderer>().sharedMaterial = material;
            var interactable = button.AddComponent<XRSimpleInteractable>();
            UnityEventTools.AddVoidPersistentListener(interactable.selectEntered, p.guide.ToggleTrail);
            var label = Label("GuideToggleLabel", root.transform, button.transform.position + Vector3.up*.5f, 1.1f);
            label.text = "GROUND GUIDE\nSelect to show / hide";
            label.transform.rotation = p.waveButton.GetComponent<VRWaveButton>().label.transform.rotation;
        }
        var g = p.guide;
        if (g.startingPlatformRoute == null || g.startingPlatformRoute.Length == 0)
            g.startingPlatformRoute = new[] {g.roomRoute[0], g.starterMineRoute[1], Point(g.transform,"StartingPlatform",67,-66)};
        if (g.mineARoute.Length == 6)
            g.mineARoute = new[] {g.mineARoute[0],g.mineARoute[1],g.mineARoute[2],
                Point(g.transform,"EntranceAApproach",61.12f,-68.3f),g.mineARoute[3],g.mineARoute[4],g.mineARoute[5]};
        g.mineARoute[4].position = new Vector3(61.12f,.15f,-70.7f);
        if (g.mineBRoute.Length == 5)
            g.mineBRoute = new[] {g.mineBRoute[0],g.mineBRoute[1],g.mineBRoute[2],
                Point(g.transform,"EntranceBApproach",56.25f,-55.5f),g.mineBRoute[3],g.mineBRoute[4]};
        g.mineBRoute[4].position = new Vector3(56.25f,.15f,-52);
        p.guide.toggleLabel = p.guide.transform.Find("GuideToggleLabel").GetComponent<TMP_Text>();
        p.guide.transform.Find("ToggleGroundGuidance").gameObject.SetActive(false);
        p.guide.toggleLabel.gameObject.SetActive(false);
        EditorUtility.SetDirty(p.guide);
        EditorUtility.SetDirty(p);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        File.WriteAllText("GuidanceSetupResult.txt", "PASS: scene saved; 5 shelf prices, 10 prefab upgrade prices, B wave-4 gate, guidance references and toggle configured.");
    }

    private static Transform Point(Transform parent, string name, float x, float z)
    {
        var point = new GameObject(name).transform;
        point.SetParent(parent);
        point.position = new Vector3(x,.15f,z);
        return point;
    }

    private static Transform[] Route(Transform parent, Transform hub, string name, Vector2[] positions)
    {
        return new[] { hub }.Concat(positions.Select((v,i) => Point(parent, name + "_" + i, v.x, v.y))).ToArray();
    }

    private static TMP_Text Label(string name, Transform parent, Vector3 position, float width)
    {
        var label = new GameObject(name).AddComponent<TextMeshPro>();
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.fontSize = 1.3f;
        label.color = new Color(.65f,1f,1f);
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(width, .8f);
        return label;
    }

    public static void Inspect()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        var lines = all.Where(t => t.GetComponent<TowerDisplayShelf>() != null
            || t.GetComponent<ToolboxPlacementZone>() != null || t.GetComponent<ToolboxPurchase>() != null
            || t.GetComponent<SellStation>() != null || t.GetComponent<OreSpawnPoint>() != null
            || t.name.Contains("NextWave") || t.name.Contains("Pickaxe") || t.name.Contains("XR Origin"))
            .Select(t => t.name + " @ " + t.position.ToString("F2") + " parent=" + t.parent?.name).ToList();
        foreach (var t in all)
        {
            var shelf = t.GetComponent<TowerDisplayShelf>();
            if (shelf != null) lines.Add("SHELF " + shelf.towerName + " price=" + shelf.price + " prefab=" + AssetDatabase.GetAssetPath(shelf.towerPrefab));
        }
        File.WriteAllLines("GuidanceInspection.txt", lines);
        File.WriteAllLines("GuidanceColliders.txt", all.SelectMany(t => t.GetComponents<Collider>())
            .Where(c => c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy)
            .Select(c => c.name + " | " + c.bounds.center.ToString("F2") + " | " + c.bounds.size.ToString("F2")));
    }
}
