using Epic.OnlineServices;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    [SerializeField]
    GameObject _singletonUI;

    LobbyControl _lobbyControl;
    TextMeshProUGUI _joinStateUI;

    FreeNet _freeNet;
    TransitionUI _transitionUI;
    BasicTransitionUI _basicTransitionUI;
    EOS_SingleLobbyManager.EOS_Lobby _lastLobby;
    //OpenLobbyUIKeyBinding _openLobbyUIKeyBinding;
    //InputBinding _inputBinding;

    private void Awake()
    {
        SingletonSpawn(this);
    }
    IEnumerator Start()
    {
        yield return SingletonMonoBehaviour<FreeNet>.WaitInitialize();
        yield return SingletonMonoBehaviour<SingletonCanvas>.WaitInitialize();
        _freeNet = FreeNet._instance;
        _transitionUI = SingletonCanvas._instance.GetComponentInChildren<TransitionUI>();   
        _basicTransitionUI = _transitionUI.GetRootUI().GetComponentInChildren<BasicTransitionUI>(true);
        _singletonUI.transform.SetParent(SingletonCanvas._instance.transform, true);
        _singletonUI.transform.SetAsFirstSibling();
        _lobbyControl = _singletonUI.GetComponentInChildren<LobbyControl>();
        _joinStateUI = _lobbyControl.transform.Find("JoinStateUI").GetComponent<TextMeshProUGUI>();

        UpdateLobbyStateUI();
        _lobbyControl._onJoined += OnJoined;
        _lobbyControl._onLeaved += OnLeaved;

        //_inputBinding = GetComponent<InputBinding>();   
        //_openLobbyUIKeyBinding = new OpenLobbyUIKeyBinding(_inputBinding, "OpenLobbyUIKeyBinding","l");
        //_inputBinding.EnableMap("OpenLobbyUIKeyBinding");
        //_openLobbyUIKeyBinding._onKeyInputChanged += OnOpenLobbyUIKey;

        SingletonInitialize();
    }
    void OnOpenLobbyUIKey(float val)
    {
        if(val == 1)
        {
            _lobbyControl.gameObject.SetActive(!_lobbyControl.gameObject.activeSelf);
        }
    }
    public void JoinLastLobby()
    {
        if (_lastLobby != null)
        {
            var transition = new BasicTransition("JoinLobby", _basicTransitionUI, "Joining Lobby...");
            _transitionUI.AddTransition(transition);
            _freeNet._singleLobbyManager.JoinLobby(_lastLobby._lobbyID, (result,lobby) =>
            {
                if(result == Result.Success)
                {
                    OnJoined(lobby);
                }
                _basicTransitionUI._waitInfoDetail.text = $"{result}";
                _transitionUI.MakeTransitionEnd("JoinLobby");
            });
        }
    }
    void UpdateLobbyStateUI()
    {
        if ((_lastLobby!=null) &&_lastLobby._joined)
        {
            if (_lastLobby.GetLobbyCode(out var code))
            {
                _joinStateUI.text = $"Joined Lobby {code}";
            }
        }
        else
        {
            _joinStateUI.text = "No Joined Lobby";
        }
    }
    void OnLeaved(EOS_SingleLobbyManager.EOS_Lobby lobby)
    {
        _freeNet._ngoManager.Shutdown();    
        _lastLobby = lobby;
        _lobbyControl.gameObject.SetActive(true);
        UpdateLobbyStateUI();
    }
    void OnJoined(EOS_SingleLobbyManager.EOS_Lobby lobby)
    {
        _lastLobby = lobby;
        _lobbyControl.gameObject.SetActive(false);
        if (lobby.GetLobbyCode(out string code))
        {
            _freeNet._ngoManager.OnClientStopped -= NGODisConnected;
            _freeNet._ngoManager.OnClientStarted -= NGOConnected;
            _freeNet._ngoManager.OnClientStopped += NGODisConnected;
            _freeNet._ngoManager.OnClientStarted += NGOConnected;
            var transition = new BasicTransition("NGOClientConnect", _basicTransitionUI, "NGO Client Connect...");
            _transitionUI.AddTransition(transition); 
            if (lobby._lobbyOwner.ToString() == lobby._localPUID.ToString())
            {
                _freeNet._ngoManager.StartHost(lobby._localPUID, code);
            }
            else
            {
                _freeNet._ngoManager.StartClient(lobby._localPUID, lobby._lobbyOwner, code);
            }
        }
        UpdateLobbyStateUI();
    }
    void NGODisConnected(bool b)
    {
        Debug.Log("NGO DisConnected");
    }
    void NGOConnected()
    {
        _basicTransitionUI._waitInfoDetail.text =  "NGO Connect Success";
        _transitionUI.MakeTransitionEnd("NGOClientConnect");

        SceneManager.LoadScene("LobbyScene");

    }

    public override void OnRelease()
    {
        _lobbyControl._onJoined -= OnJoined;
        //_openLobbyUIKeyBinding._onKeyInputChanged -= OnOpenLobbyUIKey;
        _freeNet._ngoManager.OnClientStopped -= NGODisConnected;
        _freeNet._ngoManager.OnClientStarted -= NGOConnected;
        Destroy(_lobbyControl);
    }
}
