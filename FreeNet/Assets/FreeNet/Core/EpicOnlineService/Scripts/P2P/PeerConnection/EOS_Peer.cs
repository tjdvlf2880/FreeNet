using Epic.OnlineServices.P2P;
using System;
using UnityEngine;

public class EOS_Peer : MonoBehaviour
{
    public enum state
    {
        starting,
        start,
        stopping,
        stop
    }

    public EOS_Socket _socket { get; protected set; }
    protected EOS_Core _eosNet;
    public  byte _channel { get; protected set; }
    public state _state { get; protected set; }
    public event Action<state> _onStateChanged;

    public void ChangeState(state state)
    {
        if(state != _state)
        {
            _state = state;
            _onStateChanged?.Invoke(state);
        }
    }

    public virtual void OnClientEnqueuePacket(EOS_Socket.Connection connection) { }
    public virtual void OnMakeConnectionCB(EOS_Socket.Connection info) { }
    public virtual void OnClosedCB(OnRemoteConnectionClosedInfo info) { }
    public virtual bool StartConnection() { return true; }
    public virtual bool StopConnection() { return true; }
    public virtual void Shutdown() { }
}
