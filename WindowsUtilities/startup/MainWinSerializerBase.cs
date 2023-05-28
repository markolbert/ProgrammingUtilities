#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MainWinSerializerBase.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.WindowsUtilities;

public abstract class MainWinSerializerBase<TConfig>
where TConfig : AppConfigBase
{
    public static async Task<DisplayInfo?> GetPrimaryDisplayInfoAsync()
    {
        var displayList = await DeviceInformation.FindAllAsync( DisplayMonitor.GetDeviceSelector() );

        if( !displayList.Any() )
            return null;

        var monitorInfo = await DisplayMonitor.FromInterfaceIdAsync( displayList[ 0 ].Id );
        if( monitorInfo == null )
            return null;

        return new DisplayInfo( monitorInfo.NativeResolutionInRawPixels.Width,
                                monitorInfo.NativeResolutionInRawPixels.Height, 
                                monitorInfo.RawDpiX,
                                monitorInfo.RawDpiY );
    }

    public event EventHandler? WindowChanged;

    private readonly IWinApp _winApp;
    private readonly TConfig? _appConfig;
    private readonly IDataProtector _protector;
    private readonly ILogger? _logger;
    private readonly ThrottleAction _throttleWinChange = new();
    private readonly JsonSerializerOptions _jsonOptions;

    protected MainWinSerializerBase(
        Window mainWindow,
        JsonSerializerOptions? jsonOptions = null
    )
    {
        MainWindow = mainWindow;

        jsonOptions ??= new JsonSerializerOptions { WriteIndented = true };
        _jsonOptions = jsonOptions;

        _winApp = Application.Current as IWinApp
         ?? throw new NullReferenceException(
                $"{Application.Current.GetType()} does not implement required {typeof( IWinApp )} interface" );

        _appConfig = _winApp.Services.GetService<TConfig>();

        _protector = _winApp.Services.GetService<IDataProtector>()
         ?? throw new NullReferenceException(
                $"{Application.Current.GetType()} does not implement required {typeof( IDataProtector )} interface" );

        var loggerFactory = _winApp.Services.GetService<ILoggerFactory>();
        _logger = loggerFactory?.CreateLogger( GetType() );

        mainWindow.Closed += ( _, _ ) => OnMainWindowClosed();

        var hWnd = WindowNative.GetWindowHandle( mainWindow );
        var windowId = Win32Interop.GetWindowIdFromWindow( hWnd );
        AppWindow = AppWindow.GetFromWindowId( windowId );
        AppWindow.Changed += AppWindowOnChanged;
    }

    private void AppWindowOnChanged( AppWindow sender, AppWindowChangedEventArgs args )
    {
        if( args is { DidPositionChange: false, DidSizeChange: false }
        || _appConfig == null
        || AppWindow == null )
            return;


        _throttleWinChange.Throttle( 100,
                                     () =>
                                     {
                                         _appConfig.MainWindowRectangle = new PositionSize(AppWindow.Position.X,
                                             AppWindow.Position.Y,
                                             AppWindow.Size.Width,
                                             AppWindow.Size.Height);

                                         WindowChanged?.Invoke( this, EventArgs.Empty );
                                     });
    }

    protected abstract RectInt32 GetDefaultRectangle();

    public Window MainWindow { get; }
    public AppWindow? AppWindow { get; }

    public void SetSizeAndPosition( RectInt32? rect = null )
    {
        if( AppWindow == null )
            return;

        rect ??= _appConfig?.MainWindowRectangle ?? GetDefaultRectangle();

        if( rect.Value.Height == 0 || rect.Value.Width == 0 )
            rect = GetDefaultRectangle();

        AppWindow.MoveAndResize( rect.Value );
    }

    protected virtual void OnMainWindowClosed()
    {
        if( !_winApp.SaveConfigurationOnExit 
        || string.IsNullOrEmpty( _appConfig?.UserConfigurationFilePath ) )
            return;

        _appConfig.Encrypt( _protector );

        try
        {
            var jsonText = JsonSerializer.Serialize(_appConfig, _jsonOptions);

            File.WriteAllText( _appConfig.UserConfigurationFilePath, jsonText );
        }
        catch( Exception ex )
        {
            _logger?.LogError( "Failed to write configuration file, exception was '{exception}'",
                               ex.Message );
        }
    }
}
