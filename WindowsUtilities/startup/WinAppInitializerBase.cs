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
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace J4JSoftware.WindowsUtilities;

public class WinAppInitializerBase<TConfig>
    where TConfig : AppConfigBase, new()
{
    private readonly IWinApp _winApp;
    private readonly string _configPath;
    private readonly IDataProtector _protector;

    private TConfig? _appConfig;
    private ILoggerFactory? _loggerFactory;
    private ILogger? _logger;

    protected WinAppInitializerBase(
        IWinApp winApp,
        string configFileName = "userConfig.json"
        )
    {
        _winApp = winApp;

        var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dpProvider = DataProtectionProvider.Create(
            new DirectoryInfo(Path.Combine(localAppFolder, "ASP.NET", "DataProtection-Keys")));
        _protector = dpProvider.CreateProtector( winApp.GetType().Name );

        _configPath = Path.Combine(AppConfigBase.UserFolder, configFileName);
    }

    public bool IsInitialized { get; private set; }

    public bool Initialize()
    {
        var serilogConfig = GetSerilogConfiguration();
        if (serilogConfig != null)
        {
            _loggerFactory = new LoggerFactory().AddSerilog(serilogConfig.CreateLogger());
            _logger = _loggerFactory.CreateLogger(_winApp.GetType());
        }

        try
        {
            _winApp.Services = CreateHostBuilder()
                      .Build()
                      .Services;

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            IsInitialized = false;
            _logger?.LogCritical("Failed to initialize app, message was '{mesg}'", ex.Message);
        }

        return IsInitialized;
    }

    protected virtual LoggerConfiguration? GetSerilogConfiguration() => null;

    protected virtual IHostBuilder CreateHostBuilder() =>
        new HostBuilder()
           .ConfigureHostConfiguration( builder => ParseConfigurationFile( _configPath, builder ) )
           .ConfigureServices( ( hbc, s ) => ConfigureServices( hbc, s ) );

    protected virtual IServiceCollection ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
    {
        if( _loggerFactory != null )
            services.AddSingleton( _loggerFactory );

        services.AddSingleton( _protector );
        services.AddSingleton( _appConfig! );

        return services;
    }

    protected virtual void ParseConfigurationFile(string path, IConfigurationBuilder builder)
    {
        var fileExists = File.Exists(path);
        if (!fileExists)
        {
            _logger?.LogWarning("Could not find user config file '{path}', creating default configuration", path);
            _appConfig = new TConfig { UserConfigurationFilePath = path };

            return;
        }

        var encrypted = JsonSerializer.Deserialize<TConfig>(File.ReadAllText(path));

        if (encrypted == null)
        {
            _logger?.LogError("Could not parse user config file '{path}'", path);
            return;
        }

        encrypted.UserConfigurationFilePath = path;

        _appConfig = (TConfig) encrypted.Decrypt( _protector );
    }
}
