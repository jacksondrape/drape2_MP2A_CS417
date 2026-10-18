using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWaveController : MonoBehaviour
{
    public EnemyHealth enemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;
    public BaseHealth baseHealth;
    public PlayerResources resources;

    public int[] enemyCounts = { 6, 8, 10, 12, 14, 16, 18, 22 };
    public float[] enemyHealth = { 6f, 9f, 13f, 18f, 24f, 32f, 42f, 56f };
    public float[] enemySpeeds = { 0.8f, 0.85f, 0.9f, 0.95f, 1f, 1.05f, 1.1f, 1.15f };
    public float spawnInterval = 1.5f;
    public int waveBonus = 15;

    public int currentWave = -1;
    public bool waveActive;
    public bool CombatActive => waveActive && baseHealth != null && baseHealth.lives > 0;

    private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private int enemiesWaitingToSpawn;
    private bool finished;

    public int EnemiesRemaining
    {
        get
        {
            RemoveMissingEnemies();
            return activeEnemies.Count + enemiesWaitingToSpawn;
        }
    }

    public string Status
    {
        get
        {
            if (baseHealth != null && baseHealth.lives <= 0)
                return "BASE LOST";
            if (finished)
                return "ALL WAVES CLEARED";
            if (waveActive)
                return "WAVE " + (currentWave + 1) + " / " + enemyCounts.Length;
            if (currentWave < 0)
                return "BUILD YOUR DEFENSES";
            return "WAVE CLEARED - BUILD PHASE";
        }
    }

    private void Update()
    {
        RemoveMissingEnemies();

        if (baseHealth != null && baseHealth.lives <= 0)
        {
            StopAllCoroutines();
            waveActive = false;
            return;
        }

        if (waveActive && enemiesWaitingToSpawn == 0 && activeEnemies.Count == 0)
        {
            waveActive = false;
            resources.AddGold(waveBonus);

            if (currentWave >= enemyCounts.Length - 1)
                finished = true;

        }
    }

    public string RequestNextWave()
    {
        if (waveActive)
            return "Wave already in progress.";
        if (finished)
            return "All waves are complete.";
        if (baseHealth == null || baseHealth.lives <= 0)
            return "The base has been lost.";
        if (enemyPrefab == null || spawnPoint == null || waypoints == null || waypoints.Length == 0)
            return "Wave references are missing.";

        currentWave++;
        waveActive = true;
        enemiesWaitingToSpawn = enemyCounts[currentWave];
        Time.timeScale = 1f;
        StartCoroutine(SpawnWave());
        return "Wave " + (currentWave + 1) + " started.";
    }

    private IEnumerator SpawnWave()
    {
        while (enemiesWaitingToSpawn > 0)
        {
            EnemyHealth enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            enemy.name = "Wave" + (currentWave + 1) + " Enemy";
            enemy.health = enemyHealth[currentWave];
            enemy.goldReward = 10;
            enemy.playerResources = resources;

            EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
            follower.moveSpeed = enemySpeeds[currentWave];
            follower.waypoints = waypoints;
            follower.baseHealth = baseHealth;
            // Ranged-only milestone: enemies should never stop for obstacles.
            follower.attackObstacles = false;

            activeEnemies.Add(enemy);
            enemiesWaitingToSpawn--;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void RemoveMissingEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
    }
}
