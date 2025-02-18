using System;
using UnityEngine;

public class BasicNetworkSpwaner : NetworkSpawner
{
    [SerializeField]
    GameObject _spawnerPrefab;
    [SerializeField]
    string _prefabListName;
    [SerializeField]
    string _prefabName;
    [SerializeField]
    Vector3 _position;
    [SerializeField]
    Vector3 _rotation;
    [SerializeField]
    bool _destroyWithScene;

    public void SpawnObject(bool transferOwnership = false, ulong clientID = 0)
    {
        var info = new SpawnParams()
        {
            pos = _position,
            destroyWithScene = _destroyWithScene,
            rot = Quaternion.Euler(_rotation),
            prefabListName = _prefabListName,
            prefabName = _prefabName
        };
        if (IsServer)
        {
            Spawn(info, transferOwnership, clientID);
        }
        else
        {
            SpawnObjectRpc(true, info);
        }

    }
}
