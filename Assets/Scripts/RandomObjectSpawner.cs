using Unity.Netcode;
using UnityEngine;

public class RandomObjectSpawner : NetworkBehaviour
{
    public NetworkObject[] prefabs;

    [Range(0f, 100f)]
    public float spawnChance = 50f;

    public bool spawnOnStart = true;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Spawner spawned. IsServer = " + IsServer);
        Debug.Log($"Spawner prefab count = {prefabs?.Length ?? -1}");

        if (!IsServer)
            return;

        TrySpawn();
    }

    public void TrySpawn()
    {
        if (!IsServer) return;

        if (prefabs.Length == 0)
        {
            Debug.LogError("No prefabs assigned!");
            return;
        }

        int index = Random.Range(0, prefabs.Length);

        NetworkObject prefab = prefabs[index];

        NetworkObject obj =
            Instantiate(prefab, transform.position, transform.rotation);

        obj.Spawn();

        Debug.Log("Spawned: " + prefab.name);
    }
}