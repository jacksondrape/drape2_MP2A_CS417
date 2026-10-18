using TMPro;
using UnityEngine;

public class ScrapPickup : MonoBehaviour
{
    public int scrapAmount = 10;
    public PlayerResources resources;
    public TowerBuildSlot cell;
    public ScrapSpawner source;
    public TMP_Text hint;
    public DesktopBuildInput input;
    public GameObject[] appearances;
    private bool collected;
    private float hintTime;

    private void Awake()
    {
        // Existing prefab: its direct children are the four models and the hint.
        if (appearances == null || appearances.Length == 0)
        {
            var models = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in transform)
                if (child.GetComponentInChildren<MeshFilter>(true) != null
                    && child.GetComponentInChildren<TMP_Text>(true) == null)
                    models.Add(child.gameObject);
            appearances = models.ToArray();
        }
        if (appearances.Length == 0) return;
        int selected = Random.Range(0, appearances.Length);
        for (int i = 0; i < appearances.Length; i++)
            if (appearances[i] != null) appearances[i].SetActive(i == selected);
    }

    public void ShowTutorial()
    {
        if (hint == null || input == null || input.scrapTutorialShown) return;
        input.scrapTutorialShown = true;
        hint.text = "DROPPED SCRAP\nLeft click: collect\n+" + scrapAmount + " SCRAP";
        hint.gameObject.SetActive(true);
        hintTime = 7f;
    }

    private void Update()
    {
        if (hint == null || !hint.gameObject.activeSelf) return;
        hintTime -= Time.unscaledDeltaTime;
        // Keep the text above the pickup even when the rigidbody rolls over.
        hint.transform.position = transform.position + Vector3.up * 1.1f;
        if (input != null && input.viewCamera != null)
            hint.transform.rotation = input.viewCamera.transform.rotation;
        if (hintTime <= 0f) hint.gameObject.SetActive(false);
    }

    public string Collect()
    {
        if (collected || resources == null) return "Cannot collect this can.";
        collected = true;
        resources.AddGold(scrapAmount);
        if (cell != null && cell.scrapPickup == this) cell.scrapPickup = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
        return "Recycled can: +" + scrapAmount + " SCRAP.";
    }

    private void OnDestroy()
    {
        if (cell != null && cell.scrapPickup == this) cell.scrapPickup = null;
    }
}
