using System;
using TMPro;
using UnityEngine;

public class BasicTransitionUI : MonoBehaviour 
{
    [SerializeField]
    public TextMeshProUGUI _waitInfo;
    [SerializeField]
    public TextMeshProUGUI _waitInfoDetail;
    [SerializeField]
    public GameObject _BasicPannel;

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
