using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Manual art tools only; no runtime behavior or automatic scene changes.
public static class CaveTowerVisuals
{
    [MenuItem("Tools/MP2B/Replace Attack Tower Visuals %&l")]
    public static void ApplyAttackModels()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory("ArtReview");
        Directory.CreateDirectory("Assets/Materials/CaveTurrets");
        AssetDatabase.Refresh();
        string[] towers = { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower" };
        string[] models = {
            "Assets/Low Poly Sci-Fi Turrets Demo/Prefabs/RapidFireCannon_1.prefab",
            "Assets/BlenderToRender/Sci-Fi_Turret_02/PreFabs/Sci-Fi_Turret_2_Yellow.prefab",
            "Assets/Low Poly Sci-Fi Turrets Demo/Prefabs/RapidFireCannon_3.prefab"
        };
        foreach (string prefix in new[] { "", "VR_" })
        for (int i = 0; i < towers.Length; i++)
        {
            string path = "Assets/" + prefix + towers[i] + ".prefab";
            string backup = "ArtReview/" + prefix + towers[i] + "-before-turrets.prefab.txt";
            if (!File.Exists(backup)) File.Copy(path, backup);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponentsInChildren<Transform>(true).Any(t => t.name == "CaveTurretArt")) continue;
                int colliders = root.GetComponentsInChildren<Collider>(true).Length;
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                    if (renderer.GetComponent<TMPro.TMP_Text>() == null) renderer.enabled = false;

                var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(models[i]), root.transform);
                model.name = "CaveTurretArt";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                // Import only artwork, never demo scripts, physics, or animation controllers.
                foreach (var component in model.GetComponentsInChildren<Component>(true))
                    if (!(component is Transform) && !(component is MeshFilter) && !(component is MeshRenderer))
                        Object.DestroyImmediate(component);
                var renderers = model.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(CaveMaterial).ToArray();
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var grabBounds = root.GetComponent<BoxCollider>().bounds;
                float scale = Mathf.Min(grabBounds.size.y * .95f / bounds.size.y,
                    grabBounds.size.x * .95f / bounds.size.x, grabBounds.size.z * .95f / bounds.size.z);
                model.transform.localScale = Vector3.one * scale;
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                model.transform.position += Vector3.up * (grabBounds.min.y - bounds.min.y);

