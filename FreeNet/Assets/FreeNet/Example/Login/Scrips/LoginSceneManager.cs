using Epic.OnlineServices;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour
{
    FreeNet _freeNet;

    [SerializeField]
    Button _guestLogin;

    [SerializeField]
    Button _epicPortalLogin;

    TransitionUI _transitionUI;
    BasicTransitionUI _basicTransitionUI;
    ProgressBarTransitionUI _progressBarTransitionUI;

    #region For Developer
    [SerializeField]
    Button _developerLogin;
    [SerializeField]
    string _host;
    [SerializeField]
    string _credential;
    #endregion
    private IEnumerator Start()
    {
        yield return SingletonMonoBehaviour<FreeNet>.WaitInitialize();
        _freeNet = FreeNet._instance;
        yield return SingletonMonoBehaviour<SingletonCanvas>.WaitInitialize();
        _transitionUI = SingletonCanvas._instance.GetComponentInChildren<TransitionUI>();
        var rootUI = _transitionUI.GetRootUI();
        _basicTransitionUI = rootUI.GetComponentInChildren<BasicTransitionUI>(true);
        _progressBarTransitionUI = rootUI.GetComponentInChildren<ProgressBarTransitionUI>(true);
        _guestLogin.onClick.AddListener(OnGuestLogin);
        _epicPortalLogin.onClick.AddListener(OnEpicPortalLogin);
        _developerLogin.onClick.AddListener(OnDeveloperLogin);
    }

    void OnLoginSuccess(Result result, ProductUserId localPUID)
    {
        var transition = new BasicTransition("LoginResult", _basicTransitionUI, result.ToString(), maxDuration: 0.5f);
        _transitionUI.AddTransition(transition);
        _transitionUI.MakeTransitionEnd("Login");
        if (result == Result.Success)
        {
            _freeNet._localUser._localPUID = new EOSWrapper.ETC.PUID(localPUID);
            _transitionUI.AddTransition(new ProgressBarSceneTranstion("LobbyScene", _progressBarTransitionUI));
        }
    }
    void OnGuestLogin()
    {
        var transition = new BasicTransition("Login", _basicTransitionUI, "Login...");
        _transitionUI.AddTransition(transition);
        string username = "I_AM_User";
        EOSWrapper.ConnectControl.DeviceIDConnect(_freeNet._eosCore._IConnect, username, (ref Epic.OnlineServices.Connect.LoginCallbackInfo info)=>
        {
            if(info.ResultCode == Epic.OnlineServices.Result.NotFound)
            {
                EOSWrapper.ConnectControl.CreateDeviceID(_freeNet._eosCore._IConnect,(ref Epic.OnlineServices.Connect.CreateDeviceIdCallbackInfo info) =>
                {
                    if (info.ResultCode == Epic.OnlineServices.Result.Success)
                    {
                        EOSWrapper.ConnectControl.DeviceIDConnect(_freeNet._eosCore._IConnect, username);
                    }
                });
            }
            else if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
            {
                OnLoginSuccess(Result.Success, info.LocalUserId);
            }
        });
    }
    void OnEpicPortalLogin()
    {
        var transition = new BasicTransition("Login", _basicTransitionUI, "Login...");
        _transitionUI.AddTransition(transition);
        EOSWrapper.LoginControl.EpicPortalLogin(_freeNet._eosCore._IAuth, (ref Epic.OnlineServices.Auth.LoginCallbackInfo info) =>
        {
            if(EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
            {
                EOSWrapper.ConnectControl.EpicIDConnect(_freeNet._eosCore._IAuth, _freeNet._eosCore._IConnect, info.LocalUserId, (ref Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                {
                    if (info.ResultCode == Epic.OnlineServices.Result.InvalidUser)
                    {
                        EOSWrapper.ConnectControl.CreateUser(_freeNet._eosCore._IConnect, info.ContinuanceToken, (ref Epic.OnlineServices.Connect.CreateUserCallbackInfo info) =>
                        {
                            if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
                            {
                                OnLoginSuccess(Result.Success, info.LocalUserId);
                            }
                        });
                    }
                    else if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
                    {
                        OnLoginSuccess(Result.Success, info.LocalUserId);
                    }
                });
            }
        });

    }
    void OnDeveloperLogin()
    {
        var transition = new BasicTransition("Login", _basicTransitionUI, "Login...");
        _transitionUI.AddTransition(transition);
        EOSWrapper.LoginControl.DeveloperToolLogin(_freeNet._eosCore._IAuth, _host, _credential, (ref Epic.OnlineServices.Auth.LoginCallbackInfo info) =>
        {
            if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
            {
                EOSWrapper.ConnectControl.EpicIDConnect(_freeNet._eosCore._IAuth, _freeNet._eosCore._IConnect, info.LocalUserId, (ref Epic.OnlineServices.Connect.LoginCallbackInfo info) =>
                {
                    if (info.ResultCode == Epic.OnlineServices.Result.InvalidUser)
                    {
                        EOSWrapper.ConnectControl.CreateUser(_freeNet._eosCore._IConnect, info.ContinuanceToken, (ref Epic.OnlineServices.Connect.CreateUserCallbackInfo info) =>
                        {
                            if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
                            {
                                OnLoginSuccess(Result.Success,info.LocalUserId);
                            }
                        });
                    }
                    else if (EOSWrapper.ETC.ErrControl<ProductUserId>(info.ResultCode, OnLoginSuccess))
                    {
                        OnLoginSuccess(Result.Success, info.LocalUserId);
                    }

                });
            }
        });
    }

    private void OnDestroy()
    {
        _guestLogin.onClick.RemoveAllListeners();
        _epicPortalLogin.onClick.RemoveAllListeners();
        _developerLogin.onClick.RemoveAllListeners();
    }
}
