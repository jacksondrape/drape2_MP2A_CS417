using UnityEngine;
using TMPro;

public class TowerSpeedUpgrade : MonoBehaviour
{
    public TowerShooting shooting;
    public PlayerResources resources;
    public TowerBuildSlot buildSlot;
    public Renderer upgradePanel;
    public int upgradeCost = 15;
    public int maxUpgrades = 2;
    public float fireIntervalMultiplier = 0.7f;
    public int upgradeLevel;
    public float damagePerUpgrade = 1f;
    public TMP_Text statusLabel;
    public string machineName = "MACHINE";
    public int tutorialIndex;
    private float tutorialTime;
    private Camera viewCamera;
    private bool hovered;

    public int NextCost => upgradeCost * (upgradeLevel + 1);

    public void SetHovered(bool value, DesktopBuildInput input)
    {
        hovered = value;
        if (value && input != null)
        {
            viewCamera = input.viewCamera;
            if (tutorialIndex >= 0 && tutorialIndex < input.upgradeTutorialShown.Length
                && !input.upgradeTutorialShown[tutorialIndex])
            {
                input.upgradeTutorialShown[tutorialIndex] = true;
                tutorialTime = 7f;
            }
        }
    }

    private void LateUpdate()
    {
        if (statusLabel == null) return;
        tutorialTime -= Time.unscaledDeltaTime;
        statusLabel.gameObject.SetActive(hovered || tutorialTime > 0f);
        if (!statusLabel.gameObject.activeSelf) return;
        if (viewCamera != null) statusLabel.transform.rotation = viewCamera.transform.rotation;
        statusLabel.text = "<b>" + machineName + "  " + upgradeLevel + "/" + maxUpgrades + "</b>"
            + "\nDMG " + shooting.EffectiveDamage.ToString("0.#") + "  |  " + shooting.EffectiveInterval.ToString("0.00") + "s / shot";
        if (shooting.cooling != null && shooting.cooling.speedMultiplier > 1f)
            statusLabel.text += "\n<color=#BDEEFF>SPEED +" + ((shooting.cooling.speedMultiplier - 1f) * 100f).ToString("0") + "%</color>";
        if (shooting.damageBoost != null && shooting.damageBoost.Multiplier > 1f)
            statusLabel.text += "\n<color=#FFD580>DMG x" + shooting.damageBoost.Multiplier.ToString("0.00") + "  " + shooting.damageBoost.Remaining.ToString("0.0") + "s</color>";
        statusLabel.text += upgradeLevel < maxUpgrades ? "\nE Upgrade · " + NextCost + " SCRAP" : "\nMAX LEVEL";
        if (tutorialTime > 0f) statusLabel.text += "\nUpgrade: more damage, faster shots.";

    }

    public string Upgrade()
    {
        if (shooting == null || resources == null)
            return "Tower upgrade references are missing.";
        if (upgradeLevel >= maxUpgrades)
            return "This tower is fully upgraded.";
        int cost = NextCost;
        if (!resources.TrySpendGold(cost))
            return "Not enough SCRAP. Need " + cost + ".";

        shooting.fireInterval *= fireIntervalMultiplier;
        shooting.bonusDamage += damagePerUpgrade;
        upgradeLevel++;

        if (upgradePanel != null)
        {
            MaterialPropertyBlock colors = new MaterialPropertyBlock();
            Color color = upgradeLevel >= maxUpgrades ? Color.cyan : Color.yellow;
            colors.SetColor("_BaseColor", color);
            colors.SetColor("_Color", color);
            upgradePanel.SetPropertyBlock(colors);
        }

        return "Machine upgraded to level " + upgradeLevel + ". -" + cost + " SCRAP.";
    }
}
