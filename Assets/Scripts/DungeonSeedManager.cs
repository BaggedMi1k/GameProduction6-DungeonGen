using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DunGen;
using Unity.Netcode;
using UnityEngine;

public class DungeonSeedManager : NetworkBehaviour
{
    public DungeonGenerator generator;

    [Header("Seed Settings")]
    public bool useCustomSeed = false;      // Toggle in inspector
    public int customSeed = 12345;          // Your desired seed

    private NetworkVariable<int> dungeonSeed = new NetworkVariable<int>(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone
    );

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            // Determine which seed to use
            int seedToUse = useCustomSeed ? customSeed : UnityEngine.Random.Range(0, 100000);

            dungeonSeed.Value = seedToUse;
            GenerateDungeon(seedToUse);
        }

        dungeonSeed.OnValueChanged += OnSeedChanged;

        // If client joins late
        if (!IsHost && dungeonSeed.Value != 0)
        {
            GenerateDungeon(dungeonSeed.Value);
        }
    }

    private void OnSeedChanged(int oldSeed, int newSeed)
    {
        GenerateDungeon(newSeed);
    }

    private void GenerateDungeon(int seed)
    {
        Debug.Log("Generating Dungeon with seed: " + seed);
        generator.Seed = seed;
        generator.Generate();
    }
}
