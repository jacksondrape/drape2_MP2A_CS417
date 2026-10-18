using UnityEngine;

public class TowerDamageBoost : MonoBehaviour
{
    public DamageAmplifier source;
    public GameObject yellowMarker;
    public LineRenderer powerLink;
    public bool preview;
    private float refresh;

    public float Multiplier => source != null && source.isActiveAndEnabled && source.activeSeconds > 0f
        && source.Covers(transform.position) ? source.damageMultiplier : 1f;
    public float Remaining => Multiplier > 1f ? source.activeSeconds : 0f;

    public void RefreshBoost()
    {
        source = null;
        foreach (DamageAmplifier amplifier in FindObjectsByType<DamageAmplifier>())
            Consider(amplifier);
    }

    public void Consider(DamageAmplifier amplifier)
    {
        if (amplifier == null || !amplifier.isActiveAndEnabled || amplifier.activeSeconds <= 0f || !amplifier.Covers(transform.position)) return;
        if (source == null || amplifier.damageMultiplier > Multiplier
            || (Mathf.Approximately(amplifier.damageMultiplier, Multiplier) && amplifier.activeSeconds > source.activeSeconds))
            source = amplifier;
    }

    private void Update()
    {
        refresh -= Time.unscaledDeltaTime;
        if (refresh <= 0f)
        {
            refresh = 0.1f;
            RefreshBoost();
        }
    }

    private void LateUpdate()
    {
        if (yellowMarker != null) yellowMarker.SetActive(preview || Multiplier > 1f);
        if (powerLink != null)
        {
            powerLink.enabled = Multiplier > 1f;
            if (powerLink.enabled)
            {
                powerLink.SetPosition(0, source.transform.position + Vector3.up * 0.65f);
                powerLink.SetPosition(1, transform.position + Vector3.up * 0.8f);
            }
        }
    }
}
