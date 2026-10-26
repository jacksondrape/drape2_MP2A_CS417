using UnityEngine;
using UnityEngine.Serialization;

public class PlayerResources : MonoBehaviour
{
    public EconomyManager sharedEconomy;
    [FormerlySerializedAs("gold")]
    [SerializeField] private int startingGold;
    public int gold => sharedEconomy != null ? sharedEconomy.Gold : startingGold;

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
            return false;
        if (sharedEconomy != null) return sharedEconomy.TrySpend(amount);

        startingGold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (sharedEconomy != null) sharedEconomy.AddGold(amount);
        else startingGold += amount;
        Debug.Log("Gold: " + gold, this);
    }
}
