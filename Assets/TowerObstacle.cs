using UnityEngine;

public class TowerObstacle : MonoBehaviour
{
    public float maxHealth = 30f;
    public float health = 30f;
    public float repairAmount = 10f;
    public int repairCost = 5;
    public PlayerResources resources;
    public Renderer body;

    public void TakeDamage(float damage)
    {
        if (health <= 0f || damage <= 0f) return;
        health = Mathf.Max(0f, health - damage);
        UpdateColor();
        if (health <= 0f)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public string Repair()
    {
        if (health <= 0f) return "This obstacle is destroyed.";
        if (health >= maxHealth) return "Obstacle already at full health.";
        if (resources == null || !resources.TrySpendGold(repairCost))
            return "Repair needs " + repairCost + " gold.";
        health = Mathf.Min(maxHealth, health + repairAmount);
        UpdateColor();
        return "Repaired obstacle. -" + repairCost + " gold.";
    }

    private void UpdateColor()
    {
        if (body == null) return;
        MaterialPropertyBlock colors = new MaterialPropertyBlock();
        Color color = Color.Lerp(Color.red, Color.blue, health / maxHealth);
        colors.SetColor("_BaseColor", color);
        colors.SetColor("_Color", color);
        body.SetPropertyBlock(colors);
    }
}
