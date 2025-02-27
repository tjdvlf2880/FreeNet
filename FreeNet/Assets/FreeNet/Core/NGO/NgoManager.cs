using Epic.OnlineServices.P2P;
using Unity.Netcode;

public class NgoManager : NetworkManager
{
    /* - NGO Time System Issue
     * 
     * NGO 는 의도적인 클라이언트 지연을 유발하여 안정적으로 서버 상태를 따라가려는 경향이 있음.
     * 클라이언트는 실제 서버시간 보다 빠른 LocalTime, 느린 ServerTime을 가짐 
     * 클라이언트는 동기화 패킷에서 서버의 Tick을 맹목적으로 받아들이도록 설계되어있음.
     * 이 과정에서 클라이언트의 Tick이 롤백될 수 있으며, Network Loop 에서 스킵되거나 중복 실행되거나 과거의 Tick을 반복하는 상황이 발생할 수 있음.
     * 
     * - NGO Transform System Issue
     * 위 이슈로 Tick이 일정하지 않기 때문에 여러가지 문제(teleport, jitter)가 생김.
     * 현재는 이전 Tick을 캐싱하여 그보다 앞선 Tick인 경우에만 Transform을 Update 하는 방식을 취하고 있음
     * 
     * - Interporation
     * teleport 를 방지하고자 Extrapolation를 적용하지 않고 있으며  
     * 롤백 시뮬레이션 하지 않고 마지막 서버 Transform 정보를 가지고 보간한다. 
     * 그래서 Burst(지연된 정보가 한꺼번에 들어옴) 상황에서는 올바른 경로를 시뮬레이션 하지 않는다.
     * Burst를 방지하고자 ServerTime 에서 2틱 더 지연한 시간으로 보간 값을 계산하며 
     * 이때 부드러움을 강조하고자 한번 더 2차 보간하여 (기본 0.1초) 나타낸다. 
     * 결론적으로 지연이 심해보임
     * 
     * - optimization
     * 비트 패킹을 해주는 유용한 FastBufferWriter 를 사용하고 있다.
     * 매 프레임 마다 모든 패킷을 Send, Receive를 하고 있다.
     * 패킷 중요도에 따른 큐잉 및 네트워크 부하 관리는 Transport Layer 에서 해야할 것 같다.
     */
    FreeNet _freeNet;
    EOSNetcodeTransport _EOSNetcodeTransport;
    public byte _channel => 0;
    public byte _UrgentPacketChannel => 1;

    public void Init(FreeNet freeNet)
    {
        _freeNet = freeNet;
        _EOSNetcodeTransport = GetComponent<EOSNetcodeTransport>();
    }

    public bool StartServer(EOSWrapper.ETC.PUID localPUID, string socketName)
    {
        var result = _EOSNetcodeTransport.InitializeEOSServer(localPUID, socketName, _channel) && StartServer();
        if (result)
        {
            MessageManager.NonFragmentedMessageMaxSize = P2PInterface.MaxPacketSize;
        }
        return result;
    }
    public bool StartClient(EOSWrapper.ETC.PUID localPUID, EOSWrapper.ETC.PUID remotePUID, string socketName)
    {
        var result = _EOSNetcodeTransport.InitializeEOSClient(localPUID, remotePUID, socketName, _channel) && StartClient();
        if (result)
        {
            MessageManager.NonFragmentedMessageMaxSize = P2PInterface.MaxPacketSize;
        }
        return result;
    }
    public bool StartHost(EOSWrapper.ETC.PUID localPUID, string socketName)
    {

        bool result = _EOSNetcodeTransport.InitializeEOSServer(localPUID, socketName, _channel) &&
            _EOSNetcodeTransport.InitializeEOSClient(localPUID, localPUID, socketName, _channel) &&
            StartHost() && _EOSNetcodeTransport.StartClient();

        if(result)
        {
            MessageManager.NonFragmentedMessageMaxSize = P2PInterface.MaxPacketSize;
        }
        return result;
    }
}