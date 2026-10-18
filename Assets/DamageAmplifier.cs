using TMPro;
using UnityEngine;

public class DamageAmplifier : MonoBehaviour
{
    public SimpleWaveController waves;
    private void Start()
    {
        if (waves == null) waves = FindAnyObjectByType<SimpleWaveController>();
    }

    public TowerTargeting targeting;
    public TowerBuildSlot buildSlot;
    public PlayerResources resources;
    public Transform chargeBar;
    public LineRenderer pulse;
    public TMP_Text label;
    public float chargeSeconds = 6f;
    public float extraSecondsPerTower = 2f;
    public int connectedTowers;
    public float RequiredChargeSeconds => chargeSeconds + Mathf.Max(0, connectedTowers - 1) * extraSecondsPerTower;
    public float duration = 4f;
    public float damageMultiplier = 1.5f;
    public float charge;
    public float activeSeconds;
    public int level;
    public int maxLevel = 2;
    public int upgradeCost = 15;
    private bool hovered;
    private float tutorialTime;
    private Camera cameraView;
    private float scanTimer;

    public void RefreshConnections()
    {
        connectedTowers = 0;
        foreach (TowerDamageBoost tower in FindObjectsByType<TowerDamageBoost>())
            if (tower.isActiveAndEnabled && Covers(tower.transform.position)) connectedTowers++;
        if (connectedTowers == 0) charge = 0f;
    }

    public bool Covers(Vector3 point)
    {
        Vector3 delta = point - transform.position;
        delta.y = 0f;
        return targeting != null && delta.sqrMagnitude <= targeting.range * targeting.range;
    }

    // Separate from input and visuals so the charge cycle is easy to check.
    public void AdvanceCharge(float seconds)
    {
        if (seconds <= 0f) return;
        if (activeSeconds > 0f)
        {
            activeSeconds = Mathf.Max(0f, activeSeconds - seconds);
            return;
        }
        if (connectedTowers == 0) return;
        charge += seconds;
        if (charge >= RequiredChargeSeconds)
        {
            charge = 0f;
            activeSeconds = duration;
        }
    }

    public string Upgrade()
    {
        if (level >= maxLevel) return "Amplifier at maximum level.";
        int cost = upgradeCost * (level + 1);
        if (resources == null || !resources.TrySpendGold(cost)) return "Need " + cost + " SCRAP.";
        level++;
        damageMultiplier += 0.25f;
        return "Amplifier upgraded: damage x" + damageMultiplier.ToString("0.00") + ".";
    }

    public void SetHovered(bool value, DesktopBuildInput input)
    {
        hovered = value;
        if (!value) return;
        cameraView = input.viewCamera;
        if (!input.amplifierTutorialShown)
        {
            input.amplifierTutorialShown = true;
            tutorialTime = 7f;
        }
    }

    private void Update()
    {
        scanTimer -= Time.unscaledDeltaTime;
        if (scanTimer <= 0f)
        {
            RefreshConnections();
            scanTimer = 0.1f;
        }
        if (waves == null || waves.CombatActive) AdvanceCharge(Time.deltaTime);
        if (chargeBar != null)
        {
            float fill = Mathf.Clamp01(activeSeconds > 0f ? activeSeconds / duration : charge / RequiredChargeSeconds);
            chargeBar.localScale = new Vector3(0.18f, Mathf.Max(0.01f, fill), 0.18f);
            chargeBar.localPosition = new Vector3(0f, -0.5f + fill * 0.5f, -0.43f);
        }
        if (pulse != null)
        {
            float age = duration - activeSeconds;
            pulse.enabled = activeSeconds > 0f && age < 0.6f;
            if (pulse.enabled)
                pulse.transform.localScale = Vector3.one * targeting.range * Mathf.Clamp01(age / 0.6f);
        }
        if (label == null) return;
        tutorialTime -= Time.unscaledDeltaTime;
        label.gameObject.SetActive(hovered || tutorialTime > 0f);
        if (!label.gameObject.activeSelf) return;
        if (cameraView != null) label.transform.rotation = cameraView.transform.rotation;
        label.text = "<b>AMPLIFIER  " + level + "/" + maxLevel + "</b>"
            + "\nDMG x" + damageMultiplier.ToString("0.00") + "  |  " + connectedTowers + " towers"
            + (activeSeconds > 0f ? "\n<color=#FFD580>BOOST " + activeSeconds.ToString("0.0") + "s</color>"
                : connectedTowers == 0 ? "\nWaiting for nearby towers"
                : "\nCHARGE " + charge.ToString("0.0") + " / " + RequiredChargeSeconds.ToString("0") + "s")
            + (level < maxLevel ? "\nE Upgrade · " + upgradeCost * (level + 1) + " SCRAP" : "\nMAX LEVEL");
        if (tutorialTime > 0f) label.text += "\nMore towers: slower charge. Strongest boost only.";

    }
}


