using System.Collections;
using UnityEngine;

public class OreSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject[] orePrefabs;
    [SerializeField, Min(0.1f)] private float minSpawnDelay = 5f;
    [SerializeField, Min(0.1f)] private float maxSpawnDelay = 12f;

    private OreHealth currentOre;
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentOre == null)
            {
                float delay = Random.Range(
                    minSpawnDelay,
                    Mathf.Max(minSpawnDelay, maxSpawnDelay));

                yield return new WaitForSeconds(delay);

                if (currentOre == null)
                    SpawnOre();
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnOre()
    {
        if (orePrefabs == null || orePrefabs.Length == 0)
        {
            Debug.LogWarning($"No ore prefabs assigned to {name}.", this);
            return;
        }

        GameObject prefab = orePrefabs[
            Random.Range(0, orePrefabs.Length)];

        if (prefab == null) return;

        GameObject instance = Instantiate(
            prefab, transform.position, transform.rotation);

        currentOre = instance.GetComponentInChildren<OreHealth>();

        if (currentOre == null)
        {
            Debug.LogError(
                $"Prefab {prefab.name} needs an OreHealth component.", prefab);

            Destroy(instance);
            return;
        }

        currentOre.SetSpawnPoint(this);
    }

    public void NotifyOreDestroyed(OreHealth ore)
    {
        if (currentOre == ore)
            currentOre = null;
    }
}
