using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTranstion : TransitionUI.Transition
{
    AsyncOperation _asyncOperation;
    protected float _progress;
    public SceneTranstion(string loadSceneName)
    {
        _transitionName = $"Load Scene {loadSceneName}";
        _asyncOperation = SceneManager.LoadSceneAsync(loadSceneName);
        _asyncOperation.allowSceneActivation = false;
    }
    public override IEnumerator StartTransition()
    {
        while(!_isDone)
        {
            _progress = _asyncOperation.progress;
            SetUI();
            if (_progress >= 0.9f)
            {
                _asyncOperation.allowSceneActivation = true;
            }
            if (_asyncOperation.isDone)
            {
                SetDone(true);
            }
            yield return null;
        }
    }
    protected virtual void SetUI() { }



}
