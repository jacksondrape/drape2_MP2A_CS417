using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Tooltip("Scrap currency. The field name is retained to preserve existing scene references.")]
    public int gold = 0;

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
            return false;

        gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gold += amount;
        Debug.Log("Scrap: " + gold, this);
    }
}
