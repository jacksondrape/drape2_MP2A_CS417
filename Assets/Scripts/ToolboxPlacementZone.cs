
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ToolboxPlacementZone : MonoBehaviour
{
    [Header("Pathway Settings")]
    [SerializeField] private GameObject originalObject;
    [SerializeField] private GameObject replacementPrefab;
    [SerializeField] private Transform replacementSpawnPoint;

    [Header("Placement Requirements")]
    [SerializeField, Min(1)] private int requiredPlacements = 1;
    public ToolboxPlacementZone previousZone;
    public TMP_Text progressLabel;
    public string routeName = "Mining route";
    [TextArea] public string unlockIntroduction;
    public float revealSeconds = 5f;
    private bool revealing;
    private float errorUntil;

    private void LateUpdate()
    {
        if (progressLabel == null) return;
        bool available = previousZone == null || previousZone.IsUnlocked;
        bool showError = Time.unscaledTime < errorUntil;
        progressLabel.gameObject.SetActive(available || showError);
        if (showError)
        {
            if (Camera.main != null) progressLabel.transform.rotation = Camera.main.transform.rotation;
            progressLabel.text = "ENTRANCE NOT READY\nComplete the first toolbox entrance (A),\nthen bring your purchased toolbox back here.";
            return;
        }
        if (!available) return;
        if (Camera.main != null) progressLabel.transform.rotation = Camera.main.transform.rotation;
        progressLabel.text = revealing ? unlockIntroduction
            : isUnlocked ? routeName + "\nOPEN - explore the new passage"
            : routeName + "\nToolboxes delivered: " + currentPlacements + "/" + requiredPlacements
                + "\nBuy at the shop, then release here.";
    }

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

        if (previousZone != null && !previousZone.IsUnlocked)
        {
            errorUntil = Time.unscaledTime + 5f;
            return false;
        }
        if (isUnlocked || revealing)
            return false;

        // Count this valid toolbox placement.
        currentPlacements++;

        Debug.Log(
            $"Toolbox placement: {currentPlacements}/{requiredPlacements}");

        // Unlock the pathway only after the required number
        // of toolbox placements has been reached.
        if (currentPlacements >= requiredPlacements)
        {
            StartCoroutine(RevealThenUnlock());
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

    private IEnumerator RevealThenUnlock()
    {
        revealing = true;
        // Keep the barrier closed until the new passage has been introduced.
        if (progressLabel != null) yield return new WaitForSecondsRealtime(revealSeconds);
        UnlockPathway();
        revealing = false;
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
