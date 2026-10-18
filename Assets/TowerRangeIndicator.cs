using UnityEngine;

public class TowerRangeIndicator : MonoBehaviour
{
    public TowerTargeting targeting;
    public LineRenderer circle;
    public Material lineMaterial;
    public bool showRange = true;
    public float coneAngle = 360f;
    public int segments = 48;
    public float lineWidth = 0.035f;
    public Color lineColor = new Color(1f, 0.85f, 0.15f, 0.8f);

    private void Start()
    {
        if (targeting == null)
            targeting = GetComponent<TowerTargeting>();
        if (circle == null)
            circle = gameObject.AddComponent<LineRenderer>();

        circle.loop = true;
        circle.useWorldSpace = true;
        circle.positionCount = segments;
        circle.startWidth = lineWidth;
        circle.endWidth = lineWidth;
        circle.startColor = lineColor;
        circle.endColor = lineColor;
        circle.material = lineMaterial != null
            ? lineMaterial
            : new Material(Shader.Find("Sprites/Default"));
    }

    private void LateUpdate()
    {
        if (circle == null || targeting == null)
            return;

        circle.enabled = showRange;
        if (!showRange)
            return;

        Vector3 center = transform.position;
        center.y = 0.09f;
        if (coneAngle < 360f)
        {
            circle.positionCount = segments + 2;
            circle.SetPosition(0, center);
            for (int i = 0; i <= segments; i++)
            {
                float angle = -coneAngle * 0.5f + coneAngle * i / segments;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
                circle.SetPosition(i + 1, center + direction * targeting.range);
            }
            return;
        }
        circle.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            circle.SetPosition(i, center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * targeting.range);
        }
    }
}
