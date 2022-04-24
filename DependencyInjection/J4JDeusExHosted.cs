using System;
using System.IO;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.DependencyInjection;

public abstract class J4JDeusExHosted : J4JDeusEx
{
    protected virtual J4JHostConfiguration? GetHostConfiguration() => null;

    protected virtual string GetCrashFilePath( J4JHostConfiguration hostConfig, string crashFileName = "crashFile.txt" )
    {
        var fileName = Path.GetFileName( crashFileName );

        if( string.IsNullOrEmpty( fileName ) )
            fileName = "crashFile.txt";

        return Path.Combine( hostConfig.ApplicationConfigurationFolder, fileName );
    }

    public bool Initialize( string crashFileName = "crashFile.txt" )
    {
        var hostConfig = GetHostConfiguration();

        if( hostConfig == null )
            throw new J4JDeusExException( $"Undefined {typeof( J4JHostConfiguration )}" );

        CrashFilePath = GetCrashFilePath( hostConfig, crashFileName );

        if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
        {
            OutputFatalMessage(
                $"Missing {typeof( J4JHostConfiguration )} items: {hostConfig.MissingRequirements}",
                hostConfig.Logger );

            return false;
        }

        Logger = hostConfig.Logger;

        var host = hostConfig.Build();

        if( host != null )
        {
            ServiceProvider = host.Services;
            Logger = host.Services.GetService<IJ4JLogger>();
            IsInitialized = true;

            return true;
        }

        Logger = null;

        OutputFatalMessage( $"Could not create {typeof( IJ4JHost )}",
                            hostConfig.Logger );

        return false;
    }
}
