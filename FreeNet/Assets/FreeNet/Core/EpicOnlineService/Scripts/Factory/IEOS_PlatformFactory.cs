using Epic.OnlineServices.Platform;
public interface IEOS_PlatformFactory
{
    bool LoadDLL();
    bool UnLoadDLL();
    bool MakePlatform(EOS_Credential credential, out PlatformInterface OutIPlatform);


}