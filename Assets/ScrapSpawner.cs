using System.Collections.Generic;
using UnityEngine;

public class ScrapSpawner : MonoBehaviour
{
    public ScrapPickup canPrefab;
    public TowerBuildSlot[] cells;
    public PlayerResources resources;
    public DesktopBuildInput input;
    public BaseHealth cheese;
    public int startingCans = 6;
    public int maxCans = 8;
    public float minSpawnDelay = 12f;
    public float maxSpawnDelay = 20f;
    [HideInInspector] public bool tutorialShown;
    private float countdown;

    private void Start()
    {
        for (int i = 0; i < startingCans; i++) SpawnCan();
        countdown = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    private void Update()
    {
        if (cheese != null && cheese.lives <= 0) return;
        // New cans arrive during waves. Opening cans also exist in build mode.
        countdown -= Time.deltaTime;
        if (countdown > 0f) return;
        SpawnCan();
        countdown = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    private void SpawnCan()
    {
        if (canPrefab == null || cells == null || resources == null) return;
        List<TowerBuildSlot> empty = new List<TowerBuildSlot>();
        int count = 0;
        foreach (TowerBuildSlot cell in cells)
        {
            if (cell == null) continue;
            if (cell.scrapPickup != null) count++;
            if (!cell.isPath && !cell.blocked && cell.builtTower == null && cell.scrapPickup == null)
                empty.Add(cell);
        }
        if (count >= maxCans || empty.Count == 0) return;
        TowerBuildSlot chosen = empty[Random.Range(0, empty.Count)];
        Vector3 position = chosen.transform.position;
        position.y = 0.035f;
        ScrapPickup can = Instantiate(canPrefab, position, Quaternion.identity);
        can.resources = resources;
        can.cell = chosen;
        can.source = this;
        can.input = input;
        chosen.scrapPickup = can;
    }
}
