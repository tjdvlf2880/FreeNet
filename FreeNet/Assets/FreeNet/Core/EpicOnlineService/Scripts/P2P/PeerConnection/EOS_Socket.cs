using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System.Collections.Generic;
using System;
using UnityEngine;
using static EOS_Core;

public class EOS_Socket
{
    public EOSWrapper.ETC.PUID _localPUID { get; private set; }
    public SocketId _socketID { get; private set; }
    DoubleKeyDict<EOS_Core.Role, string, Connection> _Connections;
    public class Connection
    {
        public enum State
        {
            Connected,
            Disconnected,
        }
        public State _State { get; private set; }
        public EOS_Core.Role _remoteRole { get; private set; }
        public EOSWrapper.ETC.PUID _remotePUID { get; private set; }
        public event Action<Connection> _onEnqueuePacket;
        public event Action<Connection> _onConnectionStateChanged;
        private Queue<EOS_Core.EOS_Packet> _IncomingPackets;
        public void SetState(State state)
        {
            if (_State != state)
            {
                _State = state;
                _onConnectionStateChanged?.Invoke(this);
            }
        }
        public void Release()
        {
            SetState(State.Disconnected);
            _onEnqueuePacket = null;
            _onConnectionStateChanged = null;
        }
        public bool DeqeuePacket(out EOS_Core.EOS_Packet packet)
        {
            return _IncomingPackets.TryDequeue(out packet);
        }
        public void EnqueuePacket(EOS_Core.EOS_Packet packet)
        {
            _IncomingPackets.Enqueue(packet);
            _onEnqueuePacket?.Invoke(this);
        }

        public Connection(EOS_Core.Role role, string remotepuid = null)
        {
            _IncomingPackets = new Queue<EOS_Core.EOS_Packet>();
            _remoteRole = role;
            _remotePUID = new EOSWrapper.ETC.PUID(remotepuid);
            _State = State.Disconnected;
        }
    }
    #region callbacks
    public event Action<Connection> _onMakeConnection;
    public event Action<OnRemoteConnectionClosedInfo> _onClosed;
    event Action<OnIncomingConnectionRequestInfo> _onRequest;
    event Action<OnPeerConnectionEstablishedInfo> _onEstablished;
    event Action<OnPeerConnectionInterruptedInfo> _onInterrupted;

    #endregion
    #region EOScallbacks
    ulong _onRequestHandle;
    ulong _onEstablishedHandle;
    ulong _onInterruptedHandle;
    ulong _onClosedHandle;

