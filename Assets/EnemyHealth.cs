using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 3f;
    public int goldReward = 10;
    public PlayerResources playerResources;
    public ScrapPickup scrapDropPrefab;

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || health <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - damage);
        EnemyHitFlash flash = GetComponent<EnemyHitFlash>();
        if (flash != null) flash.Flash();
        Debug.Log("Enemy health: " + health, this);

        if (health <= 0f)
        {
            if (scrapDropPrefab != null)
            {
                ScrapPickup drop = Instantiate(scrapDropPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
                drop.resources = playerResources;
                drop.scrapAmount = goldReward;
                Rigidbody body = drop.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.AddForce(new Vector3(Random.Range(-0.8f, 0.8f), 1.5f, Random.Range(-0.8f, 0.8f)), ForceMode.Impulse);
                    body.AddTorque(Random.insideUnitSphere * 0.2f, ForceMode.Impulse);
                }
            }
            else if (playerResources != null)
            {
                playerResources.AddGold(goldReward);
            }

            Debug.Log("Enemy died.", this);
            Destroy(gameObject);
        }
    }

    // Use the component's Inspector menu to test without a tower.
    [ContextMenu("Test: Take 1 Damage")]
    private void TestTakeDamage()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Enter Play mode to test damage.", this);
            return;
        }

        TakeDamage(1f);
    }
}
