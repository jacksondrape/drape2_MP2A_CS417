using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float damage = 1f;
    public EnemyHealth target;
    public ParticleSystem impactEffect;

    public SimpleWaveController waves;
    private Rigidbody body;
    private bool hasHit;

    private void Awake()
    {
        if (waves == null) waves = FindAnyObjectByType<SimpleWaveController>();
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        GetComponent<SphereCollider>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        // Remove the projectile if its enemy died before it arrived.
        if ((waves != null && !waves.CombatActive) || target == null || !target.isActiveAndEnabled || target.health <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        body.MovePosition(Vector3.MoveTowards(
            body.position, target.transform.position, moveSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || (waves != null && !waves.CombatActive)) return;
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null || !enemy.isActiveAndEnabled || enemy.health <= 0f) return;
        // A shot can hit the first rat in its path, only once.
        hasHit = true;
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            impact.Play();
            Destroy(impact.gameObject, 1.5f);
        }
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}


