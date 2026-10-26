using TMPro;
using UnityEngine;

// Only the combined scene uses this component. References are assigned in the Inspector.
public class CaveDefenseProgression : MonoBehaviour
{
    public SimpleWaveController waves;
    public TMP_Text[] displays;
    public float revealSeconds = 6f;
    public bool machinesUnlocked;
    public bool pickaxeUpgraded;
    public bool rareOresUnlocked;
    public int clearedWaves;
    public bool firstSale;
    public bool rareOreCollected;

    private int pendingReward;
    private float revealUntil;
    private string announcement;
    private string error;
    private float errorUntil;

    public void OreSold(int gold)
    {
        if (gold <= 0)
        {
            ShowError("Nothing to sell. Strike a cave ore with the pickaxe, then select the sell cart again.");
            return;
        }
        firstSale = true;
    }

    public void WaveCleared(int waveNumber)
    {
        clearedWaves = Mathf.Max(clearedWaves, waveNumber);
    }

    public void OreCollected(OreType type)
    {
        if (rareOresUnlocked && (type == OreType.Diamond || type == OreType.Rayronic))
            rareOreCollected = true;
    }

    public void ShowError(string message)
    {
        error = message;
        errorUntil = Time.unscaledTime + 7f;
    }

    private void Update()
    {
        if (pendingReward != 0 && Time.unscaledTime >= revealUntil)
        {
            if (pendingReward == 1) machinesUnlocked = true;
            if (pendingReward == 2) pickaxeUpgraded = true;
            if (pendingReward == 3) rareOresUnlocked = true;
            pendingReward = 0;
        }

        if (pendingReward == 0)
        {
            if (firstSale && !machinesUnlocked)
                Reveal(1, "WORKSHOP LICENSE EARNED\nMachine shelves open shortly. Grab to buy; release on a green cell.");
            else if (machinesUnlocked && clearedWaves >= 1 && !pickaxeUpgraded)
                Reveal(2, "REINFORCED PICKAXE EARNED\nYour pickaxe will mine two ore units per strike, with the same total yield per rock.");
            else if (pickaxeUpgraded && clearedWaves >= 3 && !rareOresUnlocked)
                Reveal(3, "RICH SEAM DISCOVERED\nDiamond and Rayronic will join the cave's ore spawns. Mine and sell them for more Gold.");
        }

        string goal = !machinesUnlocked ? "NEXT: Mine ore and sell it at the cart."
            : !pickaxeUpgraded ? "NEXT: Build defenses and clear wave 1."
            : !rareOresUnlocked ? "NEXT: Clear wave 3 to discover richer ore."
            : "ALL REWARDS UNLOCKED\nMine, sell, upgrade and defend the remaining waves.";
        string text = "<b>MINE & DEFEND</b>\n" + (pendingReward != 0 ? announcement : goal)
            + "\n\n" + Badge(firstSale, "FIRST SALE") + "  " + Badge(clearedWaves >= 1, "FIRST DEFENSE")
            + "\n" + Badge(rareOreCollected, "RARE FIND");
        if (Time.unscaledTime < errorUntil) text += "\n<color=#FFCF85>" + error + "</color>";
        foreach (TMP_Text display in displays)
            if (display != null) display.text = text;
    }

    private void Reveal(int reward, string message)
    {
        pendingReward = reward;
        announcement = message;
        revealUntil = Time.unscaledTime + revealSeconds;
    }

    private string Badge(bool earned, string title)
    {
        // Hide future achievements so the board only reveals the next goal.
        return earned ? "<color=#FFD45C>[★ " + title + "]</color>" : "";
    }
}
