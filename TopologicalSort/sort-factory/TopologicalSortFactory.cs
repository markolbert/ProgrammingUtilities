#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TopologicalSortFactory.cs
//
// This file is part of JumpForJoy Software's TopologicalSort.
// 
// TopologicalSort is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// TopologicalSort is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with TopologicalSort. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Utilities;

public class TopologicalSortFactory : ITopologicalSortFactory
{
    private readonly ILogger? _logger;

    public TopologicalSortFactory( 
        ILoggerFactory? loggerFactory = null 
        )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );
    }

    public bool CreateSortedList<T>( IEnumerable<T> toSort, out List<T>? result )
        where T : class, IEquatable<T>
    {
        result = null;

        var topoSort = new Nodes<T>();
        var tempProcessors = toSort.ToList();

        if( tempProcessors.Count == 0 )
        {
            _logger?.LogError( "No {0} objects to topologically sort are defined", typeof( T ) );
            return false;
        }

        foreach( var procInfo in tempProcessors
                                .Select( p => new
                                 {
                                     PredecessorAttributes = p.GetType()
                                                              .GetCustomAttributes( typeof(
                                                                       TopologicalPredecessorAttribute ),
                                                                   false )
                                                              .Cast<TopologicalPredecessorAttribute>()
                                                              .ToList(),
                                     HasRootAttribute = p.GetType()
                                                         .GetCustomAttributes( typeof(
                                                                                   TopologicalRootAttribute ),
                                                                               false )
                                                         .FirstOrDefault()
                                      != null,
                                     Processor = p
                                 } )
                                .Where( x => x.PredecessorAttributes.Any() || x.HasRootAttribute ) )
        {
            if( procInfo.HasRootAttribute )
                topoSort.AddIndependentNode( procInfo.Processor );
            else
            {
                foreach( var predecessorType in procInfo.PredecessorAttributes.Select( x => x.PredecessorType ) )
                {
                    var predecessor = tempProcessors.FirstOrDefault( x => x.GetType() == predecessorType );

                    if( predecessor == null )
                    {
                        _logger?.LogError("Could not find {0} type '{1}'",
                            typeof(T),
                            predecessorType.Name);
                        return false;
                    }

                    topoSort.AddDependentNode( procInfo.Processor, predecessor );
                }
            }
        }

        if( !topoSort.Sort( out result, out _ ) )
            _logger?.LogError( "Could not topologically sort {0} collection", typeof( T ) );

        return true;
    }
}