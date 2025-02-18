using UnityEngine;
using UnityEngine.UI;

public class ScrollControl : MonoBehaviour
{
    [SerializeField]
    public RectTransform _contentRectTransform;
    [SerializeField]
    ScrollRect _scrollView;
    [SerializeField]
    GameObject _viewPort;
    void Start()
    {
        _scrollView.content = _contentRectTransform;
    }
    public void RemoveContent(GameObject obj)
    {
        foreach (Transform child in _viewPort.transform)
        {
            if (child.gameObject == obj)
            {
                child.transform.parent = null;
                return;
            }
        }
    }
    public void AddContent(GameObject obj)
    {
        obj.transform.SetParent(_viewPort.transform);
    }
}
