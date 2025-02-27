#define CUSTUMNETCODEFIX
using Epic.OnlineServices.P2P;
using Unity.Netcode;
using UnityEngine;

public class NgoManager : NetworkManager
{
    /* 
     * �Ʒ��� NGO�� �� �� ������ ������
     * �̿� ��� �ڵ� ������ ������ ����. �ڼ��Ѱ� cirl + q �� CUSTUMNETCODEFIX Ž���� �ּ� Ȯ��
     * 
     * -NGO Time System Issue
     * NGO �� �ǵ����� Ŭ���̾�Ʈ ������ �����Ͽ� ���������� Ŭ���̾�Ʈ�� ���� ���¸� ���󰡵��� �Ѵ�.
     * �̸� ���� Ŭ���̾�Ʈ�� ���� �����ð� ���� ���� LocalTime, ���� ServerTime�� ���� 
     * Ŭ���̾�Ʈ�� ����ȭ ��Ŷ���� ������ Tick�� �͸������� �޾Ƶ��̵��� ����Ǿ�����.
     * �� �������� Ŭ���̾�Ʈ�� Tick�� �ѹ�� �� ������, Network Tick Loop �� ��ŵ�ǰų� �ߺ� ����ǰų� ������ ���� �ݺ��ϴ� ��Ȳ�� �߻��� �� ����.
     * 
     * - NGO Transform System Issue
     * ���� NGO�� ���� Tick�� ĳ���Ͽ� �׺��� �ռ� Tick�� ��쿡�� Transform�� Update �ϴ� ����� ���ϰ� ����
     * 
     * �������� �ʴ´ٸ� Transform�� �޴� ��� ������Ʈ �Ѵ�.
     * Transform ��Ŷ�� �ս� ���ɼ��� ����     
     * 
     * - Interporation
     * Last Sync Tranfrom �׸��� ������ ���� ���� Last Transform ���̿��� ������ ����.
     * ���� ��Ʈ��ũ�� Burst(������ ������ �Ѳ����� ���� ���) �ɶ� �ùٸ� �߰� ��θ� �ùķ��̼� ���� �ʴ´�.
     * Burst�� �����ϰ��� ServerTime ���� 2ƽ �� ������ �ð����� ���� ���� ����ϴµ�
     * �̶� �ε巯���� �����ϰ��� �ѹ� �� 2�� �����Ѵ�. (default 0.1f) 
     * Trnasform ������ ������� ������ ��ȭ�� ������ �� �������� �ܻ���� �� �� ���� 
     * -> Ŭ���̾�Ʈ ������ ���ؼ��� RPC�� ����� ������ Move ������ �ʿ�
     *
     * - optimization
     * ���������� �˾Ƽ� �� ��Ʈ ��ŷ�ϰ� ����.
     * �� ������ ���� ��� ��Ŷ�� Send, Receive�� �ϰ� ���� 
     * TODO : Transport Layer���� ť���Ͽ� ��Ʈ��ũ ���� ������ �ؾ� �� ��
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