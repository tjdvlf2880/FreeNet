using System.Collections;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour
{
    public static IEnumerator WaitSpawn()
    {
        while (!_Spwaned)
        {
            yield return null;
        }
    }
    public static IEnumerator WaitInitialize()
    {
        while (!_Initialied)
        {
            yield return null;
        }
    }
    public static bool _Spwaned { get; private set; } = false;
    public static bool _Initialied { get; private set; } = false;
    public static T _instance { get; private set; }
    protected bool SingletonSpawn(T instance)
    {
        if (_instance == null)
        {
            _instance = instance;
            _Spwaned = true;
            DontDestroyOnLoad(this);
            return true;
        }
        gameObject.SetActive(false);
        Destroy(this);

        return false;
    }
    protected void SingletonInitialize()
    {
        _Initialied = true;
    }
    public virtual void OnRelease()
    {
        _Spwaned = false;
        _Initialied = false;
    }
}
