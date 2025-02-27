using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PingPong : NetworkBehaviour
{
    public string MessageName = "PingPongMessage";
    float _jitterRange;
    bool _virtualRtt;
    float _fixedRtt;

    private Dictionary<ulong, double> _smoothedRTT;
    private const float Alpha = 0.125f;
    public override void OnNetworkSpawn()
    {
        _jitterRange = 0;
        _fixedRtt = 0;
        _virtualRtt = false;

        _smoothedRTT  = new Dictionary<ulong, double>();
        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, ReceiveMessage);
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisConnected;
    }
    public void SetVirtualRrtt(bool virtualRtt , float fixedRtt = 0)
    {
        _virtualRtt = virtualRtt;
        _fixedRtt = fixedRtt;
    }

    public void SetJitterRanage(float jitterRange)
    {
        _jitterRange = jitterRange;
    }

    private void OnClientConnected(ulong clientId)
    {
        _smoothedRTT.Add(clientId, 0);
    }
    private void OnClientDisConnected(ulong clientId)
    {
        _smoothedRTT.Remove(clientId);
    }
    private void FixedUpdate()
    {
        if (IsServer)
        {
            SendPing();
        }
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName);
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisConnected;
    }
    private void ReceiveMessage(ulong senderId, FastBufferReader messagePayload)
    {
        double sendTime;
        double rtt;
        messagePayload.ReadValueSafe(out sendTime);
        messagePayload.ReadValueSafe(out rtt);
        if (IsServer)
        {
            if(_smoothedRTT.TryGetValue(senderId, out var smoothedRTT))
            {
                rtt = (NetworkManager.RealTimeProvider.RealTimeSinceStartup * 1000.0f - sendTime);
                smoothedRTT = (1 - Alpha) * smoothedRTT + Alpha * rtt;
                _smoothedRTT[senderId] = smoothedRTT;
            }
        }
        else
        {
            _smoothedRTT[NetworkManager.ServerClientId] = rtt;
            SendPong(senderId, sendTime);
        }
    }
    private void SendPong(ulong clientId, double receivedTime)
    {
        var writer = new FastBufferWriter(sizeof(double), Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        using (writer)
        {
            writer.WriteValueSafe(receivedTime);
            customMessagingManager.SendNamedMessage(MessageName, clientId, writer);
        }
    }
    private void SendPing()
    {
        foreach (var clientID in _smoothedRTT.Keys)
        {
            if (_smoothedRTT.TryGetValue(clientID, out double rtt))
            {
                double sendTime = NetworkManager.RealTimeProvider.RealTimeSinceStartup*1000;
                var writer = new FastBufferWriter(sizeof(double) * 2, Allocator.Temp);
                using (writer)
                {
                    writer.WriteValueSafe(sendTime);
                    writer.WriteValueSafe(rtt);
                    NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName, clientID, writer);
                }
            }
        }
    }
    public bool GetRtt(ulong clientId, out double rtt)
    {
        rtt = 0;
        var jitter = Random.Range(-_jitterRange, _jitterRange);
        if (_virtualRtt)
        {
            rtt = _fixedRtt + jitter;
            return true;
        }
        else if(_smoothedRTT.TryGetValue(clientId, out rtt))
        {
            rtt = _fixedRtt + jitter;
            return true;
        }
        return false;
    }
}
