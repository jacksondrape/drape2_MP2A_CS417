using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ToolboxPurchase : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [SerializeField, Min(0)] private int purchaseCost = 50;
    [SerializeField] private ToolboxPlacementZone[] placementZones;

    public bool IsPurchased { get; private set; }
    public bool IsConsumed { get; private set; }

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    protected override void Awake()
    {
        base.Awake();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (IsConsumed)
            return;

        if (!IsPurchased)
        {
            if (EconomyManager.Instance == null ||
                !EconomyManager.Instance.TrySpend(purchaseCost))
            {
                Debug.Log("Not enough gold to purchase the toolbox.");
                return;
            }

            IsPurchased = true;
            Debug.Log("Toolbox purchased.");
        }

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (!IsPurchased || IsConsumed)
            return;

        foreach (ToolboxPlacementZone zone in placementZones)
        {
            if (zone != null && zone.TryConsume(this))
                break;
        }
    }

    public void SetConsumed(bool value)
    {
        IsConsumed = value;
    }

    public void ResetForRepurchase()
    {
        transform.SetParent(originalParent);
        transform.SetPositionAndRotation(
            originalPosition, originalRotation);

        IsPurchased = false;
        IsConsumed = false;
    }
}