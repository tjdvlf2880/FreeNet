#if UNITY_EDITOR
#define EOS_DYNAMIC_BINDINGS
using UnityEditor;
#endif
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.UserInfo;
using UnityEngine;
public partial class EOS_Core : SingletonMonoBehaviour<EOS_Core>
{
    public enum InitState
    {
        Fail,
        Suceess,
    }
    public InitState _InitState { get; private set; }
    #region EOS Interface
    public PlatformInterface _IPlatform { get; private set; }
    public AuthInterface _IAuth { get; private set; }
    public ConnectInterface _IConnect { get; private set; }
    public LobbyInterface _ILobby { get; private set; }
    public SessionsInterface _ISession { get; private set; }
    public RTCInterface _IRTC { get; private set; }
    public RTCAudioInterface _IRTCAUDIO { get; private set; }
    public P2PInterface _IP2P { get; private set; }
    public UIInterface _IUI { get; private set; }
    public UserInfoInterface _IUSER { get; private set; }
    #endregion
    #region Credentials
    [SerializeField]
    private EOS_Credential _dev;
    [SerializeField]
    private EOS_Credential _stage;
    [SerializeField]
    private EOS_Credential _live;
    [SerializeField]
    public EOS_Credential.CredentialType type;
    #endregion

    [SerializeField]
    LogCategory category;
    [SerializeField]
    LogLevel level;


    private IEOS_PlatformFactory _factory;
    private void Awake()
    {
        if (SingletonSpawn(this))
        {
           _InitState = Init();
            SingletonInitialize();
        }
    }
    InitState Init()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        _factory = EOS_Factory.GetFactory();
        if(!_factory.LoadDLL()) return InitState.Fail;
        EOS_Credential credential = null;
        if (type == EOS_Credential.CredentialType.Dev)
        {
            credential = _dev;
        }
        else if (type == EOS_Credential.CredentialType.Stage)
        {
            credential = _stage;
        }
        else if (type == EOS_Credential.CredentialType.Live)
        {
            credential = _live;
        }
        if(!_factory.MakePlatform(_dev,  category,level, out var IPlatform))
        {
            _factory.UnLoadDLL();
            return InitState.Fail;
        }
        _IPlatform = IPlatform;
        _IAuth = _IPlatform.GetAuthInterface();
        _IConnect = _IPlatform.GetConnectInterface();
        _IP2P = _IPlatform.GetP2PInterface();
        _ILobby = _IPlatform.GetLobbyInterface();
        _ISession = _IPlatform.GetSessionsInterface();
        _IRTC = _IPlatform.GetRTCInterface();
        _IRTCAUDIO = _IRTC.GetAudioInterface();
        _IUI = _IPlatform.GetUIInterface();
        _IUSER = _IPlatform.GetUserInfoInterface();
        InitP2P();
        EOSWrapper.ETC.SetApplicationStatus(_IPlatform, ApplicationStatus.Foreground);
        return InitState.Suceess;
    }
    void Update()
    {
        _IPlatform?.Tick();
        ReceivePacket();
    }
    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EOS_Core._instance.OnRelease();
        }
    }

    public override void OnRelease()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
        if (_InitState == InitState.Suceess)
        {
            ReleaseP2P();
            _IPlatform?.Release();
            PlatformInterface.Shutdown();
            _factory.UnLoadDLL();
            _InitState = InitState.Fail;
        }
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            EOSWrapper.ETC.SetApplicationStatus(_IPlatform, ApplicationStatus.Foreground);
        }
    }
    void OnApplicationPause(bool pauseStatus)
    {
        EOSWrapper.ETC.SetApplicationStatus(_IPlatform, ApplicationStatus.BackgroundSuspended);
    }
}
