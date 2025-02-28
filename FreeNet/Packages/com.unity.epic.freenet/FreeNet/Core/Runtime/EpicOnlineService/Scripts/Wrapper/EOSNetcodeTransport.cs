using System;
using System.Collections.Generic;
using UnityEngine;
using Epic.OnlineServices.P2P;
using Unity.Netcode;
using System.Text;
public class EOSNetcodeTransport : NetworkTransport
{
    [SerializeField]
    public PingPong _pingpong;

    [SerializeField]
    EOS_Server _serverPrefab;
    [SerializeField]
    EOS_Client _clientPrefab;

    EOS_Server _server;
    EOS_Client _client;

    byte _channel;

    private NgoManager _ngoManager;
    struct ServerConnectionChangeInfo
    {
        public int connectID;
        public EOS_Socket.Connection connection;
    }

    Queue<ServerConnectionChangeInfo> _ServerConnectionChangeInfo;
    Queue<EOS_Socket.Connection> _ClientConnectionChangeInfo;

    public override ulong ServerClientId => 0;
    int m_NextTransportID = 0;
    #region Netcode Transport Override
    public override void Initialize(NetworkManager networkManager = null)
    {
        this._ngoManager = networkManager as NgoManager;
        this._ngoManager.NetworkConfig.ClientConnectionBufferTimeout = 30;
        this._ngoManager.OnClientConnectedCallback += SetMTU;
        _ServerConnectionChangeInfo = new Queue<ServerConnectionChangeInfo>();
        _ClientConnectionChangeInfo = new Queue<EOS_Socket.Connection>();
    }
    void SetMTU(ulong clientID)
    {
        this._ngoManager.SetPeerMTU(clientID,P2PInterface.MaxPacketSize);
    }
    public override NetworkEvent PollEvent(out ulong transportId, out ArraySegment<byte> payload, out float receiveTime)
    {
        transportId = default;
        payload = default;
        receiveTime = Time.realtimeSinceStartup;
        return NetworkEvent.Nothing;
    }
    void PollConnectEvent()
    {
        while (true)
        {
            var netEventType = NetworkEvent.Nothing;
            if (_ngoManager.IsServer)
            {
                if (_ServerConnectionChangeInfo.TryDequeue(out var serverInfo))
                {
                    var transportId = (ulong)serverInfo.connectID;
                    if (serverInfo.connection._State == EOS_Socket.Connection.State.Connected)
                    {
                        netEventType = NetworkEvent.Connect;
                    }
                    else
                    {
                        netEventType = NetworkEvent.Disconnect;
                    }
                    InvokeOnTransportEvent(netEventType, transportId, default, Time.realtimeSinceStartup);
                }
            }
            else
            {
                if (_ClientConnectionChangeInfo.TryDequeue(out var clientInfo))
                {
                    var transportId = ServerClientId;
                    if (clientInfo._State == EOS_Socket.Connection.State.Connected)
                    {
                        netEventType = NetworkEvent.Connect;
                    }
                    else
                    {
                        netEventType = NetworkEvent.Disconnect;
                    }
                    InvokeOnTransportEvent(netEventType, transportId, default, Time.realtimeSinceStartup);
                }
            }

            if (netEventType == NetworkEvent.Nothing)
            {
                break;
            }
        }
    }
    void PollClientDataEvent()
    {
        while (true)
        {
            var netEventType = NetworkEvent.Nothing;
            if (_ngoManager.IsClient)
            {
                if (_client != null && _client.DequeuePacket(_channel, out var packet))
                {
                    var transportId = ServerClientId;
                    var payload = packet._data;
                    netEventType = NetworkEvent.Data;
                    InvokeOnTransportEvent(netEventType, transportId, payload, Time.realtimeSinceStartup);
                }
            }

            if (netEventType == NetworkEvent.Nothing)
            {
                break;
            }
        }
    }
    void PollServerDataEvent(int? id = null)
    {
        while (true)
        {
            var netEventType = NetworkEvent.Nothing;
            if (_ngoManager.IsServer)
            {
                if (_server != null)
                {
                    bool dequed = false;
                    dequed = (id == null) ? _server.DequeuePacket(_channel, out var packetInfo) : _server.DequeuePacket(id.Value, _channel, out packetInfo);
                    if(dequed)
                    {
                        if (_server.GetConnectID(packetInfo._senderPUID, out var transportId))
                        {
                            var payload = packetInfo._data;
                            netEventType = NetworkEvent.Data;
                            InvokeOnTransportEvent(netEventType, (ulong)transportId, payload, Time.realtimeSinceStartup);
                        }
                    }
                }
            }

            if (netEventType == NetworkEvent.Nothing)
            {
                break;
            }
        }
    }
    protected override void OnEarlyUpdate()
    {
        PollConnectEvent();
        PollServerDataEvent();
        PollClientDataEvent();
        base.OnEarlyUpdate();
    }
    public override void DisconnectLocalClient()
    {
        _client.StopConnection();
    }
    public override void DisconnectRemoteClient(ulong transportId)
    {
        _server.StopConnection((int)transportId);
    }
    public override ulong GetCurrentRtt(ulong clientId)
    {
        double rtt = 0;
        _pingpong.GetRtt(clientId, out rtt);
        return (ulong)rtt;
    }
    public override void Send(ulong transportID, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
    {
        Epic.OnlineServices.P2P.PacketReliability reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
        if (networkDelivery == NetworkDelivery.Unreliable)
        {
            reliability = Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;
        }
        else if (networkDelivery == NetworkDelivery.Reliable)
        {
            reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered;
        }

        if (payload.Count > P2PInterface.MaxPacketSize)
        {
            if (reliability != Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered)
            {
                Debug.LogError($"NGO : send fail payload {payload.Count} exceeds {P2PInterface.MaxPacketSize}");
                return;
            }
        }
        if (transportID == ServerClientId)
        {
            _client.SendToServer(_channel, payload, reliability);
        }
        else
        {
            _server.SendToClient(_channel, payload, (int)transportID, reliability);
        }
    }
    public override void Shutdown()
    {
        if (_server != null)
        {
            _server.Shutdown();
            _server._onConnectionStateChanged -= OnServerConnectionStateChangedCB;
            _server._onStateChanged -= OnServerStateChangedCB;
            _server._onReceivedPacket -= OnServerReceivedPacketCB;
            _ServerConnectionChangeInfo.Clear();
            Destroy(_server.gameObject);
            _server = null;
        }
        if (_client != null)
        {
            _client.Shutdown();
            _client._onConnectionStateChanged -= OnClientConnectionStateChangedCB;
            _client._onStateChanged -= OnClientStateChangedCB;
            _client._onReceivedPacket -= OnClientReceivedPacketCB;
            _ClientConnectionChangeInfo.Clear();
            Destroy(_client.gameObject);
            _client = null;
        }
        this._ngoManager.OnClientConnectedCallback -= SetMTU;
        m_NextTransportID = 0;
    }
    public override bool StartClient()
    {
        _client.StartConnection();
        return true;
    }
    public override bool StartServer()
    {
        _server.StartConnection();
        return true;
    }
    public int GetNewConnectID(EOS_Core.Role role)
    {
        m_NextTransportID++;
        return m_NextTransportID;
    }
    public bool InitializeEOSServer(EOS_Core eosCore, EOSWrapper.ETC.PUID localPUID, string socketid, byte channel)
    {
        if (_server != null) return false;
        _server = Instantiate(_serverPrefab);
        _server.transform.parent = this.transform;
        _server.Init(eosCore,localPUID, socketid, GetNewConnectID);
        _channel = channel;
        _server._onConnectionStateChanged -= OnServerConnectionStateChangedCB;
        _server._onConnectionStateChanged += OnServerConnectionStateChangedCB;
        _server._onStateChanged -= OnServerStateChangedCB;
        _server._onStateChanged += OnServerStateChangedCB;
        _server._onReceivedPacket -= OnServerReceivedPacketCB;
        _server._onReceivedPacket += OnServerReceivedPacketCB;
        return true;
    }
    void OnServerConnectionStateChangedCB((int connectid, EOS_Socket.Connection connection) info)
    {
        if (_ngoManager.IsServer && info.connection._remoteRole != EOS_Core.Role.localClient)
        {
            _ServerConnectionChangeInfo.Enqueue(new ServerConnectionChangeInfo()
            {
                connectID = info.connectid,
                connection = info.connection
            });
        }
    }
    void OnServerReceivedPacketCB(int id,int channel)
    {
        if (channel == _ngoManager._UrgentPacketChannel)
        {
            _server.DequeuePacket(id,channel,out var _);
            PollConnectEvent();
            PollServerDataEvent(id);
        }
    }
    void OnServerStateChangedCB(EOS_Peer.state state)
    {

    }
    public bool InitializeEOSClient(EOS_Core eosCore,EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketid, byte channel)
    {
        if (_client != null) return false;
        _client = Instantiate(_clientPrefab);
        _client.transform.parent = this.transform;
        _client.Init(eosCore,localPUID, remotePUID, socketid, channel);
        _client._onConnectionStateChanged -= OnClientConnectionStateChangedCB;
        _client._onConnectionStateChanged += OnClientConnectionStateChangedCB;
        _client._onStateChanged -= OnClientStateChangedCB;
        _client._onStateChanged += OnClientStateChangedCB;
        _client._onReceivedPacket -= OnClientReceivedPacketCB;
        _client._onReceivedPacket += OnClientReceivedPacketCB;
        return true;
    }
    void OnClientConnectionStateChangedCB(EOS_Socket.Connection connection)
    {
        if (_ngoManager.IsClient && connection._remoteRole != EOS_Core.Role.localHost)
        {
            _ClientConnectionChangeInfo.Enqueue(connection);
        }
    }
    void OnClientReceivedPacketCB(int channel)
    {        
        if(channel == _ngoManager._UrgentPacketChannel)
        {
            _client.DequeuePacket(channel, out var _);
            PollConnectEvent();
            PollClientDataEvent();
        }
    }
    void OnClientStateChangedCB(EOS_Peer.state state)
    {

    }
    #endregion
}