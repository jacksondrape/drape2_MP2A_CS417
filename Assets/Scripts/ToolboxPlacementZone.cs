using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToolboxPlacementZone : MonoBehaviour
{
    [SerializeField] private GameObject originalObject;
    [SerializeField] private GameObject replacementPrefab;
    [SerializeField] private Transform replacementSpawnPoint;

    private ToolboxPurchase overlappingToolbox;

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

        StartCoroutine(ConsumeAndReset(toolbox));
        return true;
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