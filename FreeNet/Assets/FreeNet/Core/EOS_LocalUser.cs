using Epic.OnlineServices;
using UnityEngine;

public class EOS_LocalUser : SingletonMonoBehaviour<EOS_LocalUser>
{
    public EOSWrapper.ETC.PUID _localPUID;
    private void Awake()
    {
        if (SingletonSpawn(this))
        {
            SingletonInitialize();
        }
    }
    
}
