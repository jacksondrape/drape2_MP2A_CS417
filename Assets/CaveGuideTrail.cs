using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// Four hand-placed walks from the cave junction. This only draws directions;
// it never moves the player or changes which passages are open.
public class CaveGuideTrail : MonoBehaviour
{
    public LineRenderer line;
    public Transform[] roomRoute;
    public Transform[] starterMineRoute;
    public Transform[] mineARoute;
    public Transform[] mineBRoute;
    public Transform[] startingPlatformRoute;
    public ToolboxPlacementZone zoneA;
    public ToolboxPlacementZone zoneB;
    public int entranceAIndex = 2;
    public int entranceBIndex = 2;
    public bool showTrail = true;
    public TMP_Text toggleLabel;
    private readonly List<Vector3> points = new List<Vector3>();
    private Transform[][] routes;
    private InputAction toggleInput;

    private void Awake()
    {
        routes = new[] { roomRoute, starterMineRoute, mineARoute, mineBRoute, startingPlatformRoute };
        toggleInput = new InputAction("Toggle ground guide", InputActionType.Button,
            "<XRController>{LeftHand}/secondaryButton");
        toggleInput.AddBinding("<Keyboard>/g");
        toggleInput.performed += OnTogglePressed;
        // Retire the old room-only button, including in scenes already configured.
        Transform oldButton = transform.Find("ToggleGroundGuidance");
        if (oldButton != null) oldButton.gameObject.SetActive(false);
        if (toggleLabel != null) toggleLabel.gameObject.SetActive(false);
    }

    private void OnEnable() { toggleInput?.Enable(); }
    private void OnDisable() { toggleInput?.Disable(); }
    private void OnDestroy() { toggleInput?.Dispose(); }
    private void OnTogglePressed(InputAction.CallbackContext context) { ToggleTrail(); }

    public void ToggleTrail() { showTrail = !showTrail; }

    public void Draw(Transform target)
    {
        if (toggleLabel != null && Camera.main != null)
        {
            toggleLabel.transform.rotation = Camera.main.transform.rotation;
            toggleLabel.text = "GROUND GUIDE: " + (showTrail ? "ON" : "OFF") + "\nSelect to " + (showTrail ? "hide" : "show");
        }
        if (line == null) return;
        line.enabled = showTrail && target != null && Camera.main != null;
        if (!line.enabled) return;
        Vector3 start = Camera.main.transform.position;
        Vector3 finish = target.position;
        FindNearest(start, out int startRoute, out float startStep, out Vector3 startJoin);
        FindNearest(finish, out int endRoute, out float endStep, out Vector3 endJoin);
        // The starting platform and ore walk share their first leg. Turn at
        // their shared corner instead of sending the player back to the junction.
        int sharedCorner = 0;
        while (sharedCorner + 1 < routes[startRoute].Length && sharedCorner + 1 < routes[endRoute].Length
            && Vector3.Distance(routes[startRoute][sharedCorner + 1].position, routes[endRoute][sharedCorner + 1].position) < .01f)
            sharedCorner++;
        if (startStep <= sharedCorner) startRoute = endRoute;
        if (endStep <= sharedCorner) endRoute = startRoute;
        points.Clear();
        AddPoint(start);
        AddPoint(startJoin);
        if (startRoute == endRoute)
        {
            if (startStep <= endStep)
                for (int i = Mathf.FloorToInt(startStep) + 1; i <= Mathf.FloorToInt(endStep); i++)
                    AddPoint(routes[startRoute][i].position);
            else
                for (int i = Mathf.CeilToInt(startStep) - 1; i >= Mathf.CeilToInt(endStep); i--)
                    AddPoint(routes[startRoute][i].position);
        }
        else
        {
            for (int i = Mathf.FloorToInt(startStep); i >= sharedCorner; i--) AddPoint(routes[startRoute][i].position);
            for (int i = sharedCorner + 1; i <= Mathf.FloorToInt(endStep); i++) AddPoint(routes[endRoute][i].position);
        }
        AddPoint(endJoin);
        AddPoint(finish);
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    private void FindNearest(Vector3 point, out int route, out float step, out Vector3 join)
    {
        route = 0; step = 0; join = roomRoute[0].position;
        float nearest = float.PositiveInfinity;
        point.y = 0;
        for (int r = 0; r < routes.Length; r++)
        {
            int last = routes[r].Length - 1;
            if (r == 2 && !zoneA.IsUnlocked) last = entranceAIndex;
            if (r == 3 && !zoneB.IsUnlocked) last = entranceBIndex;
            for (int i = 0; i < last; i++)
            {
                Vector3 a = routes[r][i].position; a.y = 0;
                Vector3 b = routes[r][i + 1].position; b.y = 0;
                Vector3 delta = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(point - a, delta) / Mathf.Max(.001f, delta.sqrMagnitude));
                Vector3 projected = a + delta * t;
                float distance = (point - projected).sqrMagnitude;
                if (distance >= nearest) continue;
                nearest = distance; route = r; step = i + t; join = projected;
            }
        }
    }

    private void AddPoint(Vector3 point)
    {
        // Sample the uneven mine floor so the trail does not disappear underground.
        Vector3 previous = points.Count > 0 ? points[points.Count - 1] : point;
        float distance = Vector2.Distance(new Vector2(previous.x, previous.z), new Vector2(point.x, point.z));
        int samples = Mathf.Max(1, Mathf.CeilToInt(distance / .4f));
        for (int i = 1; i <= samples; i++)
        {
            Vector3 sample = Vector3.Lerp(previous, point, (float)i / samples);
            sample.y = .15f;
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(sample.x, 2.8f, sample.z), Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
            {
                string floorName = hit.collider.name;
                if (hit.normal.y < .6f || !(floorName.Contains("Ground") || floorName.Contains("Floor") || floorName.Contains("Platform"))) continue;
                sample.y = Mathf.Max(sample.y, hit.point.y + .07f);
            }
            points.Add(sample);
        }
    }
}
