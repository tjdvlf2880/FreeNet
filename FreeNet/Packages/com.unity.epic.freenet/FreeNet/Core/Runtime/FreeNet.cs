using System.Collections;
using Unity.Netcode;
using UnityEngine;
public class FreeNet : SingletonMonoBehaviour<FreeNet>
{
    public EOS_Core _eosCore { get; private set; }
    public EOS_SingleLobbyManager _singleLobbyManager { get; private set; }
    public EOS_LocalUser _localUser { get; private set; }
    public NgoManager _ngoManager { get; private set; }

    private void Awake()
    {
        SingletonSpawn(this);
    }
    private void Start()
    {
        _ngoManager = GetComponent<NgoManager>();
        _localUser = GetComponent<EOS_LocalUser>();
        _singleLobbyManager = GetComponent<EOS_SingleLobbyManager>();
        _eosCore = GetComponent<EOS_Core>();
        _singleLobbyManager.Init(this);
        _ngoManager.Init(this);
        SingletonInitialize();
    }
    public override void OnRelease()
    {
        if (_eosCore._InitState == EOS_Core.InitState.Suceess)
        {
            _singleLobbyManager.OnRelease();
            _eosCore.OnRelease();
        }
    }
    private void OnApplicationQuit()
    {
        OnRelease();
    }
}
