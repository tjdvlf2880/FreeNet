using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarTransitionUI : MonoBehaviour
{
    [SerializeField]
    public Slider _slider;
    [SerializeField]
    public TextMeshProUGUI _loadSceneInfo;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
}
