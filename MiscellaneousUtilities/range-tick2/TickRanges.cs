using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public class TickRanges
    {
        private readonly List<ITickRange> _rangers;
        private readonly IJ4JLogger? _logger;

        public TickRanges( IEnumerable<ITickRange> rangers,
                           IJ4JLogger? logger = null )
        {
            _rangers = rangers.ToList();

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsSupported( Type toCheck ) => _rangers.Any( x => x.IsSupported( toCheck ) );
        public bool IsSupported<T>() => _rangers.Any( x => x.IsSupported( typeof( T ) ) );

        public ITickRange? GetRanger<TValue>( ITickRangeConfig? config )
        {
            var retVal = _rangers.FirstOrDefault( x => x.IsSupported( typeof( TValue ) ) );

            if( retVal == null )
            {
                _logger?.Error( "No ITickRange class supporting '{0}' exists", typeof( TValue ) );
                return null;
            }

            if( config == null )
                return retVal;

            if( !retVal!.Configure( config ) )
                _logger?.Error( "Failed to configure ITickRange supporting '{0}'", typeof( TValue ) );

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
                _logger?.Error( "One or both of the minimum/maximum values are null" );
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
                _logger?.Error( "One or both of the minimum/maximum values are null" );
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
                _logger?.Error( "Range values were not '{0}'", typeof( TResult ) );
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
                _logger?.Error( "One or both of the minimum/maximum values are null" );
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
}
