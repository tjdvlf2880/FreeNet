using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EOS_SingleLobbyManager;

public class LobbyInfoUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _lobbyTypeText;
    [SerializeField]
    TextMeshProUGUI _lobbyInfoText;
    [SerializeField]
    TextMeshProUGUI _lobbyMemberText;
    [SerializeField]
    TextMeshProUGUI _lobbyCodeText;
    Button _button;
    Image _image;
    public EOS_SingleLobbyManager.LobbySearchResult _foundLobby { get; private set; }
    public event Action<LobbyInfoUI> _onClick;
    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }
    public void OnClick()
    {
        _onClick?.Invoke(this);
    }
    public void OffFocus()
    {
        SetColor(Color.white);
    }
    public void OnFocused()
    {
        SetColor(Color.green);
    }

    public void SetColor(Color color)
    {
        _image.color = color;
    }

    public void UpdateDetails(LobbySearchResult lobby)
    {
        _foundLobby = lobby;
        _lobbyMemberText.text = $"({_foundLobby._info.AvailableSlots}/{_foundLobby._info.MaxMembers})";
        if (_foundLobby._attribute.TryGetValue("LOBBYTYPE", out var typeVal))
        {
            string type = typeVal.Data.Value.Value.AsUtf8;
            _lobbyTypeText.text = type;
        }
        if (_foundLobby._attribute.TryGetValue("LOBBYCODE", out var codeVal))
        {
            string code = codeVal.Data.Value.Value.AsUtf8;
            _lobbyCodeText.text = code;
        }
        if (_foundLobby._attribute.TryGetValue("LOBBYINFO", out var infoVal))
        {
            string info = infoVal.Data.Value.Value.AsUtf8;
            _lobbyInfoText.text = info;
        }
    }
}
