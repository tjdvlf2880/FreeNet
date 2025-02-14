using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    LobbySceneManager _lobbySceneManager;

    void OnSpawnerCreated()
    {
        _lobbySceneManager = Object.FindAnyObjectByType<LobbySceneManager>();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void RequestPUIDRpc()
    {
        string puid = SingletonMonoBehaviour<EOS_LocalUser>._instance._localPUID._localpuid;
        _lobbySceneManager.SendPUIDRpc(puid);
    }
}
