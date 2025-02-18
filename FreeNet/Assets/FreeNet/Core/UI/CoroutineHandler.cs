using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CoroutineHandler
{
    MonoBehaviour _owner;
    Coroutine _coroutine;
    bool _IsStart;

    Queue<int> _coroutineID;
    int _lastID;
    public CoroutineHandler(MonoBehaviour owner)
    {
        _owner = owner;
        _coroutine = null;
        _IsStart = false;
        _lastID = 0;
        _coroutineID = new Queue<int>();
    }
    public void StopCoroutine()
    {
        if (_coroutine != null)
        {
            if(_coroutineID.TryDequeue(out var id))
            {
                Debug.Log($"Coroutine{id} Stopped ");
            }
            _owner.StopCoroutine(_coroutine);
            _coroutine = null;
            _IsStart = false;
        }
    }
    public void StartCoroutine(IEnumerator coroutine)
    {
        _coroutine = _owner.StartCoroutine(coroutine);
        _IsStart = true;
    }
    public int StartUniqueCoroutine(Func<int, IEnumerator> coroutineFunc, Action<int> onStart = null)
    {
        _lastID++;
        _coroutineID.Enqueue(_lastID);
        _owner.StartCoroutine(UniqueCoroutine(_lastID, coroutineFunc, onStart));
        return _lastID;
    }
    private IEnumerator UniqueCoroutine(int id, Func<int, IEnumerator> coroutineFunc, Action<int> onStart = null)
    {
        while (_IsStart)
        {
            yield return null;
        }
        if(_coroutineID.TryPeek(out var nextID))
        {
            if (id == nextID)
            {
                onStart?.Invoke(id);
                Debug.Log($"Coroutine{id} Start ");
                StartCoroutine(coroutineFunc(id));
            }
        }
    }
}
