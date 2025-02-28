using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static EOS_Socket;

public class EOS_Server : EOS_Peer
{
    public EOSWrapper.ETC.PUID _localPUID { get; private set; }
    private Dictionary<int, EOSWrapper.ETC.PUID> _ConnectIDmapping;
    private Dictionary<EOSWrapper.ETC.PUID, int> _PUIDmapping;
    public event Action<int,int> _onReceivedPacket;
    Dictionary<EOSWrapper.ETC.PUID, Dictionary<int, Queue<EOS_Core.EOS_Packet>>> _incomingPackets;
    public event Action<(int, EOS_Socket.Connection)> _onConnectionStateChanged;

    public delegate int GetNewConnectID(EOS_Core.Role role);
    GetNewConnectID _GetNewConnectIDDelegate;
    public void Init(EOS_Core eosCore, EOSWrapper.ETC.PUID localPUID, string socketid, GetNewConnectID getNewConnectIDDelegate)
    {
        _eosCore = eosCore;
        _state = state.stop;
        _localPUID = localPUID;
        _GetNewConnectIDDelegate = getNewConnectIDDelegate;
        _socket = _eosCore.CreateSocket(localPUID, socketid);
        _socket._onMakeConnection -= OnMakeConnectionCB;
        _socket._onClosed -= OnClosedCB;
        _socket._onMakeConnection += OnMakeConnectionCB;
        _socket._onClosed += OnClosedCB;
        _ConnectIDmapping = new Dictionary<int, EOSWrapper.ETC.PUID>();
        _PUIDmapping = new Dictionary<EOSWrapper.ETC.PUID, int>();
        _incomingPackets = new Dictionary<EOSWrapper.ETC.PUID, Dictionary<int, Queue<EOS_Core.EOS_Packet>>>();  
    }
    public void RemoveMapping(EOSWrapper.ETC.PUID puid)
    {
        if (GetConnectID(puid, out var id))
        {
            _ConnectIDmapping.Remove(id);
        }
        _PUIDmapping.Remove(puid);
    }
    public void RemoveMapping(int id)
    {
        if (GetPUID(id, out var puid))
        {
            _PUIDmapping.Remove(puid);
        }
        _ConnectIDmapping.Remove(id);
    }
    public void RemoveMapping()
    {
        _ConnectIDmapping.Clear();
        _PUIDmapping.Clear();
    }
    public bool GetConnection(int id, out EOS_Socket.Connection connection)
    {
        connection = null;
        if (GetPUID(id, out var puid))
        {
            if (_localPUID == puid)
            {
                return _socket.GetConnection(EOS_Core.Role.localClient, puid, out connection);
            }
            else
            {
                return _socket.GetConnection(EOS_Core.Role.RemotePeer, puid, out connection);
            }
        }
        return false;
    }
    public bool GetConnectID(EOSWrapper.ETC.PUID puid, out int id)
    {
        return _PUIDmapping.TryGetValue(puid, out id);
    }
    public bool GetPUID(int id, out EOSWrapper.ETC.PUID puid)
    {
        return _ConnectIDmapping.TryGetValue(id, out puid);
    }
    public IEnumerable<(int, EOSWrapper.ETC.PUID)> GetEnumerable()
    {
        foreach (var kp in _ConnectIDmapping.ToList())
        {
            yield return (kp.Key, kp.Value);
        }
    }
    public int AddMapping(int newID, EOSWrapper.ETC.PUID remotePUID)
    {
        if (_PUIDmapping.TryGetValue(remotePUID, out int id))
        {
            return id;
        }
        else
        {
            _ConnectIDmapping.Add(newID, remotePUID);
            _PUIDmapping.Add(remotePUID, newID);
            return newID;
        }
    }
    public void OnServerEnqueuePacket(EOS_Socket.Connection connection,int channel)
    {
        if (connection.DeqeuePacket(channel, out var packet))
        {
            if (!_incomingPackets.TryGetValue(connection._remotePUID, out var incomingPackets))
            {
                incomingPackets = new Dictionary<int, Queue<EOS_Core.EOS_Packet>>();
                _incomingPackets.Add(connection._remotePUID, incomingPackets);
            }

            if (!incomingPackets.TryGetValue(channel, out var queue))
            {
                queue = new Queue<EOS_Core.EOS_Packet>();
                incomingPackets.Add(channel, queue);
            }
            queue.Enqueue(packet);
            if (GetConnectID(connection._remotePUID, out int id))
            {
                _onReceivedPacket?.Invoke(id,channel);
            }
        }
    }

