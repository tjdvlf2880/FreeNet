using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
public interface IEOS_PlatformFactory
{
    bool LoadDLL();
    bool UnLoadDLL();
    bool MakePlatform(EOS_Credential credential, LogCategory category, LogLevel level, out PlatformInterface OutIPlatform);
}