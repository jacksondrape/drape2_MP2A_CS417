using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToolboxPlacementZone : MonoBehaviour
{
    [Header("Pathway Settings")]
    [SerializeField] private GameObject originalObject;
    [SerializeField] private GameObject replacementPrefab;
    [SerializeField] private Transform replacementSpawnPoint;

    [Header("Placement Requirements")]
    [SerializeField, Min(1)] private int requiredPlacements = 1;

    private int currentPlacements = 0;
    private bool isUnlocked = false;

    private ToolboxPurchase overlappingToolbox;

    public int CurrentPlacements => currentPlacements;
    public int RequiredPlacements => requiredPlacements;
    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ToolboxPurchase toolbox =
            other.GetComponentInParent<ToolboxPurchase>();

        if (toolbox != null)
            overlappingToolbox = toolbox;
    }

    private void OnTriggerExit(Collider other)
    {
        ToolboxPurchase toolbox =
            other.GetComponentInParent<ToolboxPurchase>();

        if (toolbox != null && overlappingToolbox == toolbox)
            overlappingToolbox = null;
    }

    public bool TryConsume(ToolboxPurchase toolbox)
    {
        if (toolbox == null || overlappingToolbox != toolbox)
            return false;

        if (!toolbox.IsPurchased || toolbox.IsConsumed)
            return false;

        if (isUnlocked)
            return false;

        // Count this valid toolbox placement.
        currentPlacements++;

        Debug.Log(
            $"Toolbox placement: {currentPlacements}/{requiredPlacements}");

        // Unlock the pathway only after the required number
        // of toolbox placements has been reached.
        if (currentPlacements >= requiredPlacements)
        {
            UnlockPathway();
        }

        // Consume the toolbox and return it to the shop.
        StartCoroutine(ConsumeAndReset(toolbox));

        return true;
    }

    private void UnlockPathway()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        if (originalObject != null)
            originalObject.SetActive(false);

        if (replacementPrefab != null)
        {
            Transform spawn = replacementSpawnPoint != null
                ? replacementSpawnPoint
                : transform;

            Instantiate(
                replacementPrefab,
                spawn.position,
                spawn.rotation);
        }

        Debug.Log("Pathway unlocked!");
    }

    private IEnumerator ConsumeAndReset(ToolboxPurchase toolbox)
    {
        overlappingToolbox = null;

        toolbox.SetConsumed(true);
        toolbox.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        toolbox.ResetForRepurchase();
        toolbox.gameObject.SetActive(true);
    }
}