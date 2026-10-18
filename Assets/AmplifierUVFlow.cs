using UnityEngine;

public class AmplifierUVFlow : MonoBehaviour
{
    public DamageAmplifier amplifier;
    public Renderer[] panels;
    public float minimumSpeed = 0.15f;
    public float maximumSpeed = 0.8f;
    public float releaseSpeed = 1.4f;
    private Material material;
    private float offset;

    private void Start()
    {
        if (panels == null || panels.Length == 0 || panels[0] == null) return;
        material = new Material(panels[0].sharedMaterial);
        foreach (Renderer panel in panels)
            if (panel != null) panel.sharedMaterial = material;
    }

    private void LateUpdate()
    {
        if (material == null || amplifier == null) return;
        bool releasing = amplifier.activeSeconds > 0f;
        bool connected = amplifier.connectedTowers > 0;
        bool running = amplifier.waves == null || amplifier.waves.CombatActive;
        float progress = Mathf.Clamp01(amplifier.charge / Mathf.Max(0.01f, amplifier.RequiredChargeSeconds));
        float speed = releasing ? releaseSpeed : connected ? Mathf.Lerp(minimumSpeed, maximumSpeed, progress) : 0f;
        if (running) offset = Mathf.Repeat(offset - speed * Time.deltaTime, 1f);
        material.SetTextureOffset("_BaseMap", new Vector2(0f, offset));
        Color color = releasing ? new Color(0.35f, 1f, 1f)
            : connected ? Color.Lerp(new Color(0.5f, 0.24f, 0.02f), new Color(1f, 0.85f, 0.2f), progress)
            : new Color(0.12f, 0.12f, 0.12f);
        material.SetColor("_BaseColor", color);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}
