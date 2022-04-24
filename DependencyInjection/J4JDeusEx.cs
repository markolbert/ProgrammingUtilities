using System;
using System.Dynamic;
using System.IO;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable CS8618

namespace J4JSoftware.DependencyInjection;

public class J4JDeusEx
{
    public static bool IsInitialized { get; protected set; }
    public static string? CrashFilePath { get; protected set; }

    protected static IServiceProvider? ServiceProvider { get; set; }
    protected static IJ4JLogger? Logger { get; set; }

    public static bool Initialize(
        IServiceProvider serviceProvider,
        string? crashFilePath = null
    )
    {
        CrashFilePath = string.IsNullOrEmpty(crashFilePath)
            ? Path.Combine(Environment.CurrentDirectory, "crashFile.txt")
            : crashFilePath;

        ServiceProvider = serviceProvider;
        Logger = ServiceProvider.GetService<IJ4JLogger>();

        IsInitialized = true;

        return true;
    }

    public static bool Initialize(
        J4JHostConfiguration? hostConfig,
        string? crashFilePath = null
    )
    {
        CrashFilePath = string.IsNullOrEmpty( crashFilePath )
            ? Path.Combine( Environment.CurrentDirectory, "crashFile.txt" )
            : crashFilePath;

        if (hostConfig == null)
        {
            OutputFatalMessage($"Undefined {typeof(J4JHostConfiguration)}", null);

            return false;
        }

        if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
        {
            OutputFatalMessage(
                $"Missing {typeof( J4JHostConfiguration )} items: {hostConfig.MissingRequirements}",
                hostConfig.Logger );

            return false;
        }

        var host = hostConfig.Build();

        if( host != null )
        {
            ServiceProvider = host.Services;
            Logger = host.Services.GetService<IJ4JLogger>();
            IsInitialized = true;

            return true;
        }

        OutputFatalMessage( $"Could not create {typeof( IJ4JHost )}",
                            hostConfig.Logger );

        return false;
    }

    public static void OutputFatalMessage( string msg, IJ4JLogger? logger )
    {
        // how we log depends on whether we successfully created the service provider
        logger?.Fatal( msg );

        if( !FileIsWriteable( CrashFilePath ) )
            return;

        try
        {
            File.AppendAllText( CrashFilePath!, $"{msg}\n" );
        }
        catch
        {
        }
    }

    private static bool FileIsWriteable(string? filePath)
    {
        if( string.IsNullOrEmpty( filePath ) )
            return false;

        try
        {
            File.AppendAllText(filePath, "test");
            File.Delete(filePath);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static IServiceProvider? GetServiceProvider()
    {
        return ServiceProvider;
    }

    public static object? GetService( Type serviceType )
    {
        if( ServiceProvider != null )
            return ServiceProvider.GetService( serviceType );

        Logger?.Warning("Trying to retrieve service '{0}' from undefined IServiceProvider", serviceType);

        return null;
    }

    public static T? GetService<T>()
        where T : class
    {
        if (ServiceProvider != null)
            return ServiceProvider.GetService<T>();

        Logger?.Warning( "Trying to retrieve service '{0}' from undefined IServiceProvider", typeof( T ) );

        return null;
    }

    public static object GetRequiredService( Type serviceType )
    {
        try
        {
            if( ServiceProvider == null )
                Logger?.Warning( "Trying to retrieve service '{0}' from undefined IServiceProvider", serviceType );
            else return ServiceProvider.GetRequiredService( serviceType );
        }
        catch( Exception ex )
        {
            Logger?.Fatal<Type, string>( "Exception thrown requesting required service {0}, message was {1}",
                                         serviceType,
                                         ex.Message );

            throw new J4JDeusExException( $"Exception thrown requesting required service {serviceType}", ex );
        }

        throw new J4JDeusExException(
            $"Trying to retrieve service '{serviceType}' from undefined IServiceProvider" );
    }

    public static T GetRequiredService<T>()
        where T : class
    {
        try
        {
            if( ServiceProvider == null )
                Logger?.Warning( "Trying to retrieve service '{0}' from undefined IServiceProvider", typeof( T ) );
            else return ServiceProvider.GetRequiredService<T>();
        }
        catch( Exception ex )
        {
            Logger?.Fatal<Type, string>( "Exception thrown requesting required service {0}, message was {1}",
                                         typeof( T ),
                                         ex.Message );

            throw new J4JDeusExException( $"Exception thrown requesting required service {typeof( T )}", ex );
        }

        throw new J4JDeusExException(
            $"Trying to retrieve service '{typeof( T )}' from undefined IServiceProvider" );
    }
}
