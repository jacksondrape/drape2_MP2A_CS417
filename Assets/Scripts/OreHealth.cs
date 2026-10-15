using UnityEngine;

public enum OreSize
{
    Small,
    Medium,
    Large
}

public class OreHealth : MonoBehaviour
{
    [SerializeField] private OreType oreType;
    [SerializeField] private OreSize size;
    [SerializeField] private int currentHealth;

    private OreSpawnPoint spawnPoint;
    private bool depleted;

    public OreType Type => oreType;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = GetMaxHealth();
    }

    public int GetMaxHealth()
    {
        switch (size)
        {
            case OreSize.Small: return 3;
            case OreSize.Medium: return 4;
            case OreSize.Large: return 5;
            default: return 3;
        }
    }

    public void SetSpawnPoint(OreSpawnPoint point)
    {
        spawnPoint = point;
    }

    public void TakeHit()
    {
        if (depleted) return;

        currentHealth--;

        if (currentHealth <= 0)
        {
            depleted = true;

            if (spawnPoint != null)
                spawnPoint.NotifyOreDestroyed(this);

            Destroy(gameObject);
        }
    }
}
