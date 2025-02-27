using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class TransitionUI : MonoBehaviour
{
    [SerializeField]
    GameObject _UIRoot;
    [SerializeField]
    CanvasGroup _Canvas;
    CoroutineHandler _transitionUICoroutine;
    LinkedList<Transition> _transitionLists;
    Dictionary<string, Transition> _transitionDicts;
    public abstract class Transition 
    {
        public bool _isDone { get; protected set; }
        public string _transitionName { get; protected set; }
        public void SetDone(bool b)
        {
            _isDone = b;
        }
        public virtual void OnStart()
        {

        }
        public virtual void OnEnd()
        {

        }
        public virtual IEnumerator StartTransition()
        {
            while(!_isDone)
            {
                yield return null;
            }
        }
    
    
    }
    class OpenTransition : Transition
    {
        GameObject _UIRoot;
        CanvasGroup _Canvas;
        public OpenTransition(GameObject uIRoot, CanvasGroup canvas)
        {
            _UIRoot = uIRoot;
            _Canvas = canvas;
            _transitionName = "OpenTransition_Internal";
        }
        public override IEnumerator StartTransition()
        {
            _Canvas.alpha = 0;
            while (_Canvas.alpha != 1)
            {
                _Canvas.alpha += Time.deltaTime * 2;
                if (_Canvas.alpha > 1) _Canvas.alpha = 1;
                yield return null;
            }
            SetDone(true);
        }
    }
    class CloseTransition : Transition
    {
        [SerializeField]
        CanvasGroup _Canvas;
        [SerializeField]
        GameObject _UIRoot;
        public CloseTransition(GameObject uIRoot, CanvasGroup canvas)
        {
            _UIRoot = uIRoot;
            _Canvas = canvas;
            _transitionName = "CloseTransition_Internal";
        }
        public override IEnumerator StartTransition()
        {
            _Canvas.alpha = 1;
            while (_Canvas.alpha != 0)
            {
                _Canvas.alpha -= Time.deltaTime;
                if (_Canvas.alpha < 0) _Canvas.alpha = 0;
                yield return null;
            }
            _UIRoot.SetActive(false);
            SetDone(true);
        }
    }
    private void OnDestroy()
    {
        _transitionUICoroutine.StopCoroutine();
        _transitionUICoroutine = null;
    }
    protected void Awake()
    {
       Init();
    }
    protected void Init()
    {
        _transitionUICoroutine = new CoroutineHandler(this);
        _transitionLists = new LinkedList<Transition>();
        _transitionDicts = new Dictionary<string, Transition>();
    }
    public GameObject GetRootUI()
    {
        return _UIRoot;
    }
    public void AddTransition(Transition transition)
    {
        transition.SetDone(false);
        if (!_transitionDicts.TryAdd(transition._transitionName, transition))
        {
            UnityEngine.Debug.LogError("SameName Transition Set. The last one will be discarded.");
        }
        else
        {
            if (_transitionLists.Count == 0)
            {
                _transitionLists.AddLast(new OpenTransition(_UIRoot, _Canvas));
                _transitionLists.AddLast(transition);
                _transitionLists.AddLast(new CloseTransition(_UIRoot, _Canvas));
                _transitionUICoroutine.StartUniqueCoroutine((int id) => TransitionCoroutine(id));
            }
            else if (_transitionLists.Count > 0)
            {
                _transitionLists.AddBefore(_transitionLists.Last, transition);
            }
        }
    }
    public void StopWaitingUICoroutine()
    {
        _transitionUICoroutine.StopCoroutine();
    }
    IEnumerator TransitionCoroutine(int id)
    {
        while(_transitionLists.First !=null)
        {
            Transition transition = _transitionLists.First.Value;
            _transitionLists.RemoveFirst();
            transition.OnStart();
            while (!transition._isDone)
            {
                yield return transition.StartTransition();
            }
            _transitionDicts.Remove(transition._transitionName);
            transition.OnEnd();
        }
        _transitionUICoroutine.StopCoroutine();
    }
    public void MakeTransitionEnd(string name)
    {
        if (_transitionDicts.TryGetValue(name, out Transition transition))
        {
            transition.SetDone(true);
        }
        else
        {
            Debug.LogWarning("No Transition, it may be done.");
        }
    }
}
