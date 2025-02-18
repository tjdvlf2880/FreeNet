using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;
public class LobbySceneManager : NetworkBehaviour
{
    LobbyManager _lobbyManager;
    BasicNetworkSpwaner _basicNetworkSpwaner;
    public Dictionary<ulong, EOSWrapper.ETC.PUID> _cahsedClientIDMapping;

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
        _basicNetworkSpwaner.Spawn(new NetworkSpawner.SpawnParams()
        {
            pos = Vector3.zero,
            destroyWithScene = true,
            rot = Quaternion.identity,
            prefabListName = "Lobbyprefabs",
            prefabName = "PlayZone"
        });
    }

    //[Rpc(SendTo.Server, RequireOwnership = true)]
    //public void SendPUIDRpc(EOSWrapper.ETC.PUID puid, RpcParams rpcParams = default)
    //{
    //    _cahsedClientIDMapping.Add(rpcParams.Receive.SenderClientId, puid);
    //    _onSendPUIDRpc?.Invoke();
    //}

    //[Rpc(SendTo.SpecifiedInParams, RequireOwnership = true)]
    //public void StartGameRpc(EOSWrapper.ETC.PUID serverPuid,string code, RpcParams rpcParams = default)
    //{
    //    var localPUID = EOS_LocalUser._instance._localPUID;
    //    if (localPUID == serverPuid)
    //    {
    //        NetworkManager.Singleton.Shutdown();
    //        FreeNet._instance.GetComponent<EOSNetcodeTransport>().StartHost(localPUID, code);
    //    }
    //    else
    //    {
    //        NetworkManager.Singleton.Shutdown();
    //        FreeNet._instance.GetComponent<EOSNetcodeTransport>().StartClient(localPUID, serverPuid, code);
    //    }
    //}
    public override void OnDestroy()
    {
        _basicNetworkSpwaner._onSpawned -= OnSpawnerCreated;
    }
}
