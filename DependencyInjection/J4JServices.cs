#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

#pragma warning disable CS8618

namespace J4JSoftware.DependencyInjection.Deprecated;

public static class J4JServices
{
    public static IServiceProvider Default { get; private set; }

    public static string CrashFile { get; private set; }

    public static ILogger? BuildLogger { get; private set; }
    public static ILoggerFactory? BuildLoggerFactory { get; private set; }
    public static bool IsValid { get; private set; }

    public static bool Initialize( J4JHostConfiguration? hostConfig, string crashFilePath )
    {
        CrashFile = crashFilePath;

        if( hostConfig == null )
        {
            OutputFatalMessage($"Undefined {typeof(J4JHostConfiguration)}");
            return false;
        }

        BuildLogger = hostConfig.BuildLogger;
        BuildLoggerFactory = hostConfig.BuildLoggerFactory;

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
        var logger = IsValid ? Default.GetRequiredService<ILogger>() : BuildLogger;
        logger?.Fatal(msg);

        try
        {
            File.AppendAllText( CrashFile, $"{msg}\n" );
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
        }
    }
}