    public bool DequeuePacket(EOSWrapper.ETC.PUID puid, int channel, out EOS_Core.EOS_Packet packet)
    {
        packet = default;
        if(_incomingPackets.TryGetValue(puid, out var incomingpackets))
        {
            if (incomingpackets.TryGetValue(channel, out var queue))
            {
                return queue.TryDequeue(out packet);
            }
        }
        return false;
    }
    public bool DequeuePacket(int channel, out EOS_Core.EOS_Packet packet)
    {
        packet = default;
        foreach (var incomingpackets in _incomingPackets.Values)
        {
            foreach (var queue in incomingpackets)
            {
                if(channel == queue.Key)
                {
                    foreach (var item in queue.Value)
                    {
                        return queue.Value.TryDequeue(out packet);
                    }
                }
            }
        }
        return false;
    }
    public bool DequeuePacket(int id,int channel, out EOS_Core.EOS_Packet packet)
    {
        packet = default;
        if (GetPUID(id,out var puid))
        {
            if (_incomingPackets.TryGetValue(puid, out var incomingPackets))
            {
                if(incomingPackets.TryGetValue(channel,out var queue))
                { 
                    return queue.TryDequeue(out packet);
                }
            }
        }
        return false;
    }
    public void SendToClient(byte channel, ArraySegment<byte> segment, int connectionId, PacketReliability reliability)
    {
        if (GetPUID(connectionId, out var puid))
        {
            if(puid._puid == _localPUID._puid)
            {
                _eosCore.SendLocal(_socket, EOS_Core.Role.localClient, channel, segment);

            }
            else
            {
                _eosCore.SendPeer(_socket, ProductUserId.FromString(puid._puid), channel, segment, reliability);
            }
        }
    }
    public override void OnMakeConnectionCB(Connection info)
    {
        var remoterole = (EOS_Core.Role)info._remoteRole;
        if (remoterole != EOS_Core.Role.localHost)
        {
            AddMapping(_GetNewConnectIDDelegate(remoterole), info._remotePUID);
            if (_socket.GetConnection(remoterole, info._remotePUID, out var connection))
            {
                connection._onEnqueuePacket += OnServerEnqueuePacket;
                connection._onConnectionStateChanged += OnConnectionStateChangedCB;
            }
        }
    }
    void OnConnectionStateChangedCB(Connection connection)
    {
        if (GetConnectID(connection._remotePUID, out int id))
        {
            _onConnectionStateChanged?.Invoke((id, connection));
        }
    }
    public override void OnClosedCB(OnRemoteConnectionClosedInfo info)
    {
        var remoterole = (EOS_Core.Role)info.ClientData;
        if (remoterole != EOS_Core.Role.localHost)
        {
            var remotePUID = new EOSWrapper.ETC.PUID(info.RemoteUserId);
            if (GetConnectID(remotePUID, out var id))
            {
                RemoveMapping(remotePUID);
                if (_socket.GetConnection(remoterole, remotePUID, out var connection))
                {
                    connection._onEnqueuePacket -= OnServerEnqueuePacket;
                    connection._onConnectionStateChanged -= OnConnectionStateChangedCB;
                }
            }
        }
    }
    public bool StopConnection(EOSWrapper.ETC.PUID puid)
    {
        if (_localPUID == puid)
        {
            _socket.StopConnect(EOS_Core.Role.localClient, puid._puid);
        }
        else
        {
            _socket.StopConnect(EOS_Core.Role.RemotePeer, puid._puid);
        }
        RemoveMapping(puid);
        return true;
    }
    public bool StopConnection(int connectionId)
    {
        if (GetPUID(connectionId, out var puid))
        {
            if (_localPUID._puid == puid._puid)
            {
                _socket.StopConnect(EOS_Core.Role.localClient, puid._puid);
            }
            else
            {
                _socket.StopConnect(EOS_Core.Role.RemotePeer, puid._puid);
            }
            RemoveMapping(connectionId);
        }
        return true;
    }
    public override bool StopConnection()
    {
        foreach (var kvp in GetEnumerable())
        {
            StopConnection(kvp.Item2);
        }
        return true;
    }
    public override bool StartConnection()
    {
        ChangeState(state.start);
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
