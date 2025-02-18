public class EOS_Factory
{
    public static IEOS_PlatformFactory GetFactory()
    {
        IEOS_PlatformFactory factory = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        factory = new WindowsFactory();
#endif
        return factory;
    }
}