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

using System.IO;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace J4JSoftware.DeusEx;

public abstract class J4JDeusExHosted : J4JDeusEx
{
    protected abstract J4JHostConfiguration? GetHostConfiguration();

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
                hostConfig.BuildLogger );

            return false;
        }

        Logger = hostConfig.BuildLogger;

        var host = hostConfig.Build();

        if( host != null )
        {
            ServiceProvider = host.Services;

            var runTimeLogger = host.Services.GetService<ILogger>();

            if( runTimeLogger != null )
                hostConfig.BuildLoggerSink.OutputTo(runTimeLogger);

            Logger = runTimeLogger;

            IsInitialized = true;

            return true;
        }

        Logger = null;

        OutputFatalMessage( $"Could not create {typeof( IJ4JHost )}",
                            hostConfig.BuildLogger );

        return false;
    }
}
