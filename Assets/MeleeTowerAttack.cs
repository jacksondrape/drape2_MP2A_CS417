using UnityEngine;

public class MeleeTowerAttack : MonoBehaviour
{
    public TowerTargeting targeting;
    public float attackInterval = 0.65f;
    public float damage = 2f;

    private float cooldown;

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f)
            return;

        cooldown -= Time.deltaTime;
        if (targeting == null || targeting.currentTarget == null || cooldown > 0f)
            return;

        targeting.currentTarget.TakeDamage(damage);
        cooldown = attackInterval;
    }
}
