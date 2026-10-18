using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopBuildInput : MonoBehaviour
{
    public Camera viewCamera;
    public TMP_Text instructions;
    public BaseHealth baseHealth;
    public int selectedTower;
    public bool battleStarted;
    public bool enableMouseBuilding = true;
    public string extraInstructions;
    public SimpleWaveController waveController;

    private bool showHelp;
    private string lastMessage;
    private float messageUntil;
    private string message = "Build ranged towers on ground, then press SPACE.";
    public TowerRangeIndicator placementPreview;
    private TowerBuildSlot hoveredSlot;
    [HideInInspector] public bool scrapTutorialShown;
    [HideInInspector] public bool shatterTutorialShown;
    [HideInInspector] public bool sodaTutorialShown;
    public float placementYaw;
    [HideInInspector] public bool fanTutorialShown;
    [HideInInspector] public bool amplifierTutorialShown;
    private TowerDamageBoost[] damagePreviewReceivers = new TowerDamageBoost[0];
    private TowerCooling[] previewReceivers = new TowerCooling[0];
    [HideInInspector] public bool[] upgradeTutorialShown = new bool[2];

    private void Start()
    {
        Time.timeScale = 1f;
        battleStarted = false;
        selectedTower = Mathf.Clamp(selectedTower, 0, 4);
        RefreshText();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.hKey.wasPressedThisFrame) showHelp = !showHelp;
            if (keyboard.digit1Key.wasPressedThisFrame)
                selectedTower = 0;
            if (keyboard.digit2Key.wasPressedThisFrame)
                selectedTower = 1;
            if (keyboard.digit3Key.wasPressedThisFrame)
                selectedTower = 2;
            if (keyboard.digit4Key.wasPressedThisFrame)
                selectedTower = 3;
            if (keyboard.digit5Key.wasPressedThisFrame)
                selectedTower = 4;
            if (keyboard.spaceKey.wasPressedThisFrame && waveController != null)
            {
                message = waveController.RequestNextWave();
                battleStarted = waveController.waveActive;
            }
            else if (keyboard.spaceKey.wasPressedThisFrame && !battleStarted)
            {
                battleStarted = true;
                Time.timeScale = 1f;
                message = "Battle started. You can still build or sell.";
            }
        }

        Mouse mouse = Mouse.current;
        foreach (TowerCooling receiver in previewReceivers)
            if (receiver != null) receiver.preview = false;
        foreach (TowerDamageBoost receiver in damagePreviewReceivers)
            if (receiver != null) receiver.preview = false;
        if (hoveredSlot != null)
        {
            hoveredSlot.Highlight(false, selectedTower);
            if (hoveredSlot.builtTower != null)
            {
                DamageAmplifier oldAmplifier = hoveredSlot.builtTower.GetComponent<DamageAmplifier>();
                if (oldAmplifier != null) oldAmplifier.SetHovered(false, this);
                CoolingFan oldFan = hoveredSlot.builtTower.GetComponent<CoolingFan>();
                if (oldFan != null) oldFan.SetHovered(false, this);
                SodaTower oldSoda = hoveredSlot.builtTower.GetComponent<SodaTower>();
                if (oldSoda != null) oldSoda.SetHovered(false, this);
                TowerSpeedUpgrade oldUpgrade = hoveredSlot.builtTower.GetComponent<TowerSpeedUpgrade>();
                if (oldUpgrade != null) oldUpgrade.SetHovered(false, this);
                TowerRangeIndicator oldRange = hoveredSlot.builtTower.GetComponent<TowerRangeIndicator>();
                if (oldRange != null) oldRange.showRange = false;
            }
        }
        hoveredSlot = null;
        if (placementPreview != null) placementPreview.showRange = false;
        if (enableMouseBuilding && mouse != null && viewCamera != null && (baseHealth == null || baseHealth.lives > 0))
        {
            bool build = mouse.leftButton.wasPressedThisFrame;
            bool sell = mouse.rightButton.wasPressedThisFrame;
            {
                Ray ray = viewCamera.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    ScrapPickup pickup = hit.collider.GetComponentInParent<ScrapPickup>();
                    if (pickup != null)
                    {
                        pickup.input = this;
                        pickup.ShowTutorial();
                        if (build) message = pickup.Collect();
                        RefreshText();
                        return;
                    }
                    TowerBuildSlot slot = hit.collider.GetComponent<TowerBuildSlot>();
                    DamageAmplifier hitAmplifier = hit.collider.GetComponentInParent<DamageAmplifier>();
                    if (hitAmplifier != null) slot = hitAmplifier.buildSlot;
                    CoolingFan hitFan = hit.collider.GetComponentInParent<CoolingFan>();
                    if (hitFan != null) slot = hitFan.buildSlot;
                    SodaTower hitSoda = hit.collider.GetComponentInParent<SodaTower>();
                    if (hitSoda != null) slot = hitSoda.buildSlot;
                    TowerSpeedUpgrade hitUpgrade = hit.collider.GetComponentInParent<TowerSpeedUpgrade>();
                    if (hitUpgrade != null) slot = hitUpgrade.buildSlot;
                    if (slot != null)
                    {
                        hoveredSlot = slot;
                        if (slot.builtTower == null && selectedTower == 2)
                        {
                            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                                placementYaw = (placementYaw + 90f) % 360f;
                            slot.buildPoint.rotation = Quaternion.Euler(0f, placementYaw, 0f);
                        }
                        if (build || sell) message = build ? slot.Build(selectedTower) : slot.Sell();
                        slot.Highlight(true, selectedTower);
                        if (slot.builtTower != null)
                        {
                            DamageAmplifier amplifier = slot.builtTower.GetComponent<DamageAmplifier>();
                            if (amplifier != null)
                            {
                                amplifier.SetHovered(true, this);
                                if (!build && !sell && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                                    message = amplifier.Upgrade();
                            }
                            CoolingFan fan = slot.builtTower.GetComponent<CoolingFan>();
                            if (fan != null)
                            {
                                fan.SetHovered(true, this);
                                if (!build && !sell && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                                    message = fan.Upgrade();
                            }
                            SodaTower soda = slot.builtTower.GetComponent<SodaTower>();
                            if (soda != null)
                            {
                                soda.SetHovered(true, this);
                                if (!build && !sell && keyboard != null)
                                {
                                    if (keyboard.rKey.wasPressedThisFrame) soda.RotateQuarterTurn();
                                    if (keyboard.eKey.wasPressedThisFrame) message = soda.Upgrade();
                                }
                            }
                            TowerSpeedUpgrade upgrade = slot.builtTower.GetComponent<TowerSpeedUpgrade>();
                            if (upgrade != null)
                            {
                                upgrade.SetHovered(true, this);
                                if (!build && !sell && keyboard != null && keyboard.eKey.wasPressedThisFrame)
                                    message = upgrade.Upgrade();
                            }
                            TowerRangeIndicator range = slot.builtTower.GetComponent<TowerRangeIndicator>();
                            if (range != null) range.showRange = true;
                        }
                        else if (placementPreview != null && slot.PlacementError(selectedTower) == null)
                        {
                            placementPreview.transform.position = slot.transform.position;
                            placementPreview.targeting = slot.towerPrefabs[selectedTower];
                            SodaTower previewSoda = slot.towerPrefabs[selectedTower].GetComponent<SodaTower>();
                            placementPreview.coneAngle = previewSoda != null ? previewSoda.coneAngle : 360f;
                            placementPreview.transform.rotation = slot.buildPoint.rotation;
                            placementPreview.showRange = true;
                            if (selectedTower == 4)
                            {
                                damagePreviewReceivers = FindObjectsByType<TowerDamageBoost>();
                                foreach (TowerDamageBoost receiver in damagePreviewReceivers)
                                {
                                    Vector3 distance = receiver.transform.position - slot.transform.position;
                                    distance.y = 0f;
                                    receiver.preview = distance.sqrMagnitude <= placementPreview.targeting.range * placementPreview.targeting.range;
                                }
                            }
                            if (selectedTower == 3)
                            {
                                previewReceivers = FindObjectsByType<TowerCooling>();
                                foreach (TowerCooling receiver in previewReceivers)
                                {
                                    Vector3 distance = receiver.transform.position - slot.transform.position;
                                    distance.y = 0f;
                                    receiver.preview = distance.sqrMagnitude <= placementPreview.targeting.range * placementPreview.targeting.range;
                                }
                            }
                        }
                    }
                }
            }
        }
        RefreshText();
    }

    private void RefreshText()
    {
        if (instructions == null)
            return;
        if (!enableMouseBuilding)
        {
            instructions.text = "GRAB FROM SHELF: BUY   |   RELEASE ON GREEN CELL: PLACE"
                + "\nGrab placed tower to move free   |   SPACE: next wave";
            return;
        }
        string tower = selectedTower == 0 ? "BOTTLE CAP" : selectedTower == 1 ? "SPRING PUNCH" : selectedTower == 2 ? "SODA SPRAYER" : selectedTower == 3 ? "COOLING FAN" : "DAMAGE AMPLIFIER";
        if (message != lastMessage)
        {
            lastMessage = message;
            messageUntil = Time.unscaledTime + 3f;
        }
        int cost = hoveredSlot != null ? hoveredSlot.buildCost : 20;
        instructions.text = "<b>" + (selectedTower + 1) + "  " + tower + "</b>  |  " + cost + " SCRAP";
        if (hoveredSlot != null && hoveredSlot.builtTower != null)
        {
            instructions.text += "\nE Upgrade   Right Sell +" + hoveredSlot.sellRefund;
            if (hoveredSlot.builtTower.GetComponent<SodaTower>() != null) instructions.text += "   R Rotate";
        }
        else instructions.text += "\n1-5 Select   Left Build / Collect";
        instructions.text += "   H Help";
        if (waveController != null && !waveController.waveActive && baseHealth != null && baseHealth.lives > 0
            && waveController.currentWave < waveController.enemyCounts.Length - 1)
            instructions.text += "\nSPACE · Next wave";
        if (showHelp)
            instructions.text += "\nSPACE: next wave   R: rotate soda 90 degrees"
                + "\n1 Cap   2 Punch   3 Soda   4 Fan   5 Amplifier"
                + "\nGreen: can build   Red: unavailable   Keep roads clear";
        if (Time.unscaledTime < messageUntil)
            instructions.text += "\n<color=#FFD580>" + message + "</color>";

    }

    private void OnDisable()
    {
        // Never leave the editor or the next scene paused.
        Time.timeScale = 1f;
    }
}
