using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    public Renderer body;
    public float duration = 0.08f;
    private float remaining;
    public void Flash()
    {
        if (body == null) return;
        MaterialPropertyBlock colors = new MaterialPropertyBlock();
        colors.SetColor("_BaseColor", Color.white);
        colors.SetColor("_Color", Color.white);
        body.SetPropertyBlock(colors);
        remaining = duration;
    }
    private void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f && body != null) body.SetPropertyBlock(null);
    }
}
