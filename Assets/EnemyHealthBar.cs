using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyHealth health;
    public Transform fill;
    public Camera viewCamera;

    private float startingHealth;
    private Vector3 fullScale;
    private Vector3 fullPosition;

    private void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (health == null || fill == null)
            return;

        startingHealth = health.health;
        fullScale = fill.localScale;
        fullPosition = fill.localPosition;
    }

    private void LateUpdate()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera != null) transform.rotation = viewCamera.transform.rotation;
        if (health == null || fill == null || startingHealth <= 0f)
            return;

        float fraction = Mathf.Clamp01(health.health / startingHealth);
        fill.localScale = new Vector3(fullScale.x * fraction, fullScale.y, fullScale.z);
        // Keep the left edge fixed as health decreases.
        fill.localPosition = fullPosition + Vector3.left * fullScale.x * (1f - fraction) * 0.5f;
    }
}
