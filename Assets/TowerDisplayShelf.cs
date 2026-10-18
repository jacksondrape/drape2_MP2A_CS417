using TMPro;
using UnityEngine;

public class TowerDisplayShelf : MonoBehaviour
{
    public PlayerResources resources;
    public PurchasedTowerGrab towerPrefab;
    public Transform displayPoint;
    public TMP_Text label;
    public int price = 20;
    public string towerName = "BOTTLE CAP";
    private PurchasedTowerGrab sample;
    [System.NonSerialized] public bool tutorialShown;

    private void Update()
    {
        if (sample == null || sample.purchased)
        {
            sample = Instantiate(towerPrefab, displayPoint.position, displayPoint.rotation);
            sample.shelf = this;
            sample.resources = resources;
        }
        if (label != null && resources != null)
            label.text = towerName + "\nGRAB: " + price + " SCRAP\nBALANCE " + resources.gold
                + (resources.gold < price ? "\n<color=#FF8888>NOT ENOUGH SCRAP</color>" : "\nGrab to buy · Release on a green cell");
    }
}