    #endregion
    public EOS_Socket(EOSWrapper.ETC.PUID localpuid, string socketid)
    {
        _localPUID = localpuid;
        _socketID = new SocketId() { SocketName = socketid };
        _Connections = new DoubleKeyDict<Role, string, Connection>();
    }
    void AddRequestCB()
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _onRequest += OnRequest;
        if(_onRequestHandle == 0)
        {
            var Requestoptions = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = _localPUID._PUID,
                SocketId = _socketID
            };
            _onRequestHandle = eosNet._IP2P.AddNotifyPeerConnectionRequest(ref Requestoptions, Role.RemotePeer, (ref OnIncomingConnectionRequestInfo info) =>
            {
                _onRequest?.Invoke(info);
            });
        }
    }
    void AddClosedCB()
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _onClosed += OnClosed;
        if (_onClosedHandle == 0)
        {
            var ClosedOptions = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = _localPUID._PUID,
                SocketId = _socketID
            };
            _onClosedHandle = eosNet._IP2P.AddNotifyPeerConnectionClosed(ref ClosedOptions, Role.RemotePeer, (ref OnRemoteConnectionClosedInfo info) =>
            {
                _onClosed?.Invoke(info);
            });
        }
    }
    void AddEstablishedCB()
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _onEstablished += OnEstablished;
        if(_onEstablishedHandle == 0)
        {
            var options = new AddNotifyPeerConnectionEstablishedOptions()
            {
                LocalUserId = _localPUID._PUID,
                SocketId = _socketID
            };
            _onEstablishedHandle = eosNet._IP2P.AddNotifyPeerConnectionEstablished(ref options, Role.RemotePeer, (ref OnPeerConnectionEstablishedInfo info) =>
            {
                _onEstablished?.Invoke(info);
            });
        }
    }
    void AddInterruptedCB()
    {
        _onInterrupted += OnInterrupted;
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        if(_onInterruptedHandle==0)
        {
            var options = new AddNotifyPeerConnectionInterruptedOptions()
            {
                LocalUserId = _localPUID._PUID,
                SocketId = _socketID
            };
            _onInterruptedHandle = eosNet._IP2P.AddNotifyPeerConnectionInterrupted(ref options, Role.RemotePeer, (ref OnPeerConnectionInterruptedInfo info) =>
            {
                _onInterrupted?.Invoke(info);
            });
        }
    }

    public void RemoveCB()
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        eosNet._IP2P.RemoveNotifyPeerConnectionRequest(_onRequestHandle);
        eosNet._IP2P.RemoveNotifyPeerConnectionInterrupted(_onInterruptedHandle);
        eosNet._IP2P.RemoveNotifyPeerConnectionEstablished(_onEstablishedHandle);
        eosNet._IP2P.RemoveNotifyPeerConnectionClosed(_onClosedHandle);

        _onRequest = null;
        _onClosed = null;
        _onEstablished = null;
        _onInterrupted = null;
    }
    public void AddCB()
    {
        AddRequestCB();
        AddClosedCB();
        AddEstablishedCB();
        AddInterruptedCB();
    }


    public void OnRequest(OnIncomingConnectionRequestInfo info)
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        Role remoterole = (Role)info.ClientData;
        if (remoterole == Role.RemotePeer)
        {
            if (!EOSWrapper.P2PControl.AcceptConnection(eosNet._IP2P, _localPUID._PUID, info.RemoteUserId, _socketID))
            {
                Debug.LogError($"연결 실패");
            }
        }
        else
        {
            _onEstablished?.Invoke(new OnPeerConnectionEstablishedInfo()
            {
                ClientData = remoterole,
                LocalUserId = _localPUID._PUID,
                RemoteUserId = info.RemoteUserId,
                SocketId = _socketID,
            });

            if (remoterole == Role.localClient)
            {
                _onRequest?.Invoke(new OnIncomingConnectionRequestInfo()
                {
                    ClientData = Role.localHost,
                    LocalUserId = _localPUID._PUID,
                    RemoteUserId = info.RemoteUserId,
                    SocketId = _socketID,
                });
            }
        }
    }
    public void OnClosed(OnRemoteConnectionClosedInfo info)
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        EOS_Core.Role remoterole = (EOS_Core.Role)info.ClientData;

        var remotePUID = new EOSWrapper.ETC.PUID(info.RemoteUserId);
        if (_Connections.TryGetValue(remoterole, remotePUID._puid, out var connection))
        {
            connection.Release();
            _Connections.Remove(remoterole, remotePUID._puid);

            if (remoterole == EOS_Core.Role.localClient)
            {
                StopConnect(EOS_Core.Role.localHost, remotePUID._puid);
            }
            else if (remoterole == EOS_Core.Role.localHost)
            {
                StopConnect(EOS_Core.Role.localClient, remotePUID._puid);
            }
        }
    }
    public void OnEstablished(OnPeerConnectionEstablishedInfo info)
    {
        EOS_Core.Role remoterole = (EOS_Core.Role)info.ClientData;
        var connection = new Connection(remoterole, info.RemoteUserId.ToString());
        _Connections.TryAdd(remoterole, connection._remotePUID._puid, connection);
        _onMakeConnection?.Invoke(connection);
        connection.SetState(Connection.State.Connected);
    }
    public void OnInterrupted(OnPeerConnectionInterruptedInfo info)
    {

    }
    public bool GetConnection(EOS_Core.Role role, EOSWrapper.ETC.PUID remotePUID,out Connection Outconnection)
    {
       return _Connections.TryGetValue(role, remotePUID._puid, out Outconnection);
    }
    public void StartConnect(EOSWrapper.ETC.PUID remotePUID)
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        if( _localPUID._puid == remotePUID._puid)
        {
            _onRequest?.Invoke(new OnIncomingConnectionRequestInfo()
            {
                ClientData = Role.localClient,
                LocalUserId = _localPUID._PUID,
                RemoteUserId = remotePUID._PUID,
                SocketId = _socketID,
            });
        }
        else
        {
            if (!EOSWrapper.P2PControl.AcceptConnection(eosNet._IP2P, _localPUID._PUID, remotePUID._PUID, _socketID))
            {
                Debug.LogError($"연결 실패");
            }
        }
    }
    public void StopConnect(EOS_Core.Role role, string remotePuid)
    {

        var  remotePUID = new EOSWrapper.ETC.PUID(remotePuid);
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        if(GetConnection(role,remotePUID,out var connection))
        {
            if (connection._remoteRole == Role.RemotePeer)
            {
                if (!EOSWrapper.P2PControl.CloseConnection(eosNet._IP2P, _localPUID._PUID, connection._remotePUID._PUID, _socketID))
                {
                    Debug.LogError($"연결 끊기 실패");
                }
            }
            else
            {
                _onClosed?.Invoke(new OnRemoteConnectionClosedInfo()
                {
                    ClientData = connection._remoteRole,
                    LocalUserId = _localPUID._PUID,
                    RemoteUserId = connection._remotePUID._PUID,
                    SocketId = _socketID,
                });
            }
        }
    }
    public void StopAllConnect()
    {
        var eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        foreach(var kvp in _Connections.GetEnumerator())
        {
            StopConnect(kvp.Key1, kvp.Key2);
        }
    }

}