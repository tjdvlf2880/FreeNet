using UnityEngine;

public class SingletonCanvas : SingletonMonoBehaviour<SingletonCanvas>
{
    private void Awake()
    {
        if(SingletonSpawn(this))
        {
            SingletonInitialize();
        }
    }

    public override void OnRelease()
    {

    }

}
