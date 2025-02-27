#define CUSTUMNETCODEFIX
using Epic.OnlineServices.P2P;
using Unity.Netcode;
using UnityEngine;

public class NgoManager : NetworkManager
{
    /* 
     * 아래는 NGO를 쓸 때 유의할 점들임
     * 이에 몇몇 코드 로직을 수정할 예정. 자세한건 cirl + q 로 CUSTUMNETCODEFIX 탐색후 주석 확인
     * 
     * -NGO Time System Issue
     * NGO 는 의도적인 클라이언트 지연을 유발하여 안정적으로 클라이언트가 서버 상태를 따라가도록 한다.
     * 이를 위해 클라이언트는 실제 서버시간 보다 빠른 LocalTime, 느린 ServerTime을 가짐 
     * 클라이언트는 동기화 패킷에서 서버의 Tick을 맹목적으로 받아들이도록 설계되어있음.
     * 이 과정에서 클라이언트의 Tick이 롤백될 수 있으며, Network Tick Loop 는 스킵되거나 중복 실행되거나 과거의 값을 반복하는 상황이 발생할 수 있음.
     * 
     * - NGO Transform System Issue
     * 현재 NGO는 이전 Tick을 캐싱하여 그보다 앞선 Tick인 경우에만 Transform을 Update 하는 방식을 취하고 있음
     * 
     * 보간하지 않는다면 Transform을 받는 즉시 업데이트 한다.
     * Transform 패킷은 손실 가능성이 존재     
     * 
     * - Interporation
     * Last Sync Tranfrom 그리고 서버로 부터 받은 Last Transform 사이에서 보간을 진행.
     * 따라서 네트워크가 Burst(지연된 정보가 한꺼번에 오는 경우) 될때 올바른 중간 경로를 시뮬레이션 하지 않는다.
     * Burst를 방지하고자 ServerTime 에서 2틱 더 지연한 시간으로 보간 값을 계산하는데
     * 이때 부드러움을 강조하고자 한번 더 2차 보간한다. (default 0.1f) 
     * Trnasform 정보만 가지고는 이후의 변화를 예측할 수 없음으로 외삽법을 쓸 수 없음 
     * -> 클라이언트 예측을 위해서는 RPC를 사용한 별도의 Move 로직이 필요
     *
     * - optimization
     * 내부적으로 알아서 잘 비트 패킹하고 있음.
     * 매 프레임 마다 모든 패킷을 Send, Receive를 하고 있음 
     * TODO : Transport Layer에서 큐잉하여 네트워크 부하 관리를 해야 할 듯
     */
    FreeNet _freeNet;
    EOSNetcodeTransport _EOSNetcodeTransport;

    [SerializeField]
    private double _localBufferSec;

    [SerializeField]
    private double _serverBufferSec;


    [SerializeField]
    private bool _useEpicOnlineTransport;
    [SerializeField]
    private float _jitterRange;
    [SerializeField]
    private bool _virtualRtt;
    [SerializeField]
    private float _fixedRtt;

    public byte _channel => 0;
    public byte _UrgentPacketChannel => 1;

    public void Init(FreeNet freeNet)
    {
        _freeNet = freeNet;
        _EOSNetcodeTransport = GetComponent<EOSNetcodeTransport>();
    }
    public void SetNetworkValue()
    {
        if(NetworkTimeSystem != null)
        {
            NetworkTimeSystem.LocalBufferSec = _localBufferSec;
            NetworkTimeSystem.ServerBufferSec = _serverBufferSec;
        }

        if(MessageManager!= null)
        {
            MessageManager.NonFragmentedMessageMaxSize = P2PInterface.MaxPacketSize;
        }

        if(_EOSNetcodeTransport != null)
        {
            _EOSNetcodeTransport._pingpong.SetJitterRanage(_jitterRange);
            _EOSNetcodeTransport._pingpong.SetVirtualRrtt(_virtualRtt,_fixedRtt);
        }
    }
    public bool StartServer(EOSWrapper.ETC.PUID localPUID, string socketName)
    {
        var result = false; 
        if (_useEpicOnlineTransport)
        {
            result = _EOSNetcodeTransport.InitializeEOSServer(localPUID, socketName, _channel) && StartServer();
        }
        else
        {
            result = base.StartServer();
        }
        
        if (result)
        {
            SetNetworkValue();
            MessageManager.NonFragmentedMessageMaxSize = P2PInterface.MaxPacketSize;
        }
        return result;
    }
    public bool StartClient(EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketName)
    {
        var result = false;
        if (_useEpicOnlineTransport)
        {
            result = _EOSNetcodeTransport.InitializeEOSClient(localPUID, remotePUID, socketName, _channel) && StartClient();
        }
        else
        {
            result =  base.StartClient();
        }
        if (result)
        {
            SetNetworkValue();
        }
        return result;
    }
    public bool StartHost(EOSWrapper.ETC.PUID localPUID, string socketName)
    {
        var result = false;
        if (_useEpicOnlineTransport)
        {
            result = _EOSNetcodeTransport.InitializeEOSServer(localPUID, socketName, _channel) &&
            _EOSNetcodeTransport.InitializeEOSClient(localPUID, localPUID, socketName, _channel) &&
            StartHost() && _EOSNetcodeTransport.StartClient();
        }
        else
        {
            result = base.StartHost();
        }
        if(result)
        {
            SetNetworkValue();
        }
        return result;
    }
}