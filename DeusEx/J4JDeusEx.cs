﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of DeusEx.
//
// DeusEx is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DeusEx is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DeusEx. If not, see <https://www.gnu.org/licenses/>.

using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.DeusEx;

public class J4JDeusEx
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if( _serviceProvider != null )
                return _serviceProvider;

            Logger?.Fatal("IServiceProvider is undefined");
            throw new J4JDeusExException( "IServiceProvider is undefined" );
        }

        protected set => _serviceProvider = value;
    }

    public static bool IsInitialized { get; protected set; }
    public static string? CrashFilePath { get; protected set; }
    public static IJ4JLogger? Logger { get; protected set; }

    public static IJ4JLogger? GetLogger() => IsInitialized ? _serviceProvider?.GetService<IJ4JLogger>() : null;

    public static IJ4JLogger? GetLogger<T>() 
        where T: class
    {
        if( !IsInitialized )
            return null;

        var retVal = _serviceProvider?.GetService<IJ4JLogger>();
        if( retVal == null )
            return retVal;

        retVal.SetLoggedType<T>();
        return retVal;
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
        // ReSharper disable once EmptyGeneralCatchClause
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
}
