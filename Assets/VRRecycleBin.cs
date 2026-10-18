using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRRecycleBin : MonoBehaviour
{
    public TMP_Text label;
    private float messageUntil;

    private void OnTriggerStay(Collider other)
    {
        XRGrabInteractable grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;
        ScrapPickup scrap = other.GetComponentInParent<ScrapPickup>();
        if (scrap != null && scrap.gameObject.activeInHierarchy)
        {
            Show(scrap.Collect());
            return;
        }
        PurchasedTowerGrab tower = other.GetComponentInParent<PurchasedTowerGrab>();
        if (tower != null && tower.Recycle()) Show("RECYCLED MACHINE\n+10 SCRAP");
    }

    private void Show(string text)
    {
        if (label != null) label.text = text;
        messageUntil = Time.unscaledTime + 3f;
    }

    private void Update()
    {
        if (label != null && Camera.main != null) label.transform.rotation = Camera.main.transform.rotation;
        if (label != null && Time.unscaledTime >= messageUntil)
            label.text = "RECYCLE\nRelease scrap or purchased machines inside\nMachines: +10 SCRAP";
    }
}
