using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PurchasedTowerGrab : XRGrabInteractable
{
    public TowerDisplayShelf shelf;
    public PlayerResources resources;
    public bool purchased;
    public TowerBuildSlot placedSlot;
    public float snapDistance = 0.55f;
    public float snapHeight = 1.2f;
    public TMP_Text vrStatus;
    private TowerBuildSlot[] slots;
    private TowerBuildSlot candidate;
    private TowerShooting shooting;
    private TowerTargeting targeting;
    private TowerCooling cooling;
    private TowerDamageBoost boost;
    private Rigidbody body;
    private bool releasing;
    private SodaTower soda;
    private CoolingFan fan;
    private DamageAmplifier amplifier;
    private float tutorialUntil;

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        ShowFirstTutorial();
    }

    private void ShowFirstTutorial()
    {
        if (shelf == null || shelf.tutorialShown) return;
        shelf.tutorialShown = true;
        tutorialUntil = Time.unscaledTime + 10f;
    }

    protected override void Awake()
    {
        base.Awake();
        shooting = GetComponent<TowerShooting>();
        targeting = GetComponent<TowerTargeting>();
        cooling = GetComponent<TowerCooling>();
        boost = GetComponent<TowerDamageBoost>();
        soda = GetComponent<SodaTower>();
        fan = GetComponent<CoolingFan>();
        amplifier = GetComponent<DamageAmplifier>();
        body = GetComponent<Rigidbody>();
        slots = FindObjectsByType<TowerBuildSlot>();
        SetActiveTower(false);
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return !releasing && base.IsSelectableBy(interactor)
            && (purchased || (shelf != null && resources != null && resources.gold >= shelf.price));
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        ShowFirstTutorial();
        if (!purchased)
        {
            if (shelf == null || resources == null || !resources.TrySpendGold(shelf.price))
            {
                interactionManager.SelectExit(args.interactorObject, this);
                return;
            }
            purchased = true;
        }
        if (placedSlot != null && placedSlot.builtTower == targeting) placedSlot.builtTower = null;
        placedSlot = null;
        AssignSlot(null);
        SetActiveTower(false);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (purchased && gameObject.activeInHierarchy) StartCoroutine(PlaceAfterRelease());
    }

    private void Update()
    {
        if (vrStatus != null)
        {
            bool tutorial = Time.unscaledTime < tutorialUntil;
            vrStatus.gameObject.SetActive(isHovered || isSelected || tutorial);
            if (vrStatus.gameObject.activeSelf)
            {
                if (Camera.main != null) vrStatus.transform.rotation = Camera.main.transform.rotation;
                int level = GetLevel();
                vrStatus.text = (shelf != null ? shelf.towerName : "MACHINE") + "  LV " + level + "/2"
                    + StatsText()
                    + (!purchased ? "\nGRAB TO BUY: " + (shelf != null ? shelf.price : 20) + " SCRAP"
                        : placedSlot == null ? "\nRelease on a free ground cell" : level < 2 ? "\nHAMMER: " + (15 * (level + 1)) + " SCRAP" : "\nMAX LEVEL")
                    + (purchased ? "\nMove free · Bin refund 10 SCRAP" : "")
                    + (tutorial ? "\n<color=#BDEEFF>" + TutorialText() + "</color>" : "");
            }
        }
        if (!isSelected) return;
        ClearPreview();
        candidate = NearestCell();
        if (candidate == null || candidate.cellRenderer == null) return;
        var color = new MaterialPropertyBlock();
        color.SetColor("_BaseColor", CanPlace(candidate) ? Color.green : Color.red);
        candidate.cellRenderer.SetPropertyBlock(color);
    }

    private string StatsText()
    {
        if (shooting != null) return "\nDMG " + shooting.EffectiveDamage.ToString("0.#")
            + " | " + shooting.EffectiveInterval.ToString("0.00") + "s / shot"
            + "\nBonus damage " + shooting.bonusDamage.ToString("0.#");
        if (soda != null) return "\nDPS " + soda.EffectiveDps.ToString("0.0") + " | Base " + soda.damagePerSecond.ToString("0.0");
        if (fan != null) return "\nSPEED x" + fan.speedMultiplier.ToString("0.00");
        if (amplifier != null) return "\nDMG x" + amplifier.damageMultiplier.ToString("0.00") + " | " + amplifier.connectedTowers + " towers"
            + (amplifier.activeSeconds > 0f ? "\nBOOST " + amplifier.activeSeconds.ToString("0.0") + "s"
                : "\nCHARGE " + amplifier.charge.ToString("0.0") + " / " + amplifier.RequiredChargeSeconds.ToString("0") + "s");
        return "";
    }

    private string TutorialText()
    {
        if (soda != null) return "Sprays a cone at several rats.\nRotate in your hand to aim; hammer boosts DPS.";
        if (fan != null) return "Nearby attack machines fire faster.\nWhite wind shows cooling; hammer boosts speed.";
        if (amplifier != null) return "Charges to boost nearby damage.\nMore machines slow charging; gold links show boost.";
        TowerSpeedUpgrade upgrade = GetComponent<TowerSpeedUpgrade>();
        return upgrade != null && upgrade.tutorialIndex == 1
            ? "Long-range shots strike distant rats.\nHammer increases damage and firing speed."
            : "Rapid shots target rats in range.\nHammer increases damage and firing speed.";
    }

    private TowerBuildSlot NearestCell()
    {
        TowerBuildSlot nearest = null;
        float distance = snapDistance * snapDistance;
        foreach (TowerBuildSlot slot in slots)
        {
            if (slot == null || slot.buildPoint == null) continue;
            Vector3 delta = transform.position - slot.buildPoint.position;
            if (Mathf.Abs(delta.y) > snapHeight) continue;
            delta.y = 0f;
            if (delta.sqrMagnitude > distance) continue;
            nearest = slot;
            distance = delta.sqrMagnitude;
        }
        return nearest;
    }

    private bool CanPlace(TowerBuildSlot slot)
    {
        return slot != null && !slot.blocked && !slot.isPath && slot.builtTower == null;
    }

    private IEnumerator PlaceAfterRelease()
    {
        releasing = true;
        yield return null; // Let XRI finish restoring the Rigidbody after detach.
        ClearPreview();
        TowerBuildSlot slot = NearestCell();
        if (CanPlace(slot))
        {
            transform.SetPositionAndRotation(slot.buildPoint.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
            body.isKinematic = true;
            body.useGravity = false;
            placedSlot = slot;
            slot.builtTower = targeting;
            targeting.enemies = slot.enemies;
            targeting.findEnemiesAutomatically = true;
            AssignSlot(slot);
            SetActiveTower(true);
        }
        else
        {
            body.isKinematic = false;
            body.useGravity = true;
        }
        releasing = false;
    }

    private void SetActiveTower(bool active)
    {
        if (shooting != null) shooting.enabled = active;
        if (soda != null) soda.enabled = active;
        if (fan != null) fan.enabled = active;
        if (amplifier != null) amplifier.enabled = active;
        AmplifierUVFlow flow = GetComponent<AmplifierUVFlow>();
        if (flow != null) flow.enabled = active;
        targeting.enabled = active && shooting != null;
        targeting.currentTarget = null;
        if (cooling != null) { cooling.enabled = active; cooling.source = null; cooling.speedMultiplier = 1f; }
        if (boost != null) { boost.enabled = active; boost.source = null; }
        MachineFeedback feedback = GetComponent<MachineFeedback>();
        if (feedback != null) feedback.enabled = active;
        TowerRangeIndicator range = GetComponent<TowerRangeIndicator>();
        if (range != null) range.showRange = false;
        foreach (ParticleSystem effect in GetComponentsInChildren<ParticleSystem>(true))
            if (!active) effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        foreach (LineRenderer line in GetComponentsInChildren<LineRenderer>(true))
            if (!active) line.enabled = false;
        if (!active && cooling != null && cooling.blueMarker != null) cooling.blueMarker.SetActive(false);
        if (!active && boost != null && boost.yellowMarker != null) boost.yellowMarker.SetActive(false);
        if (!active && soda != null && soda.label != null) soda.label.gameObject.SetActive(false);
        if (!active && fan != null && fan.label != null) fan.label.gameObject.SetActive(false);
        if (!active && amplifier != null && amplifier.label != null) amplifier.label.gameObject.SetActive(false);
    }

    private void AssignSlot(TowerBuildSlot slot)
    {
        TowerSpeedUpgrade upgrade = GetComponent<TowerSpeedUpgrade>();
        if (upgrade != null) { upgrade.resources = resources; upgrade.buildSlot = slot; }
        if (soda != null) { soda.resources = resources; soda.buildSlot = slot; }
        if (fan != null) { fan.resources = resources; fan.buildSlot = slot; }
        if (amplifier != null) { amplifier.resources = resources; amplifier.buildSlot = slot; }
    }

    public int GetLevel()
    {
        TowerSpeedUpgrade upgrade = GetComponent<TowerSpeedUpgrade>();
        return upgrade != null ? upgrade.upgradeLevel : soda != null ? soda.level : fan != null ? fan.level : amplifier != null ? amplifier.level : 0;
    }

    public string HammerUpgrade()
    {
        if (!purchased || isSelected || placedSlot == null) return "Place the machine before upgrading.";
        TowerSpeedUpgrade upgrade = GetComponent<TowerSpeedUpgrade>();
        if (upgrade != null) return upgrade.Upgrade();
        if (soda != null) return soda.Upgrade();
        if (fan != null) return fan.Upgrade();
        if (amplifier != null) return amplifier.Upgrade();
        return "Cannot upgrade this object.";
    }

    public bool Recycle()
    {
        if (!purchased || isSelected || resources == null) return false;
        purchased = false;
        resources.AddGold(10);
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }

    private void ClearPreview()
    {
        if (candidate != null && candidate.cellRenderer != null) candidate.cellRenderer.SetPropertyBlock(null);
        candidate = null;
    }
    protected override void OnDisable()
    {
        ClearPreview();
        base.OnDisable();
        if (placedSlot != null && placedSlot.builtTower == targeting) placedSlot.builtTower = null;
        releasing = false;
    }
}
