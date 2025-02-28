using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    public struct SpawnParams : INetworkSerializable
    {
       public Vector3 pos;
       public bool destroyWithScene;
       public Quaternion rot;
       public string prefabListName;
       public string prefabName;
       public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref pos);
            serializer.SerializeValue(ref destroyWithScene);
            serializer.SerializeValue(ref rot);
            serializer.SerializeValue(ref prefabListName);
            serializer.SerializeValue(ref prefabName);
       }
    }
    Dictionary<string, Dictionary<string,NetworkPrefab>> _prefabs;
    public event Action _onSpawned;

    private void Awake()
    {
        if(IsServer)
        {
            GetComponent<NetworkObject>().Spawn();
        }
    }
    void UpdatePrefabList()
    {
        _prefabs = new Dictionary<string, Dictionary<string, NetworkPrefab>>();
        List<NetworkPrefabsList> list = NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists;
        foreach (var networkPrefabList in list)
        {
            var dict = new Dictionary<string, NetworkPrefab>();
            _prefabs.Add(networkPrefabList.name, dict);
            foreach (var networkPrefab in networkPrefabList.PrefabList)
            {
                dict.Add(networkPrefab.Prefab.name, networkPrefab);
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        UpdatePrefabList();
        _onSpawned?.Invoke();
    }
    public void Spawn(SpawnParams param, bool transferOwnership = false, ulong clientID = 0)
    {
        if (!IsServer) return;
        if (_prefabs.TryGetValue(param.prefabListName, out var prefabList))
        {
            if (prefabList.TryGetValue(param.prefabName, out var networkPrefab))
            {
                GameObject playerInstance = Instantiate(networkPrefab.Prefab, param.pos, param.rot);
                NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                if (transferOwnership)
                {
                    networkObject.SpawnWithOwnership(clientID);
                }
                else
                {
                    networkObject.Spawn();
                }
            }
        }
    }
    [Rpc(SendTo.Server,RequireOwnership = false)]
    public void SpawnObjectRpc(bool transferOwnership, SpawnParams param, RpcParams rpcParams = default)
    {
        Spawn(param, transferOwnership, rpcParams.Receive.SenderClientId);
    }
}