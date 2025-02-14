using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FindLobbyByCodeUI : MonoBehaviour
{
    [SerializeField]
    Button _findButton;
    [SerializeField]
    TMP_InputField _CodeInputField;
    public event Action _onfindButtonClicked;
    private void Awake()
    {
        _findButton.onClick.AddListener(OnClick);
    }

    public string GetCode()
    {
        return _CodeInputField.text;
    }

    void OnClick()
    {
        _onfindButtonClicked?.Invoke();
    }
}
