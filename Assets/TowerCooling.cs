using UnityEngine;

public class TowerCooling : MonoBehaviour
{
    public CoolingFan source;
    public float speedMultiplier = 1f;
    public LineRenderer airflow;
    public GameObject blueMarker;
    public bool preview;
    private float refresh;

    public void RefreshBoost()
    {
        source = null;
        speedMultiplier = 1f;
        foreach (CoolingFan fan in FindObjectsByType<CoolingFan>())
        {
            if (!fan.isActiveAndEnabled || !fan.Covers(transform.position)) continue;
            if (fan.speedMultiplier > speedMultiplier)
            {
                source = fan;
                speedMultiplier = fan.speedMultiplier;
            }
        }
    }

    private void Update()
    {
        refresh -= Time.unscaledDeltaTime;
        if (refresh <= 0f || (source == null && speedMultiplier > 1f))
        {
            RefreshBoost();
            refresh = 0.15f;
        }
    }

    private void LateUpdate()
    {
        if (blueMarker != null) blueMarker.SetActive(preview || source != null);
        if (airflow == null) return;
        airflow.enabled = source != null && source.isActiveAndEnabled;
        if (!airflow.enabled) return;
        airflow.SetPosition(0, source.transform.position + Vector3.up * 0.4f);
        airflow.SetPosition(1, transform.position + Vector3.up * 0.4f);
        airflow.startWidth = airflow.endWidth = 0.025f + Mathf.Sin(Time.time * 8f) * 0.008f;
    }
}
