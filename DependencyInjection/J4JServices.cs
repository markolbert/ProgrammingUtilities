using System;
using System.IO;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS8618

namespace J4JSoftware.DependencyInjection;

public static class J4JServices
{
    public static IServiceProvider Default { get; private set; }

    public static string CrashFile { get; private set; }
            //_crashFile = Path.Combine( HostConfiguration.ApplicationConfigurationFolder, "crashFile.txt" );

    public static IJ4JLogger? BuildLogger { get; private set; }
    public static bool IsValid { get; private set; }

    public static bool Initialize( J4JHostConfiguration? hostConfig, string crashFilePath )
    {
        CrashFile = crashFilePath;

        if( hostConfig == null )
        {
            OutputFatalMessage($"Undefined {typeof(J4JHostConfiguration)}");
            return false;
        }

        BuildLogger = hostConfig.Logger;

        if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
        {
            OutputFatalMessage(
                $"Missing {typeof( J4JHostConfiguration )} items: {hostConfig.MissingRequirements}" );
            
            return false;
        }

        var host = hostConfig.Build();

        if( host != null )
        {
            Default = host.Services;
            IsValid = true;
            BuildLogger = null;

            return true;
        }

        OutputFatalMessage( $"Could not create {typeof( IJ4JHost )}" );

        return false;
    }

    public static void OutputFatalMessage(string msg)
    {
        // how we log depends on whether we successfully created the service provider
        var logger = IsValid ? Default.GetRequiredService<IJ4JLogger>() : BuildLogger;
        logger?.Fatal(msg);

        try
        {
            File.AppendAllText( CrashFile, $"{msg}\n" );
        }
        catch
        {
        }
    }
}
