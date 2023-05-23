#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// WinAppInitializerBase.cs
//
// This file is part of JumpForJoy Software's WindowsUtilities.
// 
// WindowsUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WindowsUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WindowsUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace J4JSoftware.WindowsUtilities;

public class WinAppInitializerBase<TApp, TConfig> : IWinAppInitializer
    where TApp : Application
    where TConfig : AppConfigBase, new()
{
    protected WinAppInitializerBase(
        string configFileName = "userConfig.json"
        )
    {
        var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dpProvider = DataProtectionProvider.Create(
            new DirectoryInfo(Path.Combine(localAppFolder, "ASP.NET", "DataProtection-Keys")));
        Protector = dpProvider.CreateProtector( typeof( TApp ).Name );

        ConfigurationFilePath = Path.Combine(AppConfigBase.UserFolder, configFileName);
    }

    public bool IsInitialized { get; private set; }

    public bool Initialize()
    {
        var serilogConfig = GetSerilogConfiguration();
        if (serilogConfig != null)
        {
            LoggerFactory = new LoggerFactory().AddSerilog(serilogConfig.CreateLogger());
            Logger = LoggerFactory.CreateLogger<TApp>();
        }

        try
        {
            Services = CreateHostBuilder()
                      .Build()
                      .Services;

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            IsInitialized = false;
            Logger?.LogCritical("Failed to initialize app, message was '{mesg}'", ex.Message);
        }

        return IsInitialized;
    }

    public string ConfigurationFilePath { get; }
    public bool SaveConfigurationOnExit { get; set; } = true;

    public TConfig? ApplicationConfiguration { get; private set; }
    public IServiceProvider? Services { get; private set; }
    public IDataProtector Protector { get; }
    public ILoggerFactory? LoggerFactory { get; set; }
    public ILogger? Logger { get; set; }

    protected virtual LoggerConfiguration? GetSerilogConfiguration() => null;

    protected virtual IHostBuilder CreateHostBuilder() =>
        new HostBuilder()
           .ConfigureHostConfiguration( builder => ParseConfigurationFile( ConfigurationFilePath, builder ) )
           .ConfigureServices( ( hbc, s ) => ConfigureServices( hbc, s ) );

    protected virtual IServiceCollection ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
#pragma warning restore IDE0060
    {
        if( LoggerFactory != null )
            services.AddSingleton( LoggerFactory );

        services.AddSingleton( Protector );
        services.AddSingleton( ApplicationConfiguration! );

        return services;
    }

    protected virtual void ParseConfigurationFile(string path, IConfigurationBuilder builder)
    {
        var fileExists = File.Exists(path);
        if (!fileExists)
        {
            Logger?.LogWarning("Could not find user config file '{path}', creating default configuration", path);
            ApplicationConfiguration = new TConfig { UserConfigurationFilePath = path };

            return;
        }

        var encrypted = JsonSerializer.Deserialize<TConfig>(File.ReadAllText(path));

        if (encrypted == null)
        {
            Logger?.LogError("Could not parse user config file '{path}'", path);
            return;
        }

        encrypted.UserConfigurationFilePath = path;

        ApplicationConfiguration = (TConfig) encrypted.Decrypt( Protector );
    }

    AppConfigBase? IWinAppInitializer.AppConfig => ApplicationConfiguration;
}
