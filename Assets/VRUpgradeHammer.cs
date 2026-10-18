using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRUpgradeHammer : MonoBehaviour
{
    public XRGrabInteractable grab;
    public TMP_Text label;
    public float minimumSpeed = 0.15f;
    public float hitCooldown = 0.8f;
    private Vector3 lastPosition;
    private float speed;
    private float nextHit;
    private float messageUntil;

    private void OnEnable() { lastPosition = transform.position; }
    private void FixedUpdate()
    {
        speed = Vector3.Distance(transform.position, lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (grab == null || !grab.isSelected || speed < minimumSpeed || Time.unscaledTime < nextHit) return;
        PurchasedTowerGrab tower = other.GetComponentInParent<PurchasedTowerGrab>();
        if (tower == null) return;
        nextHit = Time.unscaledTime + hitCooldown;
        int previousLevel = tower.GetLevel();
        string result = tower.HammerUpgrade();
        bool success = tower.GetLevel() > previousLevel;
        if (success)
        {
            MachineFeedback feedback = tower.GetComponent<MachineFeedback>();
            if (feedback != null)
            {
                ParticleSystem sparks = feedback.damageSparks != null ? feedback.damageSparks : feedback.chargeSparks;
                if (sparks != null) sparks.Emit(20);
            }
        }
        if (label != null) label.text = (success ? "<color=#AAFFAA>" : "<color=#FFD580>") + result + "</color>";
        messageUntil = Time.unscaledTime + 3f;
    }

    private void LateUpdate()
    {
        if (label == null) return;
        label.gameObject.SetActive(grab != null && grab.isSelected);
        if (Camera.main != null) label.transform.rotation = Camera.main.transform.rotation;
        if (Time.unscaledTime >= messageUntil) label.text = "UPGRADE HAMMER\nStrike a placed machine\n15 / 30 SCRAP";
    }
}
