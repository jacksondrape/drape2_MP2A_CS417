using UnityEngine;

public class TowerBuildSlot : MonoBehaviour
{
    public PlayerResources resources;
    public EnemyHealth[] enemies;
    public TowerTargeting[] towerPrefabs;
    public Transform buildPoint;
    public int buildCost = 20;
    public int sellRefund = 10;
    public TowerTargeting builtTower;
    public bool restrictTowerType;
    public bool isPath;
    public bool blocked;
    public Renderer cellRenderer;
    public ScrapPickup scrapPickup;

    public string PlacementError(int towerType)
    {
        if (towerPrefabs != null && towerType >= 0 && towerType < towerPrefabs.Length
            && towerPrefabs[towerType] != null && towerPrefabs[towerType].GetComponent<MeleeTowerAttack>() != null)
            return "Melee towers are temporarily disabled.";
        if (restrictTowerType && isPath) return "Keep the road clear. Build ranged towers on ground.";
        if (blocked) return "Entrance and base cells cannot be built on.";
        if (builtTower != null) return "This cell is occupied.";
        if (resources == null || buildPoint == null || towerPrefabs == null
            || towerType < 0 || towerType >= towerPrefabs.Length || towerPrefabs[towerType] == null)
            return "Slot references are missing.";
        if (resources.gold < buildCost) return "Not enough scrap. Need " + buildCost + ".";
        return null;
    }

    public void Highlight(bool hovered, int towerType)
    {
        if (cellRenderer == null) return;
        if (!hovered) { cellRenderer.SetPropertyBlock(null); return; }
        MaterialPropertyBlock colors = new MaterialPropertyBlock();
        Color color = PlacementError(towerType) == null ? Color.green : Color.red;
        colors.SetColor("_BaseColor", color);
        colors.SetColor("_Color", color);
        cellRenderer.SetPropertyBlock(colors);
    }

    public string Build(int towerType)
    {
        string error = PlacementError(towerType);
        if (error != null) return error;
        if (builtTower != null)
            return "This slot is occupied.";
        if (resources == null || buildPoint == null || towerPrefabs == null
            || towerType < 0 || towerType >= towerPrefabs.Length || towerPrefabs[towerType] == null)
            return "Slot references are missing.";
        if (!resources.TrySpendGold(buildCost))
            return "Not enough scrap. Need " + buildCost + ".";

        builtTower = Instantiate(towerPrefabs[towerType], buildPoint.position, buildPoint.rotation);
        builtTower.enemies = enemies;
        builtTower.findEnemiesAutomatically = true;
        DamageAmplifier amplifier = builtTower.GetComponent<DamageAmplifier>();
        if (amplifier != null)
        {
            amplifier.resources = resources;
            amplifier.buildSlot = this;
        }
        CoolingFan fan = builtTower.GetComponent<CoolingFan>();
        if (fan != null)
        {
            fan.resources = resources;
            fan.buildSlot = this;
        }
        SodaTower soda = builtTower.GetComponent<SodaTower>();
        if (soda != null)
        {
            soda.resources = resources;
            soda.buildSlot = this;
        }
        TowerSpeedUpgrade upgrade = builtTower.GetComponent<TowerSpeedUpgrade>();
        if (upgrade != null)
        {
            upgrade.resources = resources;
            upgrade.buildSlot = this;
        }
        TowerObstacle obstacle = builtTower.GetComponent<TowerObstacle>();
        if (obstacle != null) obstacle.resources = resources;
        TowerRangeIndicator range = builtTower.GetComponent<TowerRangeIndicator>();
        if (range != null && restrictTowerType) range.showRange = false;
        return "Machine built. -" + buildCost + " SCRAP.";
    }

    public string Sell()
    {
        if (builtTower == null || resources == null)
            return "This slot is empty.";

        // Stop shooting immediately, even though Destroy finishes at the frame's end.
        GameObject soldTower = builtTower.gameObject;
        soldTower.SetActive(false);
        Destroy(soldTower);
        builtTower = null;
        resources.AddGold(sellRefund);
        return "Machine removed. +" + sellRefund + " SCRAP.";
    }
}
