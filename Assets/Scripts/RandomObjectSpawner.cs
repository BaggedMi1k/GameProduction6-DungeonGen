using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    [Range(0f, 100f)]
    public float spawnChance = 50f; // 1 in 2 chance

    public bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            TrySpawn();
        }
    }

    public void TrySpawn()
    {
        // Roll chance
        float roll = Random.value;

        if (roll <= spawnChance && prefabs.Length > 0)
        {
            // Pick random prefab
            int index = Random.Range(0, prefabs.Length);
            GameObject prefabToSpawn = prefabs[index];

            // Spawn at this spawner's position
            Instantiate(prefabToSpawn, transform.position, transform.rotation);
        }
    }
}
