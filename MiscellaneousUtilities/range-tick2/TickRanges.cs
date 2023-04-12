#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TickRanges.cs
//
// This file is part of JumpForJoy Software's MiscellaneousUtilities.
// 
// MiscellaneousUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MiscellaneousUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MiscellaneousUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public class TickRanges
{
    private readonly List<ITickRange> _rangers;
    private readonly ILogger? _logger;

    public TickRanges( 
        IEnumerable<ITickRange> rangers,
        ILoggerFactory? loggerFactory = null 
        )
    {
        _rangers = rangers.ToList();

        _logger = loggerFactory?.CreateLogger(GetType());
    }

    public bool IsSupported( Type toCheck ) => _rangers.Any( x => x.IsSupported( toCheck ) );
    public bool IsSupported<T>() => _rangers.Any( x => x.IsSupported( typeof( T ) ) );

    public ITickRange? GetRanger<TValue>( ITickRangeConfig? config )
    {
        var retVal = _rangers.FirstOrDefault( x => x.IsSupported( typeof( TValue ) ) );

        if( retVal == null )
        {
            _logger?.LogError( "No ITickRange class supporting '{0}' exists", typeof( TValue ) );
            return null;
        }

        if( config == null )
            return retVal;

        if( !retVal.Configure( config ) )
            _logger?.LogError( "Failed to configure ITickRange supporting '{0}'", typeof( TValue ) );

        return retVal;
    }

    public bool GetRange<TValue, TResult>( int controlSize,
        TValue minValue,
        TValue maxValue,
        out TResult? result,
        ITickRangeConfig? config = null )
        where TResult : class
    {
        result = null;

        if( minValue == null || maxValue == null )
        {
            _logger?.LogError( "One or both of the minimum/maximum values are null" );
            return false;
        }

        var ranger = GetRanger<TValue>( config );
        if( ranger == null )
            return false;

        if( !ranger.GetRange( controlSize, minValue, maxValue, out var innerResult ) )
            return false;

        result = innerResult as TResult;

        return result != null;
    }

    public List<TResult> GetRanges<TValue, TResult>( int controlSize,
        TValue minValue,
        TValue maxValue,
        ITickRangeConfig? config = null )
        where TResult : class
    {
        if( minValue == null || maxValue == null )
        {
            _logger?.LogError( "One or both of the minimum/maximum values are null" );
            return new List<TResult>();
        }

        var ranger = GetRanger<TValue>( config );
        if ( ranger == null )
            return new List<TResult>();

        try
        {
            return ranger.GetRanges( controlSize, minValue, maxValue ).Cast<TResult>().ToList();
        }
        catch
        {
            _logger?.LogError( "Range values were not '{0}'", typeof( TResult ) );
            return new List<TResult>();
        }
    }

    public bool GetRange<TValue, TResult>( int controlSize,
        int tickSize,
        TValue minValue,
        TValue maxValue,
        out TResult? result,
        ITickRangeConfig? config = null )
        where TResult : class
    {
        result = null;

        if( minValue == null || maxValue == null )
        {
            _logger?.LogError( "One or both of the minimum/maximum values are null" );
            return false;
        }

        var ranger = GetRanger<TValue>( config );
        if ( ranger == null )
            return false;

        if ( !ranger.GetRange( controlSize, tickSize, minValue, maxValue, out var innerResult ) )
            return false;

        result = innerResult as TResult;

        return result != null;
    }
}