using UnityEngine;

public class EnemyPathFollower : MonoBehaviour
{
    public SimpleWaveController waves;
    private void Start()
    {
        if (waves == null) waves = FindAnyObjectByType<SimpleWaveController>();
    }

    public float moveSpeed = 1f;
    public Transform[] waypoints;
    public BaseHealth baseHealth;
    public bool attackObstacles;
    public float obstacleStopDistance = 0.7f;
    public float obstacleDamage = 3f;
    public float obstacleAttackInterval = 1f;
    private float attackCooldown;

    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (Time.timeScale <= 0f || (waves != null && !waves.CombatActive)) return;
        attackCooldown -= Time.deltaTime;
        // Stay still if no path is assigned or the path is finished.
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
        {
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
        {
            return;
        }

        if (attackObstacles && AttackObstacleAhead(target.position)) return;

        transform.position = Vector3.MoveTowards(
            transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= 0.01f)
        {
            transform.position = target.position;
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log("Enemy reached the end of the path.", this);
                if (baseHealth != null)
                {
                    baseHealth.LoseLife();
                    // Stop targeting and incoming projectiles immediately on escape.
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                }
            }
        }
    }

    private bool AttackObstacleAhead(Vector3 waypoint)
    {
        Vector3 start = transform.position;
        start.y = 0f;
        waypoint.y = 0f;
        Vector3 direction = waypoint - start;
        float remainingDistance = direction.magnitude;
        if (remainingDistance < 0.001f) return false;
        direction /= remainingDistance;
        TowerObstacle nearest = null;
        float nearestDistance = obstacleStopDistance + moveSpeed * Time.deltaTime;

        foreach (TowerObstacle obstacle in FindObjectsByType<TowerObstacle>())
        {
            if (!obstacle.isActiveAndEnabled || obstacle.health <= 0f) continue;
            Vector3 offset = obstacle.transform.position - start;
            offset.y = 0f;
            float ahead = Vector3.Dot(offset, direction);
            float sideways = (offset - direction * ahead).magnitude;
            // Only block the current path segment, never a neighboring road.
            if (ahead < -0.05f || ahead > remainingDistance + 0.05f || sideways > 0.45f) continue;
            if (ahead <= nearestDistance)
            {
                nearest = obstacle;
                nearestDistance = ahead;
            }
        }
        if (nearest == null) return false;
        if (attackCooldown <= 0f)
        {
            nearest.TakeDamage(obstacleDamage);
            attackCooldown = obstacleAttackInterval;
        }
        return true;
    }
}


