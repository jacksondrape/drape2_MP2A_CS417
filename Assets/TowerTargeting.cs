using UnityEngine;

public class TowerTargeting : MonoBehaviour
{
    public float range = 1.1f;
    public EnemyHealth[] enemies;
    public EnemyHealth currentTarget;
    public bool findEnemiesAutomatically;

    private float refreshTimer;

    private void Update()
    {
        if (findEnemiesAutomatically)
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
                refreshTimer = 0.25f;
            }
        }

        EnemyHealth nearestEnemy = null;
        float nearestDistance = range;

        // Assign the test enemies in the Inspector.
        if (enemies != null)
        {
            foreach (EnemyHealth enemy in enemies)
            {
                if (enemy == null || !enemy.isActiveAndEnabled || enemy.health <= 0f)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        if (currentTarget != nearestEnemy)
        {
            if (nearestEnemy != null)
            {
                Debug.Log("Tower target: " + nearestEnemy.name, this);
            }
            else
            {
                Debug.Log("Tower: no enemy in range.", this);
            }
        }

        currentTarget = nearestEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
