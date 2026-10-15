using System;
using TMPro;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int startingGold;
    [SerializeField] private TMP_Text goldText;

    public int Gold { get; private set; }

    public event Action GoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Duplicate EconomyManager found.");
            enabled = false;
            return;
        }

        Instance = this;
        Gold = Mathf.Max(0, startingGold);
        RefreshUI();
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && Gold >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        Gold -= amount;
        RefreshUI();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        Gold += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {Gold}";

        GoldChanged?.Invoke();
    }
}