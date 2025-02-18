using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Epic.OnlineServices.P2P;
public class EOSNetcodeTransport : NetworkTransport
{

    [SerializeField]
    EOS_Server _serverPrefab;
    [SerializeField]
    EOS_Client _clientPrefab;

    EOS_Server _server;
    EOS_Client _client;

    private NetworkManager networkManager;
    struct ServerConnectionChangeInfo
    {
       public int connectID;
       public EOS_Socket.Connection connection;
    }

    Queue<ServerConnectionChangeInfo> _ServerConnectionChangeInfo;
    Queue<EOS_Socket.Connection> _ClientConnectionChangeInfo;

    public override ulong ServerClientId => 0;
    int m_NextTransportID = 0;

    #region Use This Method instead of using NetworkManager directly
    public void StartServer(EOSWrapper.ETC.PUID localPUID,string socketName)
    {
        var _eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _eosNet.GetComponent<EOSNetcodeTransport>().InitializeEOSServer(localPUID, socketName, 0);
        NetworkManager.Singleton.StartServer();
    }
    public void StartClient(EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketName)
    {
        var _eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _eosNet.GetComponent<EOSNetcodeTransport>().InitializeEOSClient(localPUID, remotePUID, socketName, 0);
        NetworkManager.Singleton.StartClient();
    }
    public void StartHost(EOSWrapper.ETC.PUID localPUID, string socketName)
    {
        var _eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
        _eosNet.GetComponent<EOSNetcodeTransport>().InitializeEOSServer(localPUID, socketName, 0);
        _eosNet.GetComponent<EOSNetcodeTransport>().InitializeEOSClient(localPUID, localPUID, socketName, 0);
        NetworkManager.Singleton.StartHost();
        _eosNet.GetComponent<EOSNetcodeTransport>().StartClient();
    }
    #endregion
    #region Netcode Transport Override
    public override void Initialize(NetworkManager networkManager = null)
    {
        this.networkManager = networkManager;
        this.networkManager.NetworkConfig.ClientConnectionBufferTimeout = 30;
        _ServerConnectionChangeInfo = new Queue<ServerConnectionChangeInfo>();
        _ClientConnectionChangeInfo = new Queue<EOS_Socket.Connection>();
    }
    public override NetworkEvent PollEvent(out ulong transportId, out ArraySegment<byte> payload, out float receiveTime)
    {
        transportId = default;
        payload = default;
        receiveTime = Time.realtimeSinceStartup;
        if(_ServerConnectionChangeInfo.TryDequeue(out var serverInfo))
        {
            transportId =(ulong)serverInfo.connectID;
            if (serverInfo.connection._State == EOS_Socket.Connection.State.Connected)
            {
                return NetworkEvent.Connect;
            }
            else
            {
                return NetworkEvent.Disconnect;
            }
        }
        if (_ClientConnectionChangeInfo.TryDequeue(out var clientInfo))
        {
            transportId = ServerClientId;
            if (clientInfo._State == EOS_Socket.Connection.State.Connected)
            {
                return NetworkEvent.Connect;
            }
            else
            {
                return NetworkEvent.Disconnect;
            }
        }
        if (_server != null && _server._incomingPacket.TryDequeue(out var packetInfo))
        {
            transportId =  (ulong)packetInfo.Item1;
            payload = packetInfo.Item2;
            return NetworkEvent.Data;
        }
        if (_client != null && _client._incomingPacket.TryDequeue(out var packet))
        {
            transportId = ServerClientId;
            payload = packet;
            return NetworkEvent.Data;
        }
        return NetworkEvent.Nothing;
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
        return 0;
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
        else if (networkDelivery == NetworkDelivery.ReliableFragmentedSequenced || networkDelivery == NetworkDelivery.ReliableSequenced)
        {
            reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
        }

        if (payload.Count > P2PInterface.MaxPacketSize)
        {
            if (reliability != Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered)
            {
                Debug.LogError($"NGO : send fail payload {payload.Count} exceeds {P2PInterface.MaxPacketSize}");
                return;
            }
        }
        if(transportID == ServerClientId)
        {
            _client.SendToServer(_client._channel, payload, reliability);
        }
        else
        {
            _server.SendToClient(_server._channel, payload,(int)transportID, reliability);
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
        if (_client!= null )
        {
            _client.Shutdown();
            _client._onConnectionStateChanged -= OnClientConnectionStateChangedCB;
            _client._onStateChanged -= OnClientStateChangedCB;
            _client._onReceivedPacket -= OnClientReceivedPacketCB;
            _ClientConnectionChangeInfo.Clear();
            Destroy(_client.gameObject);
            _client = null;
        }
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
    void InitializeEOSServer(EOSWrapper.ETC.PUID localPUID, string socketid, byte channel)
    {
        _server = Instantiate(_serverPrefab);
        _server.transform.parent = this.transform;
        _server.Init(localPUID, socketid, channel, GetNewConnectID);
        _server._onConnectionStateChanged -= OnServerConnectionStateChangedCB;
        _server._onConnectionStateChanged += OnServerConnectionStateChangedCB;
        _server._onStateChanged -= OnServerStateChangedCB;
        _server._onStateChanged += OnServerStateChangedCB;
        _server._onReceivedPacket -= OnServerReceivedPacketCB;
        _server._onReceivedPacket += OnServerReceivedPacketCB;
    }
    void OnServerConnectionStateChangedCB((int connectid, EOS_Socket.Connection connection) info)
    {
        if (networkManager.IsServer && info.connection._remoteRole != EOS_Core.Role.localClient)
        {
            _ServerConnectionChangeInfo.Enqueue(new ServerConnectionChangeInfo()
            {
                connectID = info.connectid,
                connection = info.connection
            });
        }
    }
    void OnServerReceivedPacketCB((int connectid, EOS_Core.EOS_Packet packet) info)
    {

    }
    void OnServerStateChangedCB(EOS_Peer.state state)
    {

    }
    void InitializeEOSClient(EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketid, byte channel)
    {
        _client = Instantiate(_clientPrefab);
        _client.transform.parent = this.transform;
        _client.Init(localPUID, remotePUID, socketid, channel);
        _client._onConnectionStateChanged -= OnClientConnectionStateChangedCB;
        _client._onConnectionStateChanged += OnClientConnectionStateChangedCB;
        _client._onStateChanged -= OnClientStateChangedCB;
        _client._onStateChanged += OnClientStateChangedCB;
        _client._onReceivedPacket -= OnClientReceivedPacketCB;
        _client._onReceivedPacket += OnClientReceivedPacketCB;
    }
    void OnClientConnectionStateChangedCB(EOS_Socket.Connection connection)
    {
        if (networkManager.IsClient && connection._remoteRole != EOS_Core.Role.localHost)
        {
            _ClientConnectionChangeInfo.Enqueue(connection);
        }
    }
    void OnClientReceivedPacketCB(EOS_Core.EOS_Packet packet)
    {

    }
    void OnClientStateChangedCB(EOS_Peer.state state)
    {

    }
    #endregion
}
