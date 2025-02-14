using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicTransition : TransitionUI.Transition
{
    BasicUI _basicUI;
    float _elapsedTime;
    float _endTime;
    string _waitInfoDetail;
    string _doneMsg;
    public BasicTransition(string transitionName, BasicUI basisUI, string waitInfoDetail, float endTime = 0.5f)
    {
        _transitionName = transitionName;
        _basicUI = basisUI;
        _waitInfoDetail = waitInfoDetail;
        _endTime = endTime;
    }
    public override IEnumerator StartTransition()
    {
        while (!_isDone)
        {
            _elapsedTime += Time.deltaTime;
            int quotient = (int)(_elapsedTime / 0.3) + 1;
            if(quotient > 4) quotient = 0;
            _basicUI._waitInfo.text = "Wait For " + new string('.', quotient);
            _basicUI._waitInfoDetail.text = _waitInfoDetail;
            yield return null;
        }
        _elapsedTime = 0;
        while(_elapsedTime < _endTime)
        {
            _elapsedTime += Time.deltaTime;
        }
    }
}
