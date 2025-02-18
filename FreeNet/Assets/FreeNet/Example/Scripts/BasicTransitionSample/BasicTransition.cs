using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicTransition : TransitionUI.Transition
{
    BasicTransitionUI _basicTransitionUI;
    float _elapsedTime;
    float? _minDuration;
    float? _maxDuration;
    string _waitInfoDetail;
    string _doneMsg;
    public BasicTransition(string transitionName, BasicTransitionUI basisUI, string waitInfoDetail, float? minDuration = null, float? maxDuration = null)
    {
        _transitionName = transitionName;
        _basicTransitionUI = basisUI;
        _waitInfoDetail = waitInfoDetail;
        _minDuration = minDuration;
        _maxDuration = maxDuration;
    }
    public override IEnumerator StartTransition()
    {
        _elapsedTime = 0;
        while (true)
        {
            _elapsedTime += Time.deltaTime;
            SetUI();
            if(_maxDuration != null && _elapsedTime > _maxDuration.Value)
            {
                SetDone(true);
            }

            if (_isDone)
            {
                if (_minDuration != null && _elapsedTime < _minDuration.Value)
                {
                    
                }
                else
                {
                    break;
                }
            }
            yield return null;
        }

       
    }
    protected void SetUI()
    {
        int quotient = (int)(_elapsedTime / 0.3) + 1;
        if (quotient > 4) quotient = 0;
        _basicTransitionUI._waitInfo.text = "Wait For " + new string('.', quotient);
        _basicTransitionUI._waitInfoDetail.text = _waitInfoDetail;
    }
    public override void OnStart()
    {
        _basicTransitionUI.gameObject.SetActive(true);
    }
    public override void OnEnd()
    {
        _basicTransitionUI.gameObject.SetActive(false);
    }
}
