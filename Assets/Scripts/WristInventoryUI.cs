using TMPro;
using UnityEngine;

public class WristInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text inventoryText;

    private InventoryManager inventory;
    private CaveGuideTrail guide;
    private bool lastGuideShown;

    private void Update()
    {
        if (guide == null) guide = FindFirstObjectByType<CaveGuideTrail>();
        if (inventory != InventoryManager.Instance)
        {
            if (inventory != null) inventory.InventoryChanged -= Refresh;
            inventory = InventoryManager.Instance;
            if (inventory != null) inventory.InventoryChanged += Refresh;
            Refresh();
        }
        if (guide != null && (lastGuideShown != guide.showTrail || inventory == null)) Refresh();
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
        if (guide != null)
        {
            lastGuideShown = guide.showTrail;
            inventoryText.text += "\nGuide: " + (guide.showTrail ? "ON" : "OFF") + "  |  Y / G: toggle";
        }
    }
}
