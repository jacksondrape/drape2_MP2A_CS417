using TMPro;
using UnityEngine;

public class CoolingFan : MonoBehaviour
{
    public TowerTargeting targeting;
    public TowerBuildSlot buildSlot;
    public PlayerResources resources;
    public Transform blades;
    public Transform head;
    public Vector3 bladeAxis = Vector3.forward;
    public float bladeDegreesPerSecond = 540f;
    public float swingAngle = 35f;
    public float swingPeriod = 5f;
    private Quaternion headRestRotation;
    private float swingTime;

    private void Start()
    {
        if (head != null) headRestRotation = head.localRotation;
    }
    public TMP_Text label;
    public float speedMultiplier = 1.25f;
    public int level;
    public int maxLevel = 2;
    public int upgradeCost = 15;
    private bool hovered;
    private float tutorialTime;
    private Camera cameraView;
    private float countTimer;
    private int suppliedTowers;

    public bool Covers(Vector3 position)
    {
        Vector3 delta = position - transform.position;
        delta.y = 0f;
        return targeting != null && delta.sqrMagnitude <= targeting.range * targeting.range;
    }

    public void SetHovered(bool value, DesktopBuildInput input)
    {
        hovered = value;
        if (!value) return;
        cameraView = input.viewCamera;
        if (!input.fanTutorialShown)
        {
            input.fanTutorialShown = true;
            tutorialTime = 7f;
        }
    }

    public string Upgrade()
    {
        if (level >= maxLevel) return "Fan at maximum level.";
        int cost = upgradeCost * (level + 1);
        if (resources == null || !resources.TrySpendGold(cost)) return "Need " + cost + " SCRAP.";
        level++;
        speedMultiplier += 0.1f;
        return "Fan upgraded: +" + ((speedMultiplier - 1f) * 100f).ToString("0") + "% attack speed.";
    }

    private void Update()
    {
        if (blades != null) blades.Rotate(bladeAxis, bladeDegreesPerSecond * Time.deltaTime, Space.Self);
        if (head != null)
        {
            swingTime += Time.deltaTime;
            float angle = Mathf.Sin(swingTime * Mathf.PI * 2f / Mathf.Max(0.1f, swingPeriod)) * swingAngle;
            head.localRotation = headRestRotation * Quaternion.Euler(0f, angle, 0f);
        }
        countTimer -= Time.unscaledDeltaTime;
        if (countTimer <= 0f)
        {
            countTimer = 0.2f;
            suppliedTowers = 0;
            foreach (TowerCooling receiver in FindObjectsByType<TowerCooling>())
                if (receiver.source == this) suppliedTowers++;
        }
        if (label == null) return;
        tutorialTime -= Time.unscaledDeltaTime;
        label.gameObject.SetActive(hovered || tutorialTime > 0f);
        if (!label.gameObject.activeSelf) return;
        if (cameraView != null) label.transform.rotation = cameraView.transform.rotation;
        label.text = "<b>FAN  " + level + "/" + maxLevel + "</b>"
            + "\nSPEED +" + ((speedMultiplier - 1f) * 100f).ToString("0") + "%  |  " + suppliedTowers + " towers";
        label.text += level < maxLevel ? "\nE Upgrade · " + upgradeCost * (level + 1) + " SCRAP" : "\nMAX LEVEL";
        if (tutorialTime > 0f) label.text += "\nBoosts nearby attackers. Strongest fan only.";

    }
}
