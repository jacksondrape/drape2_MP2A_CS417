using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Manual Play-mode verification. Changes disappear when Play mode stops.
public static class ProgressionSmokeTest
{
    [MenuItem("Tools/MP2B/Test Progression in Fresh Play Mode %&t")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;
        var progress = Object.FindFirstObjectByType<CaveDefenseProgression>();
        Require(progress != null && progress.waves.clearedWaves == 0
            && progress.sellStation.totalGoldEarned == 0 && progress.zoneA.CurrentPlacements == 0,
            "Run only in a fresh MineDefense Play session.");
        progress.StartCoroutine(Check(progress));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("PROGRESSION TEST FAILED: " + message);
    }

    private static void PrepareToolbox(ToolboxPlacementZone zone, ToolboxPurchase toolbox)
    {
        toolbox.ResetForRepurchase();
        toolbox.gameObject.SetActive(true);
        typeof(ToolboxPurchase).GetProperty("IsPurchased").SetValue(toolbox, true);
        typeof(ToolboxPlacementZone).GetField("overlappingToolbox", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(zone, toolbox);
    }

    private static IEnumerator Check(CaveDefenseProgression p)
    {
        Require(!p.sodaShelf.gameObject.activeSelf && !p.fanShelf.gameObject.activeSelf
            && !p.amplifierShelf.gameObject.activeSelf, "Advanced shelves must start hidden.");
        Require(!p.firstBuildTrophy.activeSelf && !p.firstWaveTrophy.activeSelf
            && !p.explorerTrophy.activeSelf && !p.traderTrophy.activeSelf && !p.engineerTrophy.activeSelf,
            "All trophies must start hidden.");
        p.sellStation.SellInventory();
        Require(p.sellStation.feedbackLabel.text.Contains("NOTHING TO SELL"), "Empty sale explanation.");
        yield return new WaitForSecondsRealtime(.3f);
        Require(p.CurrentGuideTarget == p.starterMine && p.guide.line.enabled, "Start with a visible mining trail.");
        InventoryManager.Instance.AddOre(OreType.Redstone, 20);
        yield return new WaitForSecondsRealtime(.3f);
        Require(p.CurrentGuideTarget == p.sellStation.transform, "Enough carried ore redirects to selling.");
        p.guide.ToggleTrail();
        yield return new WaitForSecondsRealtime(.3f);
        Require(!p.guide.line.enabled && !p.guideLabel.gameObject.activeSelf, "Guide can be hidden.");
        p.guide.ToggleTrail();

        var toolbox = Object.FindFirstObjectByType<ToolboxPurchase>();
        PrepareToolbox(p.zoneB, toolbox);
        Require(!p.zoneB.TryConsume(toolbox) && !toolbox.IsConsumed, "B must reject without consuming before A.");
        yield return new WaitForSecondsRealtime(.2f);
        Require(p.zoneB.progressLabel.text.Contains("ENTRANCE NOT READY"), "Early entrance explanation. Active=" + p.zoneB.isActiveAndEnabled + " text=" + p.zoneB.progressLabel.text);

        // Exercise the real inventory -> sale -> shared Gold -> unlock connection.
        int price = InventoryManager.Instance.GetSellPrice(OreType.Redstone);
        InventoryManager.Instance.AddOre(OreType.Redstone, Mathf.CeilToInt(100f / price));
        int balance = EconomyManager.Instance.Gold;
        p.sellStation.SellInventory();
        Require(EconomyManager.Instance.Gold >= balance + 100, "Ore sale must increase shared Gold.");
        yield return null;
        yield return null;
        Require(p.revealDisplay.gameObject.activeSelf && !p.sodaShelf.gameObject.activeSelf,
            "Soda announcement must precede shelf.");
        yield return new WaitForSecondsRealtime(p.introductionSeconds + .2f);
        Require(p.sodaShelf.gameObject.activeSelf && p.traderTrophy.activeSelf, "Soda and trader unlock.");

        p.waves.clearedWaves = 1; // Simulate milestone; combat itself is not under test here.
        yield return null;
        yield return null;
        Require(!p.fanShelf.gameObject.activeSelf && p.firstWaveTrophy.activeSelf, "Fan reveal before availability.");
        yield return new WaitForSecondsRealtime(p.introductionSeconds + .2f);
        Require(p.fanShelf.gameObject.activeSelf, "Fan unlock.");
        p.waves.clearedWaves = 3;
        yield return null;
        yield return null;
        Require(!p.amplifierShelf.gameObject.activeSelf, "Amplifier reveal before availability.");
        yield return new WaitForSecondsRealtime(p.introductionSeconds + .2f);
        Require(p.amplifierShelf.gameObject.activeSelf, "Amplifier unlock.");

        PrepareToolbox(p.zoneA, toolbox);
        Require(p.zoneA.TryConsume(toolbox) && !p.zoneA.IsUnlocked, "A introduction precedes opening.");
        yield return new WaitForSecondsRealtime(p.zoneA.revealSeconds + .2f);
        Require(p.zoneA.IsUnlocked && p.objectiveDisplays[0].text.Contains("route B"), "A opens and reveals B goal.");
        PrepareToolbox(p.zoneB, toolbox);
        Require(!p.zoneB.TryConsume(toolbox) && !toolbox.IsConsumed && p.zoneB.CurrentPlacements == 0,
            "B must reject at three cleared waves without consuming or counting the toolbox.");
        yield return new WaitForSecondsRealtime(.3f);
        Require(p.zoneB.progressLabel.text.Contains("3/4"), "B explains the missing fourth wave.");
        p.waves.clearedWaves = 4;
        for (int i = 1; i <= 3; i++)
        {
            PrepareToolbox(p.zoneB, toolbox);
            Require(p.zoneB.TryConsume(toolbox) && p.zoneB.CurrentPlacements == i && !p.zoneB.IsUnlocked,
                "B must count exactly three placements and remain closed during introduction.");
            yield return new WaitForSecondsRealtime(.7f);
        }
        yield return new WaitForSecondsRealtime(p.zoneB.revealSeconds + .2f);
        Require(p.zoneB.IsUnlocked && p.explorerTrophy.activeSelf, "B opens and explorer trophy appears.");
        CaveGuidanceSetup.CheckOpenRoutes(p);

        var slot = p.buildSlots[0];
        TowerDisplayShelf shelf = null;
        foreach (var candidate in Object.FindObjectsByType<TowerDisplayShelf>(FindObjectsSortMode.None))
            if (candidate.towerPrefab.GetComponent<TowerSpeedUpgrade>() != null) { shelf = candidate; break; }
        Require(shelf != null, "Projectile tower shelf exists.");
        var tower = Object.Instantiate(shelf.towerPrefab, slot.buildPoint.position, Quaternion.identity);
        tower.resources = shelf.resources;
        Require(tower.HammerUpgrade().Contains("Place"), "Unplaced hammer explanation.");
        tower.purchased = true;
        tower.placedSlot = slot;
        slot.builtTower = tower.GetComponent<TowerTargeting>();
        var upgrade = tower.GetComponent<TowerSpeedUpgrade>();
        if (upgrade != null) upgrade.resources = shelf.resources;
        EconomyManager.Instance.TrySpend(EconomyManager.Instance.Gold);
        Require(tower.HammerUpgrade().Contains("Mine and sell"), "Insufficient upgrade Gold explanation.");
        EconomyManager.Instance.AddGold(1000);
        int beforeUpgrade = EconomyManager.Instance.Gold;
        int quotedCost = tower.NextUpgradeCost;
        Require(quotedCost >= 120, "Increased first upgrade price is loaded from the prefab.");
        tower.HammerUpgrade();
        Require(beforeUpgrade - EconomyManager.Instance.Gold == quotedCost, "Hammer charge matches quoted price.");
        yield return null;
        yield return null;
        Require(p.firstBuildTrophy.activeSelf && p.engineerTrophy.activeSelf, "Build and upgrade trophies.");
        tower.HammerUpgrade();
        Require(beforeUpgrade - EconomyManager.Instance.Gold == quotedCost * 3, "Second upgrade costs twice the first.");
        Require(tower.HammerUpgrade().Contains("MAX LEVEL"), "Maximum upgrade explanation.");
        var second = Object.Instantiate(shelf.towerPrefab, slot.buildPoint.position, Quaternion.identity);
        second.GetComponent<Rigidbody>().isKinematic = true;
        second.purchased = true;
        var release = (IEnumerator)typeof(PurchasedTowerGrab).GetMethod("PlaceAfterRelease", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(second, null);
        yield return p.StartCoroutine(release);
        yield return new WaitForSecondsRealtime(.2f);
        Require(second.placedSlot == null && second.vrStatus.text.Contains("CELL OCCUPIED"), "Occupied cell lesson.");
        Debug.Log("PROGRESSION TEST PASSED: five thresholds/reveals, five trophies, five errors, shared ore-sale income. Milestones simulated; physical XR inputs still need testing.");
        System.IO.File.WriteAllText("ProgressionTestResult.txt", "PASS: mining/selling guide switch, hide/show, wave-4 B gate with unconsumed rejection, three toolboxes, exact upgrade charges, and existing progression checks. Wave milestones and toolbox overlap/purchase were simulated; XR controller inputs were not tested.");
    }
}
