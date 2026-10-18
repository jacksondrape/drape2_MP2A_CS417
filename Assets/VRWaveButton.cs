using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRWaveButton : XRSimpleInteractable
{
    public SimpleWaveController waves;
    public TMP_Text label;
    private float nextPress;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (waves == null || Time.unscaledTime < nextPress) return;
        nextPress = Time.unscaledTime + 0.5f;
        waves.RequestNextWave();
    }

    private void Update()
    {
        if (label != null && Camera.main != null) label.transform.rotation = Camera.main.transform.rotation;
        if (label != null && waves != null)
            label.text = waves.Status + (!waves.waveActive && waves.baseHealth.lives > 0
                && waves.currentWave < waves.enemyCounts.Length - 1 ? "\nSELECT: NEXT WAVE" : "");
    }
}
