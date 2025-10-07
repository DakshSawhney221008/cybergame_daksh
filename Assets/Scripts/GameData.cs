using NUnit.Framework;
using PurrNet;
using PurrNet.Packing;
using System;
using UnityEngine;

public class GameData :NetworkBehaviour
{
    public static GameData Instance;
    public SyncDictionary<ulong, string> nicknames = new SyncDictionary<ulong, string>();
    public SyncDictionary<ulong, int> scores = new SyncDictionary<ulong, int>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
