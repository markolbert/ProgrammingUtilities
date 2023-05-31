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
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace J4JSoftware.WindowsUtilities;

public class WinAppInitializerBase<TConfig>
    where TConfig : WinUIConfigBase, new()
{
    public const string EncryptedAppConfigPurpose = "AppConfig";

    private readonly IWinApp _winApp;
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _cryptoAppName;

    private ILogger? _logger;

    protected WinAppInitializerBase(
        IWinApp winApp,
        string configFileName = "userConfig.json",
        JsonSerializerOptions? jsonOptions = null,
        string? cryptoAppName = null
        )
    {
        _winApp = winApp;

        jsonOptions ??= new JsonSerializerOptions { WriteIndented = true };
        _jsonOptions = jsonOptions;

        _configPath = Path.Combine(WinUIConfigBase.UserFolder, configFileName);

        _cryptoAppName = cryptoAppName ?? _winApp.GetType().Name;
    }

    public bool IsInitialized { get; private set; }

    public bool Initialize()
    {
        var serilogConfig = GetSerilogConfiguration();
        if (serilogConfig != null)
        {
            LoggerFactory = new LoggerFactory().AddSerilog(serilogConfig.CreateLogger());
            _logger = LoggerFactory.CreateLogger(_winApp.GetType());
        }

        try
        {
            _winApp.Services = CreateHostBuilder()
                              .Build()
                              .Services;

            var dpProvider = _winApp.Services.GetRequiredService<IDataProtectionProvider>();
            _winApp.AppConfigProtector = dpProvider.CreateProtector( EncryptedAppConfigPurpose );

            AppConfig!.Decrypt( _winApp.AppConfigProtector );

            IsInitialized = true;
        }
        catch( CryptographicException exCrypto )
        {
            _logger?.LogError("Could not decrypt the app configuration file, message was '{mesg}'", exCrypto.Message);
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            IsInitialized = false;
            _logger?.LogCritical("Failed to initialize app, message was '{mesg}'", ex.Message);
        }

        return IsInitialized;
    }

    public TConfig? AppConfig { get; private set; }
    public ILoggerFactory? LoggerFactory { get; private set; }

    protected virtual LoggerConfiguration? GetSerilogConfiguration() => null;

    protected virtual IHostBuilder CreateHostBuilder() =>
        new HostBuilder()
           .ConfigureAppConfiguration(ConfigureApplication)
           .ConfigureServices( ( hbc, s ) => ConfigureServices( hbc, s ) );

    protected virtual void ConfigureApplication( HostBuilderContext hbc, IConfigurationBuilder builder )
    {
        var fileExists = File.Exists(_configPath);
        if (!fileExists)
        {
            _logger?.LogWarning("Could not find user config file '{path}', creating default configuration", _configPath);
            AppConfig = new TConfig { UserConfigurationFilePath = _configPath };
            return;
        }

        AppConfig = JsonSerializer.Deserialize<TConfig>(File.ReadAllText(_configPath), _jsonOptions);

        if (AppConfig == null)
        {
            _logger?.LogError("Could not parse user config file '{path}'", _configPath);
            return;
        }

        AppConfig.UserConfigurationFilePath = _configPath;
    }

    protected virtual IServiceCollection ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
    {
        if( LoggerFactory != null )
            services.AddSingleton( LoggerFactory );

        services.AddDataProtection()
                .SetApplicationName( _cryptoAppName );

        if( AppConfig != null)
            services.AddSingleton( AppConfig );

        return services;
    }
}
