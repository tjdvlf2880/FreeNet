using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EOS_SingleLobbyManager;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _LobbyInfo;
    [SerializeField]
    TMP_Dropdown _LobbyType;
    [SerializeField]
    TMP_Dropdown _Lobbymember;
    [SerializeField]
    Button _CreateButton;
    public event Action onClickCreateButton;

    private void Awake()
    {
        _CreateButton.onClick.AddListener(OnClick);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _CreateButton.onClick.RemoveListener(OnClick);
    }
    void OnClick()
    {
        onClickCreateButton?.Invoke();
    }

    public uint GetLobbymemberNum()
    {
        switch (_Lobbymember.value)
        {
            case 0:
                return 1;
            case 1:
                return 2;
            case 2:
                return 4;
            case 3:
                return 8;
            case 4:
                return 16;
        }
        return 1;
    }
    public LobbySecurityType GetLobbyType()
    {
        switch(_LobbyType.value)
        {
            case 0:
                return LobbySecurityType.Public;
            case 1:
                return LobbySecurityType.Protected;
        }
        return LobbySecurityType.Public;
    }
    public string GetLobbyInfo()
    {
        return _LobbyInfo.text;
    }

}
