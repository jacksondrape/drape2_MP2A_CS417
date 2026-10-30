using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class SellStation : MonoBehaviour
{
    public TMPro.TMP_Text feedbackLabel;
    [System.NonSerialized] public int totalGoldEarned;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    private void LateUpdate()
    {
        if (feedbackLabel != null && Camera.main != null)
            feedbackLabel.transform.rotation = Camera.main.transform.rotation;
    }

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelected);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        SellInventory();
    }

    public void SellInventory()
    {
        if (InventoryManager.Instance == null ||
            EconomyManager.Instance == null)
        {
            Debug.LogWarning("Inventory or economy manager is missing.");
            return;
        }

        int earned = InventoryManager.Instance.SellAll();
        EconomyManager.Instance.AddGold(earned);
        totalGoldEarned += earned;
        if (feedbackLabel != null)
            feedbackLabel.text = earned > 0
                ? "SOLD ORE: +" + earned + " GOLD\nTotal mined income: " + totalGoldEarned
                : "NOTHING TO SELL\nMine ore with your pickaxe and collect it,\nthen select this cart to sell your inventory.";

        Debug.Log($"Sold inventory for {earned} gold.");
    }
}
