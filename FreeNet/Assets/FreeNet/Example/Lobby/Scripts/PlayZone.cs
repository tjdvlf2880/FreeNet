using Epic.OnlineServices;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayZone : NetworkBehaviour
{
    [SerializeField]
    int _gamePlayerNum;
    Renderer _renderer;
    private HashSet<ulong> _playerNetworkIDs;
    LobbySceneManager _lobbySceneManager;

    public bool _isReady { get; private set; }

    private void Awake()
    {
        _playerNetworkIDs = new HashSet<ulong>();
    }

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = Color.white;
    }
    public override void OnDestroy()
    {
        //_lobbySceneManager._onSendPUIDRpc -= OnSendPUIDRpc;
    }
    public override void OnNetworkSpawn()
    {
        _lobbySceneManager = Object.FindAnyObjectByType<LobbySceneManager>();
        //_lobbySceneManager._onSendPUIDRpc += OnSendPUIDRpc;
    }

    void OnSendPUIDRpc()
    {
        if(_gamePlayerNum == _playerNetworkIDs.Count)
        { 
            List<NetworkObject> readyPlayers = new List<NetworkObject>();
            readyPlayers.Clear();
            foreach (ulong networkObjectId in _playerNetworkIDs)
            {
                if (FreeNet._instance._ngoManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
                {
                    if (_lobbySceneManager._cahsedClientIDMapping.ContainsKey(netObj.OwnerClientId))
                    {
                        readyPlayers.Add(netObj);
                    }
                }
            }
            if(readyPlayers.Count == _playerNetworkIDs.Count)
            {
                var serverRoleObj = readyPlayers[0];
                var code = EOS_SingleLobbyManager._instance.GenerateLobbyCode();
                foreach (NetworkObject netobj in readyPlayers)
                {
                    if(_lobbySceneManager._cahsedClientIDMapping.TryGetValue(serverRoleObj.OwnerClientId,out var puid))
                    {
                        var targetClient = new RpcParams
                        {
                            Send = new RpcSendParams
                            {
                                Target = RpcTarget.Single(netobj.OwnerClientId,RpcTargetUse.Temp)
                            }
                        };
                        //_lobbySceneManager.StartGameRpc(puid, code, targetClient);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (_playerNetworkIDs.Count == _gamePlayerNum)
        {
            _renderer.material.color = Color.red;
        }
        else
        {
            _renderer.material.color = Color.white;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj == null) return;

            ulong networkID = netObj.NetworkObjectId;

            if (!_playerNetworkIDs.Contains(networkID) && _playerNetworkIDs.Count < _gamePlayerNum)
            {
                _playerNetworkIDs.Add(networkID);
                if (_playerNetworkIDs.Count == _gamePlayerNum)
                {
                    _isReady = true;
                    foreach (ulong networkObjectId in _playerNetworkIDs)
                    {
                        if (FreeNet._instance._ngoManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out netObj))
                        {
                            var player = netObj.GetComponent<Player>();
                            player.RequestPUIDRpc();
                        }
                    }
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj == null) return;

            ulong networkID = netObj.NetworkObjectId;

            if (_playerNetworkIDs.Contains(networkID))
            {
                _isReady = false;
                _playerNetworkIDs.Remove(networkID);
                Debug.Log($"플레이어 {networkID} 충돌 해제. 현재 수집된 플레이어: {_playerNetworkIDs.Count}");
            }
        }
    }
}
