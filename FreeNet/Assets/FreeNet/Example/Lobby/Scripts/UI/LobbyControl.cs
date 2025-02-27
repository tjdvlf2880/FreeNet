using Epic.OnlineServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EOS_SingleLobbyManager;

public class LobbyControl : MonoBehaviour
{
    [SerializeField]
    CreateLobbyUI _createLobbyUI;
    [SerializeField]
    LobbyListControl _lobbyListControl;
    [SerializeField]
    FindLobbyByCodeUI _findLobbyByCodeUI;
 
    FreeNet _freeNet;
    TransitionUI _transitionUI;
    BasicTransitionUI _basicTransitionUI;

    public event Action<EOS_Lobby> _onJoined;
    public event Action<EOS_Lobby> _onLeaved;

    private void Awake()
    {
        _createLobbyUI.onClickCreateButton += CreateLobby;
        _findLobbyByCodeUI._onfindButtonClicked += FindLobbyByCode;
        _lobbyListControl._onfindButtonClicked += FindPublicLobby;
        _lobbyListControl._onJoinButtonClicked += JoinFoundLobby;
        _lobbyListControl._onleaveButtonClicked += LeaveLobby;
    }
    IEnumerator Start()
    {
        yield return SingletonMonoBehaviour<FreeNet>.WaitInitialize();
        _freeNet = FreeNet._instance;
        yield return SingletonMonoBehaviour<SingletonCanvas>.WaitInitialize();
        _transitionUI = SingletonCanvas._instance.GetComponentInChildren<TransitionUI>();
        _basicTransitionUI = _transitionUI.GetRootUI().GetComponentInChildren<BasicTransitionUI>(true);
    }
    private void OnEnable()
    {
        _createLobbyUI.gameObject.SetActive(false);
        _lobbyListControl.gameObject.SetActive(true);
        _findLobbyByCodeUI.gameObject.SetActive(true);
    }
    void CreateLobby()
    {
        var transition = new BasicTransition("JoinLobby", _basicTransitionUI, "Joining Lobby...");
        _transitionUI.AddTransition(transition);
        _freeNet._singleLobbyManager.CreateLobby(_createLobbyUI.GetLobbymemberNum(), "Lobby",
                _createLobbyUI.GetLobbyType(), _createLobbyUI.GetLobbyInfo(),(result,lobby)=>
                {
                    HandleJoinLobbyResult(result, lobby);
                    _basicTransitionUI._waitInfoDetail.text = $"{result}";
                    _transitionUI.MakeTransitionEnd("JoinLobby");
                });
    }
    void FindPublicLobby()
    {
        var transition = new BasicTransition("FindLobby", _basicTransitionUI, "Finding Lobby...");
        _transitionUI.AddTransition(transition);
        _freeNet._singleLobbyManager.FindPublicLobby(10, "Lobby" ,onComplete: (Result result, List<LobbySearchResult> list) =>
        {
            if (result == Result.Success)
            {
                _lobbyListControl.SetLobbyList(list);
            }
            _basicTransitionUI._waitInfoDetail.text =  $"{result}";
            _transitionUI.MakeTransitionEnd("FindLobby");
        });
    }
    void FindLobbyByCode()
    {
        string code = _findLobbyByCodeUI.GetCode();
        var transition = new BasicTransition("FindLobbyByCode", _basicTransitionUI, "Finding Lobby...");
        _transitionUI.AddTransition(transition);
        _freeNet._singleLobbyManager.FindLobbyByCode(10,code,(Result result ,List<LobbySearchResult> list)=>
        {
            if (result == Result.Success)
            {
                _lobbyListControl.SetLobbyList(list);
            }
            _basicTransitionUI._waitInfoDetail.text = $"{result}";
            _transitionUI.MakeTransitionEnd("FindLobbyByCode");
        });
    }
    void JoinFoundLobby(LobbyInfoUI lobby)
    {
        var transition = new BasicTransition("JoinLobby", _basicTransitionUI, "Joining Lobby...");
        _transitionUI.AddTransition(transition);
        lobby._foundLobby.JoinLobby((result,lobby)=>
        {
            if (result == Result.Success)
            {
                HandleJoinLobbyResult(result, lobby);
            }
            _basicTransitionUI._waitInfoDetail.text = $"{result}";
            _transitionUI.MakeTransitionEnd("JoinLobby");
        });
    }
    void LeaveLobby()
    {
        var transition = new BasicTransition("LeaveLobby", _basicTransitionUI, "Leave Lobby...");
        _transitionUI.AddTransition(transition);
        _freeNet._singleLobbyManager.LeaveLobby((result,lobby)=>
        {
            HandleLeaveLobbyResult(result, lobby);
            _basicTransitionUI._waitInfoDetail.text = $"{result}";
            _transitionUI.MakeTransitionEnd("LeaveLobby");
        });
    }
    void HandleJoinLobbyResult(Result result,EOS_Lobby lobby)
    {
        _lobbyListControl.ReleasecurrentFoundLobbies();
        if(result == Result.Success)
        {
            if (lobby != null)
            { 
                _onJoined?.Invoke(lobby);
            }
        }
    }
    void HandleLeaveLobbyResult(Result result,EOS_Lobby lobby)
    {
        _lobbyListControl.ReleasecurrentFoundLobbies();
        if (result == Result.Success)
        {
            if(lobby != null)
            {
               _onLeaved?.Invoke(lobby);
            }
        }
    }
    private void OnDestroy()
    {
        _createLobbyUI.onClickCreateButton -= CreateLobby;
        _findLobbyByCodeUI._onfindButtonClicked -= FindLobbyByCode;
        _lobbyListControl._onJoinButtonClicked -= JoinFoundLobby;
        _lobbyListControl._onleaveButtonClicked -= LeaveLobby;
    }
}
