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
    [Header("Optional ground guidance")]
    public CaveGuideTrail guide;
    public TowerDisplayShelf rapidShelf;
    public ToolboxPurchase toolbox;
    public VRUpgradeHammer hammer;
    public Transform starterMine;
    public Transform richerMineA;
    public Transform richerMineB;
    public Transform waveButton;
    public TMP_Text guideLabel;
    public string CurrentInstruction { get; private set; }
    public Transform CurrentGuideTarget { get; private set; }
    private float nextGuideUpdate;
    private int lastClearedWave;
    private int strengthAtWaveEnd;
    private bool preparedNextWave;
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
            ? "NEW GOAL: Open mining route B\nClear " + zoneB.requiredClearedWaves + " waves (" + waves.clearedWaves + "/" + zoneB.requiredClearedWaves
                + "). Then deliver 3 toolboxes: " + zoneB.CurrentPlacements + "/3."
            : "BOTH MINING ROUTES OPEN\nExplore, sell ore, and strengthen your defenses.";
        if (!sodaUnlocked) goal += "\nWORKSHOP: Sell ore worth 50 Gold (" + minedGold + "/50).";
        else if (!fanUnlocked) goal += "\nNEXT WORKSHOP GOAL: Clear your first wave.";
        else if (!amplifierUnlocked) goal += "\nNEXT WORKSHOP GOAL: Clear 3 waves (" + waves.clearedWaves + "/3).";
        if (guide != null && Time.unscaledTime >= nextGuideUpdate)
        {
            nextGuideUpdate = Time.unscaledTime + .25f;
            UpdateGuidance();
            guide.Draw(introducing ? null : CurrentGuideTarget);
            if (guideLabel != null)
            {
                guideLabel.text = CurrentInstruction;
                guideLabel.gameObject.SetActive(guide.showTrail && !introducing);
                if (CurrentGuideTarget != null)
                {
                    Vector3 labelPosition = CurrentGuideTarget.position;
                    if (guide.line.positionCount > 0)
                        labelPosition.y = Mathf.Max(labelPosition.y, guide.line.GetPosition(guide.line.positionCount - 1).y);
                    guideLabel.transform.position = labelPosition + Vector3.up * 1.1f;
                }
                else guideLabel.gameObject.SetActive(false);
                if (Camera.main != null) guideLabel.transform.rotation = Camera.main.transform.rotation;
            }
        }
        foreach (TMP_Text display in objectiveDisplays)
            if (display != null) display.text = "<b>MINE & DEFEND</b>\n"
                + (guide != null ? CurrentInstruction + "\n\n" : "") + goal;
    }

    private void UpdateGuidance()
    {
        int strength = 0;
        PurchasedTowerGrab upgradeTarget = null;
        foreach (TowerBuildSlot slot in buildSlots)
        {
            if (slot == null || slot.builtTower == null) continue;
            var tower = slot.builtTower.GetComponent<PurchasedTowerGrab>();
            strength += 1 + (tower != null ? tower.GetLevel() : 0);
            if (tower != null && tower.GetLevel() < 2
                && (upgradeTarget == null || tower.NextUpgradeCost < upgradeTarget.NextUpgradeCost)) upgradeTarget = tower;
        }
        if (waves.clearedWaves != lastClearedWave)
        {
            lastClearedWave = waves.clearedWaves;
            strengthAtWaveEnd = strength;
            preparedNextWave = false;
        }
        if (strength > strengthAtWaveEnd) preparedNextWave = true;
        if (waves.baseHealth != null && waves.baseHealth.lives <= 0)
        { SetGuide("Defense lost. Restart to try another arrangement.", null); return; }
        if (waves.clearedWaves >= waves.enemyCounts.Length)
        { SetGuide("All waves cleared! Explore and enjoy your workshop.", null); return; }
        if (waves.waveActive)
        { SetGuide("Defend the room — wave " + (waves.currentWave + 1) + " is underway.", null); return; }

        // A purchase advances to placement even if the player skipped the suggested mining trip.
        foreach (var tower in FindObjectsByType<PurchasedTowerGrab>())
        {
            if (!tower.purchased || tower.placedSlot != null) continue;
            foreach (var slot in buildSlots)
                if (slot != null && !slot.blocked && !slot.isPath && slot.builtTower == null)
                { SetGuide("Place your purchased machine on a free green cell.", slot.buildPoint); return; }
        }
        int carriedValue = 0;
        if (InventoryManager.Instance != null)
            foreach (OreType ore in System.Enum.GetValues(typeof(OreType)))
                carriedValue += InventoryManager.Instance.GetCount(ore) * InventoryManager.Instance.GetSellPrice(ore);

        if (!builtFirstTower || strength == 0)
        {
            // A small first mining trip explains the shared economy, even with starting Gold.
            if (!builtFirstTower && sellStation.totalGoldEarned == 0)
            {
                SetGuide(carriedValue >= 40 ? "Sell your ore at the cart to earn Gold."
                    : "Use the pickaxe to mine ore: " + carriedValue + "/40 Gold carried.",
                    carriedValue >= 40 ? sellStation.transform : starterMine);
                return;
            }
            if (GuideFunding(rapidShelf.price, carriedValue, "a basic tower")) return;
            SetGuide("Grab a basic tower from the shelf: " + rapidShelf.price + " Gold.", rapidShelf.displayPoint);
            return;
        }
        if (waves.clearedWaves == 0)
        { SetGuide("Start your first defense at the wave button.", waveButton); return; }
        if (!zoneA.IsUnlocked)
        { GuideToolbox(zoneA, carriedValue); return; }
        if (!zoneB.IsUnlocked && zoneB.RequirementsMet)
        { GuideToolbox(zoneB, carriedValue); return; }

        // Encourage one improvement between defenses. Never prevent starting a wave manually.
        if (!preparedNextWave)
        {
            int cost = upgradeTarget != null ? upgradeTarget.NextUpgradeCost : rapidShelf.price;
            if (GuideFunding(cost, carriedValue, "your next defense improvement")) return;
            if (upgradeTarget == null)
                SetGuide("Add another basic tower: " + cost + " Gold.", rapidShelf.displayPoint);
            else if (hammer != null && hammer.grab != null && !hammer.grab.isSelected)
                SetGuide("Pick up the hammer. Your next upgrade costs " + cost + " Gold.", hammer.transform);
            else SetGuide("Strike this placed tower to upgrade: " + cost + " Gold.", upgradeTarget.transform);
            return;
        }
        SetGuide(zoneB.IsUnlocked ? "Your defense improved. Start the next wave when ready."
            : "Start the next wave. Entrance B needs 4 cleared waves (" + waves.clearedWaves + "/4).", waveButton);
    }

    private bool GuideFunding(int cost, int carriedValue, string purchase)
    {
        int gold = rapidShelf.resources.gold;
        if (gold >= cost) return false;
        bool canSell = gold + carriedValue >= cost;
        Transform mine = zoneB.IsUnlocked ? richerMineB : zoneA.IsUnlocked ? richerMineA : starterMine;
        SetGuide(canSell ? "Sell your ore to afford " + purchase + "."
            : "Mine " + (cost - gold - carriedValue) + " more Gold of ore for " + purchase + "."
                + (zoneA.IsUnlocked ? "\nFollow the trail to richer ore." : ""),
            canSell ? sellStation.transform : mine);
        return true;
    }

    private void GuideToolbox(ToolboxPlacementZone zone, int carriedValue)
    {
        if (zone.CurrentPlacements >= zone.RequiredPlacements)
        { SetGuide("Passage opening — watch the entrance reveal.", zone.transform); return; }
        if (toolbox.IsPurchased && !toolbox.IsConsumed)
        { SetGuide("Release your toolbox at " + zone.routeName + ": " + zone.CurrentPlacements + "/" + zone.RequiredPlacements + ".", zone.transform); return; }
        if (GuideFunding(toolbox.PurchaseCost, carriedValue, "a toolbox")) return;
        SetGuide("Buy a toolbox at the shop: " + toolbox.PurchaseCost + " Gold.", toolbox.transform);
    }

    private void SetGuide(string instruction, Transform target)
    {
        CurrentInstruction = instruction;
        CurrentGuideTarget = target;
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
