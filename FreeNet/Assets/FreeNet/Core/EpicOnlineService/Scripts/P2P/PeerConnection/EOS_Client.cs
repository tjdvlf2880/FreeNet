using Epic.OnlineServices.P2P;
using System.Collections.Generic;
using System;
using static EOS_Socket;
public class EOS_Client : EOS_Peer
{
    public EOSWrapper.ETC.PUID _localPUID { get; private set; }
    EOSWrapper.ETC.PUID _remotePUID;
    public Queue<ArraySegment<byte>> _incomingPacket;

    public event Action<EOS_Core.EOS_Packet> _onReceivedPacket;
    public event Action<EOS_Socket.Connection> _onConnectionStateChanged;
    public void Init(EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketid, byte channel)
    {
        _eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _state = state.stop;
        _localPUID = localPUID;
        _remotePUID = remotePUID;
        _socket = _eosNet.CreateSocket(localPUID, socketid);
        _socket._onMakeConnection -= OnMakeConnectionCB;
        _socket._onClosed -= OnClosedCB;
        _socket._onMakeConnection += OnMakeConnectionCB;
        _socket._onClosed += OnClosedCB;
        _channel = channel;
        _incomingPacket = new Queue<ArraySegment<byte>>();
    }
    public bool GetConnection(out EOS_Socket.Connection connection)
    {
        if (_localPUID == _remotePUID)
        {
            return _socket.GetConnection(EOS_Core.Role.localHost, _remotePUID, out connection);
        }
        else
        {
            return _socket.GetConnection(EOS_Core.Role.RemotePeer, _remotePUID, out connection);
        }
    }
    public override void OnClientEnqueuePacket(EOS_Socket.Connection connection)
    {
        if (connection.DeqeuePacket(out var packet))
        {
            _incomingPacket.Enqueue((packet._data));
            _onReceivedPacket?.Invoke(packet);
        }
    }
    public override void OnMakeConnectionCB(Connection info)
    {
        if (_remotePUID._puid == info._remotePUID._puid)
        {
            var remoterole = info._remoteRole;
            if (remoterole != EOS_Core.Role.localClient)
            {
                if (_socket.GetConnection(remoterole, info._remotePUID, out var connection))
                {
                    connection._onEnqueuePacket += OnClientEnqueuePacket;
                    connection._onConnectionStateChanged += OnConnectionStateChangedCB;
                }
                ChangeState(state.start);
            }
        }
    }
    public void SendToServer(byte channelId, ArraySegment<byte> segment, PacketReliability reliability)
    {
        if (_localPUID == _remotePUID)
        {
            _eosNet.SendLocal(_socket, EOS_Core.Role.localClient, _channel, segment);
        }
        else
        {
            _eosNet.SendPeer(_socket, _remotePUID._PUID, _channel, segment, reliability);
        }
    }
    public override void OnClosedCB(OnRemoteConnectionClosedInfo info)
    {
        if (_remotePUID._puid == info.RemoteUserId.ToString())
        {
            var remoterole = (EOS_Core.Role)info.ClientData;
            if (remoterole != EOS_Core.Role.localClient)
            {
                if (_socket.GetConnection(remoterole, new EOSWrapper.ETC.PUID(info.RemoteUserId), out var connection))
                {
                    connection._onEnqueuePacket -= OnClientEnqueuePacket;
                    connection._onConnectionStateChanged -= OnConnectionStateChangedCB;
                }
                ChangeState(state.stop);
            }
        }
    }
     void OnConnectionStateChangedCB(Connection connection)
    {
        _onConnectionStateChanged?.Invoke(connection);
    }

    public override bool StartConnection()
    {
        ChangeState(state.starting);
        _socket.StartConnect(_remotePUID);
        return true;
    }
    public override bool StopConnection()
    {
        ChangeState(state.stopping);
        if (_localPUID == _remotePUID)
        {
            _socket.StopConnect(EOS_Core.Role.localHost, _remotePUID._puid);
        }
        else
        {
            _socket.StopConnect(EOS_Core.Role.RemotePeer, _remotePUID._puid);
        }
        return true;
    }
    public override void Shutdown()
    {
        StopConnection();
        if (_socket != null)
        {
            _socket._onMakeConnection -= OnMakeConnectionCB;
            _socket._onClosed -= OnClosedCB;
        }
    }
}
