using TMPro;
using UnityEngine;

public class WristInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text inventoryText;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InventoryChanged -= Refresh;
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
