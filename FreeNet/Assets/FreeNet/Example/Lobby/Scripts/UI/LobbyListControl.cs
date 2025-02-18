using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EOS_SingleLobbyManager;

public class LobbyListControl : MonoBehaviour
{
    ScrollControl _scrollControl;
    [SerializeField]
    LobbyInfoUI _lobbyInfoPreb;
    [SerializeField]
    Button _findButton;
    [SerializeField]
    Button _joinButton;
    [SerializeField]
    Button _leaveButton;

    LobbyInfoUI _currentSelected;
    List<LobbyInfoUI> _currentLobbyInfoUI;
    public event Action<LobbyInfoUI> _onJoinButtonClicked;
    public event Action _onfindButtonClicked;
    public event Action _onleaveButtonClicked;

    private void Awake()
    {
        _scrollControl = GetComponent<ScrollControl>();
        _currentLobbyInfoUI = new List<LobbyInfoUI>();
        SetCurrentSelectedLobbyInfoUI(null);
        _findButton.onClick.AddListener(OnClickFindButton);
        _joinButton.onClick.AddListener(OnClickJoinButton);
        _leaveButton.onClick.AddListener(OnClickLeaveButton);
    }

    void SetCurrentSelectedLobbyInfoUI(LobbyInfoUI obj)
    {
        if (_currentSelected != obj)
        {
            if (_currentSelected != null)
            {
                _currentSelected.OffFocus();
                _currentSelected = null;
            }
            if(obj != null)
            {
                obj.OnFocused();
            }
            _currentSelected = obj;
        }
    }
    public void SetLobbyList(List<LobbySearchResult> list)
    {
        ReleasecurrentFoundLobbies();
        foreach (var item in list)
        {
            LobbyInfoUI lobby = Instantiate(_lobbyInfoPreb);
            _scrollControl.AddContent(lobby.gameObject);
            _currentLobbyInfoUI.Add(lobby); 
            lobby.UpdateDetails(item);
            lobby._onClick += OnClickLobbyInfo;
        }
    }
    void OnClickJoinButton()
    {
        if(_currentSelected !=null)
        {
            _onJoinButtonClicked?.Invoke( _currentSelected );
        }
    }
    void OnClickFindButton()
     {
        _onfindButtonClicked?.Invoke();
     }

    void OnClickLeaveButton()
    {
        _onleaveButtonClicked?.Invoke();
    }
    void OnClickLobbyInfo(LobbyInfoUI lobby)
    {
        SetCurrentSelectedLobbyInfoUI(lobby);
    }
    public void ReleasecurrentFoundLobbies()
    {
        foreach (var item in _currentLobbyInfoUI)
        {
            _scrollControl.RemoveContent(item.gameObject);
            item._onClick -= OnClickLobbyInfo;
            item._foundLobby.Release();
            Destroy(item);
        }
        _currentLobbyInfoUI.Clear();
        _currentSelected = null;
    }
    private void OnDestroy()
    {
        _joinButton.onClick.RemoveListener(OnClickJoinButton);
        _findButton.onClick.RemoveListener(OnClickFindButton);
        _leaveButton.onClick.AddListener(OnClickLeaveButton);
        ReleasecurrentFoundLobbies();
    }
}
