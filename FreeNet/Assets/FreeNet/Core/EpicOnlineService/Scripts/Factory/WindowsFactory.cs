using Epic.OnlineServices;
using Epic.OnlineServices.IntegratedPlatform;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsFactory : IEOS_PlatformFactory
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private IntPtr handle;
    [DllImport("Kernel32")]
    private static extern IntPtr LoadLibrary(string lpLibFileName);
    [DllImport("kernel32")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("Kernel32")]
    private static extern int FreeLibrary(IntPtr hLibModule);
    [DllImport("Kernel32")]
    protected static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    public bool LoadDLL()
    {
#if UNITY_EDITOR
        string path = EOS_SDK_PluginPath(Config.LibraryName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[Load EOS] 찾을 수 없습니다 : {path}");
            return false;
        }
        handle = GetModuleHandle(path);
        if (handle != IntPtr.Zero)
        {
            Debug.Log($"[Load EOS] Reloading..");
            UnLoadDLL();
        }
        handle = LoadLibrary(path);
        if (handle == IntPtr.Zero)
        {
            Debug.LogError($"[Load EOS] Failed : {path}");
            return false;
        }
        Debug.Log($"[Load EOS] : {path}");
        Bindings.Hook(handle, GetProcAddress);
        WindowsBindings.Hook(handle, GetProcAddress);
#endif
        return true;
    }
    public bool MakePlatform(EOS_Credential credential, out PlatformInterface OutIPlatform)
    {
        OutIPlatform = null;
        if (!InitPlatform(credential)) return false;
        if(!SetLogDetail()) return false;
        if (!CreatePlatform(credential, out OutIPlatform)) return false;
        return true;
    }
    public bool UnLoadDLL()
    {
#if UNITY_EDITOR
        if (handle != IntPtr.Zero)
        {
            int tryCount = 0;
            Bindings.Unhook();
            WindowsBindings.Unhook();
            while (FreeLibrary(handle) != 0)
            {
                tryCount++;
                if (tryCount > 10)
                {
                    Debug.LogError($"[UnLoad EOS] trycount : {tryCount}");
                    return false;
                }
            }
            handle = IntPtr.Zero;
            Debug.Log($"[UnLoad EOS]");
        }
#endif
        return true;
    }
    private bool InitPlatform(EOS_Credential credential)
    {
        var initializeOptions = new InitializeOptions()
        {
            ProductName = credential.GameName,
            ProductVersion = "1.0"
        };
        var result = PlatformInterface.Initialize(ref initializeOptions);
        if (result != Result.Success)
        {
            Debug.LogError($"[InitPlatform] : {result}");
        }
        return result == Result.Success;
    }
    private bool SetLogDetail()
    {
        Result result = LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
        if (result != Result.Success) return false;
        result = LoggingInterface.SetCallback((ref LogMessage logMessage) =>
        {
            if (logMessage.Level == LogLevel.Fatal || logMessage.Level == LogLevel.Error)
            {
                Debug.LogError(logMessage.Message);
            }
            else if (logMessage.Level == LogLevel.Warning)
            {
                Debug.LogWarning(logMessage.Message);
            }
            else if (logMessage.Level == LogLevel.Info ||
            logMessage.Level == LogLevel.Verbose || logMessage.Level == LogLevel.VeryVerbose)
            {
                Debug.Log(logMessage.Message);
            }
        });
        return result == Result.Success;
    }
    private bool CreatePlatform(EOS_Credential credential, out PlatformInterface OutIPlatform)
    {
        OutIPlatform = null;
        string XAudio29DllPath = EOS_SDK_PluginPath("xaudio2_9redist");
        if (!File.Exists(XAudio29DllPath))
        {
            Debug.LogError($"[CreatePlatform] 파일을 찾을 수 없음: {XAudio29DllPath}");
            return false;
        }
        var options = new WindowsOptions()
        {
            ProductId = credential.ProductId,
            SandboxId = credential.SandboxId,
            DeploymentId = credential.DeploymentId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = credential.ClientCredentialsId,
                ClientSecret = credential.ClientCredentialsSecret
            },
            IsServer = false,
            TickBudgetInMilliseconds = 10,
            TaskNetworkTimeoutSeconds = 30,
            RTCOptions = new WindowsRTCOptions()
            {
                PlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions()
                {
                    XAudio29DllPath = XAudio29DllPath,
                }
            },
        };
        var OptionsContainer = new IntegratedPlatformOptionsContainer();
        var IntegratedPlatformOptions = new CreateIntegratedPlatformOptionsContainerOptions();
        var result = IntegratedPlatformInterface.CreateIntegratedPlatformOptionsContainer(ref IntegratedPlatformOptions, out OptionsContainer);
        if (result != Result.Success)
        {
            Debug.LogError($"CreateIntegratedPlatformOptionsContainer fail: {result}");
            OptionsContainer.Release();
            return false;
        }
        options.IntegratedPlatformOptionsContainerHandle = OptionsContainer;
#if UNITY_EDITOR
        options.Flags = PlatformFlags.None;
#else
        options.Flags = PlatformFlags.WindowsEnableOverlayD3D10;
#endif
        OutIPlatform = PlatformInterface.Create(ref options);
        OptionsContainer.Release();
        return OutIPlatform != null;
    }
    private string EOS_SDK_PluginPath(string dllName)
    {
        string path = null;
        string immediatePath =  "";

#if UNITY_EDITOR
        immediatePath = "EOS-SDK";
#endif
#if UNITY_64 || UNITY_EDITOR_64
        string System = "x86_64";
#else
        string System = "x86";
#endif
        path = Path.Combine(Application.dataPath, immediatePath, "Plugins", System, dllName + ".dll");
        return path;
    }
#endif
    }