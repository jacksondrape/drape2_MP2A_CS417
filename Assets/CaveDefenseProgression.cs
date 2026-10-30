using System.Collections;
using TMPro;
using UnityEngine;

// Scene references only: toolbox objectives and three permanent achievement displays.
public class CaveDefenseProgression : MonoBehaviour
{
    public ToolboxPlacementZone zoneA;
    public ToolboxPlacementZone zoneB;
    public SimpleWaveController waves;
    public TowerBuildSlot[] buildSlots;
    public TMP_Text[] objectiveDisplays;
    public GameObject firstBuildTrophy;
    public GameObject firstWaveTrophy;
    public GameObject explorerTrophy;
    public GameObject traderTrophy;
    public GameObject engineerTrophy;
    public SellStation sellStation;
    public TowerDisplayShelf sodaShelf;
    public TowerDisplayShelf fanShelf;
    public TowerDisplayShelf amplifierShelf;
    public TMP_Text revealDisplay;
    public float introductionSeconds = 5f;
    private bool builtFirstTower;
    private bool upgradedTower;
    private bool sodaUnlocked;
    private bool fanUnlocked;
    private bool amplifierUnlocked;
    private bool introducing;

    private void Awake()
    {
        // Hide the complete shelf, including its price and grabbable sample.
        if (sodaShelf != null) sodaShelf.gameObject.SetActive(false);
        if (fanShelf != null) fanShelf.gameObject.SetActive(false);
        if (amplifierShelf != null) amplifierShelf.gameObject.SetActive(false);
        if (revealDisplay != null) revealDisplay.gameObject.SetActive(false);
    }

    private void Start()
    {
        firstBuildTrophy.SetActive(false);
        firstWaveTrophy.SetActive(false);
        explorerTrophy.SetActive(false);
        if (traderTrophy != null) traderTrophy.SetActive(false);
        if (engineerTrophy != null) engineerTrophy.SetActive(false);
    }

    private void Update()
    {
        foreach (TowerBuildSlot slot in buildSlots)
        {
            if (slot == null || slot.builtTower == null) continue;
            builtFirstTower = true;
            PurchasedTowerGrab tower = slot.builtTower.GetComponent<PurchasedTowerGrab>();
            if (tower != null && tower.GetLevel() > 0) upgradedTower = true;
        }

        firstBuildTrophy.SetActive(builtFirstTower);
        firstWaveTrophy.SetActive(waves.clearedWaves > 0);
        explorerTrophy.SetActive(zoneA.IsUnlocked && zoneB.IsUnlocked);
        int minedGold = sellStation != null ? sellStation.totalGoldEarned : 0;
        if (traderTrophy != null) traderTrophy.SetActive(minedGold >= 100);
        if (engineerTrophy != null) engineerTrophy.SetActive(upgradedTower);

        // One introduction at a time, even if several conditions complete together.
        if (!introducing && sodaShelf != null && !sodaUnlocked && minedGold >= 50)
            StartCoroutine(Introduce(sodaShelf, "SODA SPRAYER\nMining funded a pressure sprayer!\nAim its spray across the rat path for area damage."));
        else if (!introducing && fanShelf != null && !fanUnlocked && waves.clearedWaves >= 1)
            StartCoroutine(Introduce(fanShelf, "COOLING FAN\nYour first defense earned a workshop fan!\nPlace it near attack machines to help them fire faster."));
        else if (!introducing && amplifierShelf != null && !amplifierUnlocked && waves.clearedWaves >= 3)
            StartCoroutine(Introduce(amplifierShelf, "AMPLIFIER\nThree defenses earned a power amplifier!\nIt charges, then boosts nearby attack machines' damage."));

        string goal = !zoneA.IsUnlocked
            ? "NEXT: Open mining route A\nBuy a toolbox for 50 Gold.\nRelease it in zone A: " + zoneA.CurrentPlacements + "/1."
            : !zoneB.IsUnlocked
            ? "NEW GOAL: Open mining route B\nDeliver 3 purchased toolboxes to zone B.\nProgress: " + zoneB.CurrentPlacements + "/3."
            : "BOTH MINING ROUTES OPEN\nExplore, sell ore, and strengthen your defenses.";
        if (!sodaUnlocked) goal += "\nWORKSHOP: Sell ore worth 50 Gold (" + minedGold + "/50).";
        else if (!fanUnlocked) goal += "\nNEXT WORKSHOP GOAL: Clear your first wave.";
        else if (!amplifierUnlocked) goal += "\nNEXT WORKSHOP GOAL: Clear 3 waves (" + waves.clearedWaves + "/3).";
        foreach (TMP_Text display in objectiveDisplays)
            if (display != null) display.text = "<b>MINE & DEFEND</b>\n" + goal;
    }

    private IEnumerator Introduce(TowerDisplayShelf shelf, string message)
    {
        introducing = true;
        if (revealDisplay != null)
        {
            revealDisplay.text = message;
            revealDisplay.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(introductionSeconds);
            revealDisplay.gameObject.SetActive(false);
        }
        shelf.gameObject.SetActive(true);
        if (shelf == sodaShelf) sodaUnlocked = true;
        if (shelf == fanShelf) fanUnlocked = true;
        if (shelf == amplifierShelf) amplifierUnlocked = true;
        introducing = false;
    }

    private void LateUpdate()
    {
        // Keep announcements near the player, including when they are in the cave.
        if (revealDisplay == null || !revealDisplay.gameObject.activeSelf || Camera.main == null) return;
        Transform view = Camera.main.transform;
        revealDisplay.transform.SetPositionAndRotation(view.position + view.forward * 1.6f - view.up * 0.25f, view.rotation);
    }
}
