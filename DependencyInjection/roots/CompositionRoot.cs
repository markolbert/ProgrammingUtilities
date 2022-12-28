﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of DependencyInjection.
//
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection;

[ Obsolete( "Use J4JHostConfiguration IHostBuilder instead" ) ]
public abstract class CompositionRoot
{
    private readonly string _dataProtectionPurpose;
    private readonly Func<Type?, string, int, string, string>? _filePathTrimmer;

    protected CompositionRoot( string publisher,
        string appName,
        string? dataProtectionPurpose = null,
        string osName = "Windows",
        Func<Type?, string, int, string, string>? filePathTrimmer = null )
    {
        ApplicationName = appName;

        UserConfigurationFolder =
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                          publisher,
                          appName );

        _dataProtectionPurpose = dataProtectionPurpose ?? GetType().Name;

        OperatingSystem = osName;
        _filePathTrimmer = filePathTrimmer;

        Initialize();
    }

    public string OperatingSystem { get; }

    protected virtual IEnumerable<Assembly> CommandLineAssemblies => Enumerable.Empty<Assembly>();

    protected IHostBuilder? HostBuilder { get; private set; }

    private void Initialize() => ConfigureHostBuilder();

    // override to change the way the host building process is initialized
    // doing so should be rare
    protected virtual void ConfigureHostBuilder()
    {
        HostBuilder = new HostBuilder()
           .UseServiceProviderFactory( new AutofacServiceProviderFactory() );

        HostBuilder.ConfigureAppConfiguration( SetupAppEnvironment );
        HostBuilder.ConfigureHostConfiguration( SetupConfigurationEnvironment );
        HostBuilder.ConfigureContainer<ContainerBuilder>( SetupDependencyInjection );
        HostBuilder.ConfigureServices( SetupServices );
    }

    protected virtual bool Build()
    {
        if( HostBuilder == null )
            return false;

        Host = HostBuilder.Build();

        // output anything we've logged to the startup/cached logger 
        // to the real logger
        var logger = Host!.Services.GetRequiredService<IJ4JLogger>();
        logger.OutputCache( CachedLogger );

        HostBuilder = null;

        return true;
    }

    // CachedLogger is used to capture log events during the host building process,
    // when the ultimate J4JLogger instance is not yet available
    protected J4JCachedLogger CachedLogger { get; } = new();

    protected virtual void ConfigureLogger( J4JLoggerConfiguration loggerConfig )
    {
    }

    public IHost? Host { get; private set; }
    public bool Initialized => Host != null;
    public string ApplicationName { get; }
    public abstract string ApplicationConfigurationFolder { get; }
    public string UserConfigurationFolder { get; }
    public IJ4JProtection Protection => Host?.Services.GetRequiredService<IJ4JProtection>()!;

    public IConfiguration? Configuration { get; private set; }

    public IParser? Parser { get; private set; }
    public CommandLineSource? CommandLineSource { get; private set; }
    public OptionCollection? CommandLineOptions { get; private set; }

    protected virtual void SetupAppEnvironment( HostBuilderContext hbc, IConfigurationBuilder builder )
    {
        Configuration = hbc.Configuration;
    }

    protected virtual void SetupConfigurationEnvironment( IConfigurationBuilder builder )
    {
        Parser = OperatingSystem.Equals( "windows", StringComparison.OrdinalIgnoreCase )
            ? J4JSoftware.Configuration.CommandLine.Parser.GetWindowsDefault( logger: CachedLogger )
            : J4JSoftware.Configuration.CommandLine.Parser.GetLinuxDefault( logger: CachedLogger );

        builder.AddJ4JCommandLine( Parser, out var cmdLineSrc, CachedLogger );
        CommandLineOptions = Parser.Collection;
        CommandLineSource = cmdLineSrc;

        if( CommandLineOptions == null )
            return;

        ConfigureCommandLineParsing();
        CommandLineOptions.FinishConfiguration();
    }

    protected virtual void ConfigureCommandLineParsing()
    {
    }

    protected virtual void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.RegisterType<J4JProtection>()
               .WithParameter( "purpose", _dataProtectionPurpose )
               .As<IJ4JProtection>()
               .SingleInstance();

        builder.RegisterType<DataProtection>()
               .As<IDataProtection>()
               .OnActivating( x => x.Instance.Purpose = _dataProtectionPurpose )
               .SingleInstance();

        builder.Register( c =>
                {
                    var loggerConfig = new J4JLoggerConfiguration( _filePathTrimmer );
                    ConfigureLogger( loggerConfig );

                    return loggerConfig.CreateLogger();
                } )
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    protected virtual void SetupServices( HostBuilderContext hbc, IServiceCollection services )
    {
        services.AddDataProtection();
    }

    #region Encryption/decryption

    public bool Protect( string plainText, out string? encrypted )
    {
        encrypted = null;

        var dataProtection = Host?.Services.GetService<IDataProtection>();
        if( dataProtection == null )
            return false;

        var utf8 = new UTF8Encoding();
        var bytesToEncrypt = utf8.GetBytes( plainText );

        try
        {
            var encryptedBytes = dataProtection.Protector.Protect( bytesToEncrypt );
            encrypted = Convert.ToBase64String( encryptedBytes );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool Unprotect( string encryptedText, out string? decrypted )
    {
        decrypted = null;

        var dataProtection = Host?.Services.GetService<IDataProtection>();
        if( dataProtection == null )
            return false;

        byte[] decryptedBytes;

        try
        {
            var encryptedBytes = Convert.FromBase64String( encryptedText );
            decryptedBytes = dataProtection.Protector.Unprotect( encryptedBytes );
        }
        catch
        {
            return false;
        }

        var utf8 = new UTF8Encoding();
        decrypted = utf8.GetString( decryptedBytes );

        return true;
    }

    #endregion
}