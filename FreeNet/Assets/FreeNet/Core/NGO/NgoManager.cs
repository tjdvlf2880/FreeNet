using Epic.OnlineServices.P2P;
using Unity.Netcode;

public class NgoManager : NetworkManager
{
    /* - NGO Time System Issue
     * 
     * NGO �� �ǵ����� Ŭ���̾�Ʈ ������ �����Ͽ� ���������� ���� ���¸� ���󰡷��� ������ ����.
     * Ŭ���̾�Ʈ�� ���� �����ð� ���� ���� LocalTime, ���� ServerTime�� ���� 
     * Ŭ���̾�Ʈ�� ����ȭ ��Ŷ���� ������ Tick�� �͸������� �޾Ƶ��̵��� ����Ǿ�����.
     * �� �������� Ŭ���̾�Ʈ�� Tick�� �ѹ�� �� ������, Network Loop ���� ��ŵ�ǰų� �ߺ� ����ǰų� ������ Tick�� �ݺ��ϴ� ��Ȳ�� �߻��� �� ����.
     * 
     * - NGO Transform System Issue
     * �� �̽��� Tick�� �������� �ʱ� ������ �������� ����(teleport, jitter)�� ����.
     * ����� ���� Tick�� ĳ���Ͽ� �׺��� �ռ� Tick�� ��쿡�� Transform�� Update �ϴ� ����� ���ϰ� ����
     * 
     * - Interporation
     * teleport �� �����ϰ��� Extrapolation�� �������� �ʰ� ������  
     * �ѹ� �ùķ��̼� ���� �ʰ� ������ ���� Transform ������ ������ �����Ѵ�. 
     * �׷��� Burst(������ ������ �Ѳ����� ����) ��Ȳ������ �ùٸ� ��θ� �ùķ��̼� ���� �ʴ´�.
     * Burst�� �����ϰ��� ServerTime ���� 2ƽ �� ������ �ð����� ���� ���� ����ϸ� 
     * �̶� �ε巯���� �����ϰ��� �ѹ� �� 2�� �����Ͽ� (�⺻ 0.1��) ��Ÿ����. 
     * ��������� ������ ���غ���
     * 
     * - optimization
     * ��Ʈ ��ŷ�� ���ִ� ������ FastBufferWriter �� ����ϰ� �ִ�.
     * �� ������ ���� ��� ��Ŷ�� Send, Receive�� �ϰ� �ִ�.
     * ��Ŷ �߿䵵�� ���� ť�� �� ��Ʈ��ũ ���� ������ Transport Layer ���� �ؾ��� �� ����.
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