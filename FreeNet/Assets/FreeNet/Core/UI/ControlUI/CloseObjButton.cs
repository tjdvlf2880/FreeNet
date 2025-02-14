using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CloseObjButton : MonoBehaviour
{

    [SerializeField] // Inspectorø° ≥Î√‚
    private List<GameObject> _ObjList = new List<GameObject>();
    Button _button;

    event Action onClick;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Onclick);
    }

    void Onclick()
    {
        foreach (GameObject obj in _ObjList)
        {
            obj.SetActive(false);
        }
        onClick?.Invoke();
    }    
    private void OnDestroy()
    {
        _button.onClick.RemoveListener(Onclick);
    }
}