                var shooting = root.GetComponent<TowerShooting>();
                Transform barrel = model.GetComponentsInChildren<Transform>().First(t => t.name == (i == 1 ? "Barrel.L" : "Head"));
                if (shooting != null)
                {
                    // Keep the base stationary; move only the imported head onto the existing yaw pivot.
                    foreach (Transform part in model.transform.Cast<Transform>().ToArray())
                        if (i != 1 || part.name != "SquareBase.L") part.SetParent(shooting.aimPivot, true);
                    shooting.firePoint.position = MuzzlePosition(barrel);
                    shooting.firePoint.rotation = root.transform.rotation;
                    if (shooting.muzzleFlash != null)
                    {
                        shooting.muzzleFlash.transform.SetParent(shooting.firePoint, false);
                        shooting.muzzleFlash.transform.localPosition = Vector3.zero;
                        shooting.muzzleFlash.transform.localRotation = Quaternion.identity;
                    }
                }
                else
                {
                    var soda = root.GetComponent<SodaTower>();
                    soda.spray.transform.position = MuzzlePosition(barrel);
                    soda.spray.transform.rotation = root.transform.rotation;
                }
                root.GetComponent<MachineFeedback>().flashRenderers = renderers;
                var upgrade = root.GetComponent<TowerSpeedUpgrade>();
                if (upgrade != null) upgrade.upgradePanel = renderers[0];
                if (colliders != root.GetComponentsInChildren<Collider>(true).Length)
                    throw new System.Exception("Unexpected collider change during visual replacement.");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Render(AssetDatabase.LoadAssetAtPath<GameObject>(path), "ArtReview/" + prefix + towers[i] + "-After.png");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("ATTACK TOWER ART UPDATED: rapid cannon, industrial long-range turret, triple-nozzle sprayer. Fan untouched; source and VR prefabs updated.");
    }

    private static Vector3 MuzzlePosition(Transform barrel)
    {
        var points = barrel.GetComponentsInChildren<MeshFilter>()
            .SelectMany(f => f.sharedMesh.vertices.Select(v => f.transform.TransformPoint(v))).ToArray();
        float front = points.Max(v => v.z);
        float depth = front - points.Min(v => v.z);
        var rim = points.Where(v => v.z >= front - depth * .025f).ToArray();
        return new Vector3(rim.Average(v => v.x), rim.Average(v => v.y), front + .02f);
    }

    private static Material CaveMaterial(Material source)
    {
        if (source == null || source.shader.name != "Standard") return source;
        string path = "Assets/Materials/CaveTurrets/" + source.name + "_" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)).Substring(0, 8) + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetTexture("_BaseMap", source.mainTexture);
        material.SetColor("_BaseColor", source.color);
        material.SetFloat("_Metallic", source.GetFloat("_Metallic"));
        material.SetFloat("_Smoothness", .35f);
        if (source.GetTexture("_BumpMap") != null)
        {
            material.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            material.EnableKeyword("_NORMALMAP");
        }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
    public const string Crystal = "Assets/FinottiGames/LowPoly Crystals/Prefabs/Small_Cluster/LP_Crystal_Cluster_Small_01_Cyan.prefab";

    [MenuItem("Tools/MP2B/Preview Cave Tower Assets")]
    public static void Review()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory("ArtReview");
        string[] paths = {
            "Assets/VR_RoomDamageAmplifier.prefab",
            "Assets/Low Poly Sci-Fi Turrets Demo/Prefabs/RapidFireCannon_1.prefab",
            "Assets/Low Poly Sci-Fi Turrets Demo/Prefabs/RapidFireCannon_3.prefab",
            "Assets/BlenderToRender/Sci-Fi_Turret_02/PreFabs/Sci-Fi_Turret_2_Yellow.prefab", Crystal
        };
        var report = new System.Text.StringBuilder();
        for (int i = 0; i < paths.Length; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            report.AppendLine(paths[i]);
            foreach (var t in asset.GetComponentsInChildren<Transform>(true))
                report.AppendLine(t.name + " position=" + t.localPosition + " scale=" + t.localScale);
            Render(asset, "ArtReview/Asset" + i + ".png");
        }
        File.WriteAllText("ArtReview/Hierarchy.txt", report.ToString());
        Debug.Log("ART PREVIEWS READY in ArtReview; imported shaders adapted on preview copies only.");
    }

    public static void Render(GameObject asset, string path)
    {
        var preview = new PreviewRenderUtility();
        var temporaryMaterials = new System.Collections.Generic.List<Material>();
        try
        {
            var copy = Object.Instantiate(asset);
            preview.AddSingleGO(copy);
            copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (var text in copy.GetComponentsInChildren<TMPro.TMP_Text>(true)) text.gameObject.SetActive(false);
            foreach (var particles in copy.GetComponentsInChildren<ParticleSystem>(true)) particles.gameObject.SetActive(false);
            foreach (var line in copy.GetComponentsInChildren<LineRenderer>(true)) line.gameObject.SetActive(false);
            var renderers = copy.GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled).ToArray();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                var materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    if (materials[m] == null || materials[m].shader.name != "Standard") continue;
                    var source = materials[m];
                    var converted = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    converted.SetTexture("_BaseMap", source.mainTexture);
                    converted.SetColor("_BaseColor", source.color);
                    materials[m] = converted; temporaryMaterials.Add(converted);
                }
                renderer.sharedMaterials = materials;
            }
            preview.camera.transform.position = bounds.center + new Vector3(1f, .65f, 1.4f).normalized * bounds.size.magnitude * 1.5f;
            preview.camera.transform.LookAt(bounds.center);
            preview.camera.nearClipPlane = .01f;
            preview.camera.farClipPlane = bounds.size.magnitude * 10f + 100;
            preview.camera.fieldOfView = 35f;
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = new Color(.08f, .10f, .13f);
            preview.lights[0].intensity = 2f;
            preview.lights[0].transform.rotation = Quaternion.Euler(40, 30, 0);
            preview.lights[1].intensity = 1.2f;
            preview.ambientColor = Color.gray;
            preview.BeginStaticPreview(new Rect(0, 0, 640, 640));
            preview.Render(true);
            var image = preview.EndStaticPreview();
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }
        finally
        {
            preview.Cleanup();
            foreach (var material in temporaryMaterials) Object.DestroyImmediate(material);
        }
    }

    [MenuItem("Tools/MP2B/Replace Amplifier Cans With Crystals %&o")]
    public static void ApplyCrystals()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory("ArtReview");
        foreach (string path in new[] { "Assets/RoomDamageAmplifier.prefab", "Assets/VR_RoomDamageAmplifier.prefab" })
        {
            string backup = "ArtReview/" + Path.GetFileNameWithoutExtension(path) + "-before-crystals.prefab.txt";
            if (!File.Exists(backup)) File.Copy(path, backup);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
                string before = string.Join("\n", scripts.Select(s => EditorJsonUtility.ToJson(s)));
                int colliderCount = root.GetComponentsInChildren<Collider>(true).Length;
                var mounts = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name == "Prefab_SodaCan_1Mount" || t.name == "Prefab_SodaCan_2Mount").ToArray();
                foreach (var mount in mounts)
                {
                    var oldRenderers = mount.GetComponentsInChildren<MeshRenderer>();
                    var target = oldRenderers[0].bounds;
                    foreach (var r in oldRenderers) target.Encapsulate(r.bounds);
                    var source = AssetDatabase.LoadAssetAtPath<GameObject>(Crystal);
                    var core = new GameObject(mount.name.Contains("_1") ? "CrystalCoreLeft" : "CrystalCoreRight");
                    core.transform.SetParent(mount.parent, false);
                    core.AddComponent<MeshFilter>().sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
                    var renderer = core.AddComponent<MeshRenderer>();
                    renderer.sharedMaterials = source.GetComponent<MeshRenderer>().sharedMaterials;
                    Vector3 size = renderer.bounds.size;
                    core.transform.localScale = new Vector3(target.size.x / size.x, target.size.y / size.y, target.size.z / size.z);
                    core.transform.position += target.center - renderer.bounds.center;
                    Object.DestroyImmediate(mount.gameObject);
                }
                string after = string.Join("\n", scripts.Select(s => EditorJsonUtility.ToJson(s)));
                if (before != after || colliderCount != root.GetComponentsInChildren<Collider>(true).Length)
                    throw new System.Exception("Art swap unexpectedly changed gameplay references or colliders.");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Render(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VR_RoomDamageAmplifier.prefab"), "ArtReview/AmplifierAfter.png");
        Debug.Log("AMPLIFIER ART UPDATED: two crystal cores; gameplay references, colliders, charging arrows and effects preserved. Source and VR shelf prefab updated.");
    }
}
