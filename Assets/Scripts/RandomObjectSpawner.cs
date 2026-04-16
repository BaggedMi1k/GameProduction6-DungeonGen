using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomObjectSpawner : NetworkBehaviour
{
    public NetworkObject[] prefabs;

    [Range(0f, 100f)]
    public float spawnChance = 50f;

    public bool spawnOnStart = true;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        TrySpawn();

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only run when the first client joins (or adjust for multiplayer)
        if (NetworkManager.ConnectedClients.Count >= 1)
        {
            TrySpawn();

            // prevent multiple spawns
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
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