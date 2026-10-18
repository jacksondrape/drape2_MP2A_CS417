using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    public SimpleWaveController waves;
    private void Start()
    {
        if (waves == null) waves = FindAnyObjectByType<SimpleWaveController>();
    }

    public TowerTargeting targeting;
    public Projectile projectilePrefab;
    public Transform firePoint;
    public Transform aimPivot;
    public float turnSpeed = 180f;
    public float fireInterval = 0.6f;
    public float bonusDamage;
    public TowerCooling cooling;
    public TowerDamageBoost damageBoost;
    public float EffectiveDamage => (projectilePrefab.damage + bonusDamage) * (damageBoost != null ? damageBoost.Multiplier : 1f);
    public float EffectiveInterval => fireInterval / (cooling != null ? cooling.speedMultiplier : 1f);
    public ParticleSystem muzzleFlash;
    public int flashParticleCount = 8;

    private float cooldown;

    // Run after Update so targeting has selected this frame's enemy.
    private void LateUpdate()
    {
        if (Time.timeScale <= 0f || (waves != null && !waves.CombatActive))
            return;

        cooldown -= Time.deltaTime * (cooling != null ? cooling.speedMultiplier : 1f);

        if (targeting == null || projectilePrefab == null || firePoint == null)
        {
            return;
        }

        EnemyHealth target = targeting.currentTarget;
        if (target == null || target.health <= 0f)
        {
            return;
        }

        if (aimPivot != null)
        {
            Vector3 direction = target.transform.position - aimPivot.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                aimPivot.rotation = Quaternion.RotateTowards(aimPivot.rotation,
                    Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
                if (Vector3.Angle(aimPivot.forward, direction) > 15f) return;
            }
        }
        if (cooldown > 0f) return;

        Projectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.target = target;
        projectile.damage = EffectiveDamage;
        if (muzzleFlash != null)
        {
            Vector3 direction = target.transform.position - firePoint.position;
            if (direction.sqrMagnitude > 0.001f)
                muzzleFlash.transform.rotation = Quaternion.LookRotation(direction);
            muzzleFlash.Emit(flashParticleCount);
        }
        cooldown = fireInterval;
    }
}


