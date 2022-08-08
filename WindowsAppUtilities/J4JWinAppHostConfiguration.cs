using J4JSoftware.DependencyInjection;

namespace J4JSoftware.WindowsAppUtilities;

public class J4JWinAppHostConfiguration : J4JHostConfiguration
{
    public J4JWinAppHostConfiguration(
        bool registerHost = true
    )
        : base( AppEnvironment.PackagedWinApp, registerHost )
    {
        ApplicationConfigurationFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    }

    public override string UserConfigurationFolder => ApplicationConfigurationFolder;
}