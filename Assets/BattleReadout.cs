using TMPro;
using UnityEngine;

public class BattleReadout : MonoBehaviour
{
    public PlayerResources resources;
    public BaseHealth baseHealth;
    public EnemyHealth[] enemies;
    public TMP_Text display;
    public DesktopBuildInput buildInput;
    public SimpleWaveController waveController;

    private void Update()
    {
        if (resources == null || baseHealth == null || display == null)
            return;

        int remaining = waveController != null ? waveController.EnemiesRemaining : 0;
        if (waveController == null && enemies != null)
        {
            foreach (EnemyHealth enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.health > 0f)
                    remaining++;
            }
        }

        string status = waveController != null ? waveController.Status : "DEFEND THE BASE";
        if (waveController == null && baseHealth.lives <= 0)
            status = "BASE LOST";
        else if (waveController == null && remaining == 0)
            status = "WAVE FINISHED";
        else if (waveController == null && buildInput != null && !buildInput.battleStarted)
            status = "BUILD YOUR DEFENSES";

        display.text = "<b>" + status + "</b>\nSCRAP  " + resources.gold
            + "     BASE  " + baseHealth.lives + "     RATS  " + remaining;
    }
}
