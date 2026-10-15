using UnityEngine;

public class PickaxeHit : MonoBehaviour
{
    [SerializeField, Min(0f)] private float hitCooldown = 0.25f;

    private float nextHitTime;

    private void OnCollisionEnter(Collision collision)
    {
        TryMine(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryMine(other);
    }

    private void TryMine(Collider other)
    {
        if (Time.time < nextHitTime) return;

        OreHealth ore = other.GetComponentInParent<OreHealth>();
        if (ore == null) return;

        nextHitTime = Time.time + hitCooldown;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddOre(ore.Type, 1);

        ore.TakeHit();
    }
}
