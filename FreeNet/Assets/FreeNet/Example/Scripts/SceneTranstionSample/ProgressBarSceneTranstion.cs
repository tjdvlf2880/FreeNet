public class ProgressBarSceneTranstion : SceneTranstion
{
    ProgressBarTransitionUI _progressBarTransitionUI;
    public ProgressBarSceneTranstion(string loadSceneName, ProgressBarTransitionUI ui)
        : base(loadSceneName)
    {
        _progressBarTransitionUI = ui;
    }
    protected override void SetUI()
    {
        _progressBarTransitionUI._slider.value = _progress;
    }
    public override void OnStart()
    {
        _progressBarTransitionUI.gameObject.SetActive(true);
    }
    public override void OnEnd()
    {
        _progressBarTransitionUI.gameObject.SetActive(false);
    }
}