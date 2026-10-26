using TMPro;
using UnityEngine;

public class WristInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text inventoryText;

    private InventoryManager inventory;

    private void Update()
    {
        if (inventory == InventoryManager.Instance) return;
        if (inventory != null) inventory.InventoryChanged -= Refresh;
        inventory = InventoryManager.Instance;
        if (inventory != null) inventory.InventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.InventoryChanged -= Refresh;
        inventory = null;
    }

    private void Refresh()
    {
        if (inventoryText == null) return;

        if (InventoryManager.Instance == null)
            inventoryText.text = "Inventory\nLoading...";
        else
            inventoryText.text =
                "Inventory\n" +
                InventoryManager.Instance.GetInventoryText();
    }
}
