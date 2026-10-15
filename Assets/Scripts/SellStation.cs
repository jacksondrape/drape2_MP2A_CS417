using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class SellStation : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

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

        Debug.Log($"Sold inventory for {earned} gold.");
    }
}