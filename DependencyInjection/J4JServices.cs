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
using System.IO;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS8618

namespace J4JSoftware.DependencyInjection.Deprecated;

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
