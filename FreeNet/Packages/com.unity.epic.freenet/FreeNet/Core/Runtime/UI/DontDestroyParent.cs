using UnityEngine;

public class DontDestroyParent : SingletonMonoBehaviour<DontDestroyParent>
{
    private void Awake()
    {
        if(SingletonSpawn(this))
        {
            SingletonInitialize();
        }
    }
}
