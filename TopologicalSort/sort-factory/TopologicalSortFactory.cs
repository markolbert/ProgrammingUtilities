using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

public class TopologicalSortFactory : ITopologicalSortFactory
{
    private readonly IJ4JLogger? _logger;

    public TopologicalSortFactory( IJ4JLogger? logger )
    {
        _logger = logger;
        _logger?.SetLoggedType( GetType() );
    }

    public bool CreateSortedList<T>( IEnumerable<T> toSort, out List<T>? result )
        where T : class, IEquatable<T>
    {
        result = null;

        var topoSort = new Nodes<T>();
        var tempProcessors = toSort.ToList();

        if( tempProcessors.Count == 0 )
        {
            _logger?.Error( "No {0} objects to topologically sort are defined", typeof( T ) );
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
                        _logger?.Error<Type, string>( "Could not find {0} type '{1}'",
                                                      typeof( T ),
                                                      predecessorType.Name );
                        return false;
                    }

                    topoSort.AddDependentNode( procInfo.Processor, predecessor );
                }
            }
        }

        if( !topoSort.Sort( out result, out var remaining ) )
            _logger?.Error( "Could not topologically sort {0} collection", typeof( T ) );

        return true;
    }
}