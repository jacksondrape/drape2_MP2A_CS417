using TMPro;
using UnityEngine;

public class SodaTower : MonoBehaviour
{
    public SimpleWaveController waves;
    private void Start()
    {
        if (waves == null) waves = FindAnyObjectByType<SimpleWaveController>();
    }

    public TowerTargeting targeting;
    public TowerBuildSlot buildSlot;
    public PlayerResources resources;
    public ParticleSystem spray;
    public TMP_Text label;
    public float coneAngle = 60f;
    public float damagePerSecond = 2f;
    public float damagePerUpgrade = 1f;
    public float damageInterval = 0.2f;
    public TowerCooling cooling;
    public TowerDamageBoost damageBoost;
    public float EffectiveDps => damagePerSecond * (cooling != null ? cooling.speedMultiplier : 1f)
        * (damageBoost != null ? damageBoost.Multiplier : 1f);
    public int level;
    public int maxLevel = 2;
    public int upgradeCost = 15;
    private float cooldown;
    private bool hovered;
    private float tutorialTime;
    private Camera viewCamera;

    public void RotateQuarterTurn() { SetFacing(transform.eulerAngles.y + 90f); }

    // VR can supply any yaw angle without using the desktop key.
    public void SetFacing(float yaw)
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cooldown = 0f;
        StopSpray();
    }

    public bool ContainsPoint(Vector3 point)
    {
        Vector3 offset = point - transform.position;
        offset.y = 0f;
        return targeting != null && offset.sqrMagnitude <= targeting.range * targeting.range
            && (offset.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, offset) <= coneAngle * 0.5f);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || (waves != null && !waves.CombatActive)) { StopSpray(); return; }
        cooldown -= Time.deltaTime * (cooling != null ? cooling.speedMultiplier : 1f);
        if (cooldown > 0f) return;
        cooldown = damageInterval;
        bool hit = false;
        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>())
        {
            if (!enemy.isActiveAndEnabled || enemy.health <= 0f || !ContainsPoint(enemy.transform.position)) continue;
            enemy.TakeDamage(damagePerSecond * damageInterval * (damageBoost != null ? damageBoost.Multiplier : 1f));
            hit = true;
        }
        if (spray == null) return;
        if (hit && !spray.isPlaying) spray.Play();
        if (!hit) StopSpray();
    }

    private void StopSpray()
    {
        if (spray != null && spray.isPlaying)
            spray.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDisable() { StopSpray(); }

    public string Upgrade()
    {
        if (level >= maxLevel) return "Soda sprayer at max level.";
        int cost = upgradeCost * (level + 1);
        if (resources == null || !resources.TrySpendGold(cost)) return "Need " + cost + " SCRAP.";
        level++;
        damagePerSecond += damagePerUpgrade;
        return "Soda DPS: " + damagePerSecond + ". -" + cost + " SCRAP.";
    }

    public void SetHovered(bool value, DesktopBuildInput input)
    {
        hovered = value;
        if (!value) return;
        viewCamera = input.viewCamera;
        if (!input.sodaTutorialShown)
        {
            input.sodaTutorialShown = true;
            tutorialTime = 7f;
        }
    }

    private void LateUpdate()
    {
        TowerRangeIndicator indicator = GetComponent<TowerRangeIndicator>();
        if (indicator != null) indicator.coneAngle = coneAngle;
        if (label == null) return;
        tutorialTime -= Time.unscaledDeltaTime;
        label.gameObject.SetActive(hovered || tutorialTime > 0f);
        if (!label.gameObject.activeSelf) return;
        if (viewCamera != null) label.transform.rotation = viewCamera.transform.rotation;
        label.text = "<b>SODA  " + level + "/" + maxLevel + "</b>"
            + "\nDPS " + EffectiveDps.ToString("0.0") + "  |  R Rotate";
        if (cooling != null && cooling.speedMultiplier > 1f) label.text += "\n<color=#BDEEFF>SPEED BOOST</color>";
        if (damageBoost != null && damageBoost.Multiplier > 1f)
            label.text += "\n<color=#FFD580>DMG x" + damageBoost.Multiplier.ToString("0.00") + "  " + damageBoost.Remaining.ToString("0.0") + "s</color>";
        label.text += level < maxLevel ? "\nE Upgrade · " + upgradeCost * (level + 1) + " SCRAP" : "\nMAX LEVEL";
        if (tutorialTime > 0f) label.text += "\nSprays all rats in the forward cone.";

    }
}


