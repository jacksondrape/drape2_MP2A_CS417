using System;
using UnityEngine;

public enum OreType
{
    Redstone,
    Emerald,
    Diamond,
    Rayronic
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Serializable]
    public class OreEntry
    {
        public OreType type;
        [Min(0)] public int sellPrice = 1;
        [Min(0)] public int count;
    }

    [SerializeField] private OreEntry[] ores =
    {
        new OreEntry { type = OreType.Redstone, sellPrice = 2 },
        new OreEntry { type = OreType.Emerald, sellPrice = 4 },
        new OreEntry { type = OreType.Diamond, sellPrice = 8 },
        new OreEntry { type = OreType.Rayronic, sellPrice = 12 }
    };

    public event Action InventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Duplicate InventoryManager found.");
            enabled = false;
            return;
        }

        Instance = this;
        InventoryChanged?.Invoke();
    }

    public int GetCount(OreType type)
    {
        OreEntry entry = FindEntry(type);
        return entry == null ? 0 : entry.count;
    }

    public int GetSellPrice(OreType type)
    {
        OreEntry entry = FindEntry(type);
        return entry == null ? 0 : entry.sellPrice;
    }

    public void AddOre(OreType type, int amount = 1)
    {
        OreEntry entry = FindEntry(type);
        if (entry == null || amount <= 0) return;

        entry.count += amount;
        InventoryChanged?.Invoke();
    }

    public int SellAll()
    {
        int total = 0;

        foreach (OreEntry entry in ores)
        {
            total += entry.count * entry.sellPrice;
            entry.count = 0;
        }

        InventoryChanged?.Invoke();
        return total;
    }

    public string GetInventoryText()
    {
        string result = "";

        foreach (OreEntry entry in ores)
        {
            result += $"{entry.type}: {entry.count}\n";
        }

        return result;
    }

    private OreEntry FindEntry(OreType type)
    {
        foreach (OreEntry entry in ores)
        {
            if (entry.type == type)
                return entry;
        }

        return null;
    }
}