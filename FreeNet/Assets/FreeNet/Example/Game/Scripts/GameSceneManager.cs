using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameSceneManager
{
    LobbyManager _lobbyManager;
    TransitionUI _transitionUI;
    BasicUI _basicUI;
    IEnumerator Start()
    {
        yield return SingletonMonoBehaviour<LobbyManager>.WaitInitialize();
        yield return SingletonMonoBehaviour<SingletonCanvas>.WaitInitialize();
        _transitionUI = SingletonCanvas._instance.GetComponentInChildren<TransitionUI>();
        _lobbyManager = LobbyManager._instance;
        _basicUI = _transitionUI.GetRootUI().GetComponentInChildren<BasicUI>();
        _basicUI._waitInfoDetail.text = "Load Game Success";
        _transitionUI.MakeTransitionEnd("LoadGame");
    }

    public void EndGame()
    {
        NetworkManager.Singleton.Shutdown();
        _lobbyManager.JoinLastLobby();
    }
}
