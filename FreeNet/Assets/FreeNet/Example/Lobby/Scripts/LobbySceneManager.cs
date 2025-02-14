using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static NetworkSpawner;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;
using Unity.Services.Lobbies.Models;

public class LobbySceneManager : NetworkBehaviour
{
    LobbyManager _lobbyManager;
    BasicNetworkSpwaner _basicNetworkSpwaner;
    public Dictionary<ulong, string> _cahsedClientIDMapping;
    public event Action _onSendPUIDRpc;



    private IEnumerator Start()
    {
        yield return SingletonMonoBehaviour<LobbyManager>.WaitInitialize();
        _lobbyManager = LobbyManager._instance;

       _basicNetworkSpwaner = GetComponent<BasicNetworkSpwaner>();
        _basicNetworkSpwaner._onSpawned += OnSpawnerCreated;
    }

    void OnSpawnerCreated()
    {
        _basicNetworkSpwaner.SpawnObject();
        _basicNetworkSpwaner.Spawn(new SpawnParams()
        {
            pos = Vector3.zero,
            destroyWithScene = true,
            rot = Quaternion.identity,
            prefabListName = "Lobbyprefabs",
            prefabName = "PlayZone"
        });
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void SendPUIDRpc(string puid, RpcParams rpcParams = default)
    {
        _cahsedClientIDMapping.Add(rpcParams.Receive.SenderClientId, puid);
        _onSendPUIDRpc?.Invoke();
    }

    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = true)]
    public void StartGameRpc(string serverPuid,string code, RpcParams rpcParams = default)
    {
        string localPUID = EOS_LocalUser._instance._localPUID._localpuid;
        if (localPUID == serverPuid)
        {
            NetworkManager.Singleton.Shutdown();
            FreeNet._instance.GetComponent<EOSNetcodeTransport>().StartHost(localPUID, code);
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            FreeNet._instance.GetComponent<EOSNetcodeTransport>().StartClient(localPUID, serverPuid, code);
        }
    }
    public override void OnDestroy()
    {
        _basicNetworkSpwaner._onSpawned -= OnSpawnerCreated;
    }
}
