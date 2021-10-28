using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public class TickRanges : ITickRange
    {
        private readonly List<ITickRange> _rangers;
        private readonly IJ4JLogger? _logger;

        public TickRanges(
            IEnumerable<ITickRange> rangers,
            IJ4JLogger? logger = null
        )
        {
            _rangers = rangers.ToList();

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsSupported( object value ) => _rangers.Any( x => x.IsSupported( value ) );

        public bool GetRange( int controlSize, object minValue, object maxValue, out object? result )
        {
            result = null;

            var ranger = _rangers.FirstOrDefault( x => x.IsSupported( minValue ) );
            if( ranger != null ) 
                return ranger.GetRange( controlSize, minValue, maxValue, out result );
            
            _logger?.Error( "No ITickRange supports type '{0}'", minValue.GetType() );
            
            return false;
        }

        public List<object> GetRanges( int controlSize, object minValue, object maxValue )
        {
            var ranger = _rangers.FirstOrDefault( x => x.IsSupported( minValue ) );
            if( ranger != null )
                return ranger.GetRanges( controlSize, minValue, maxValue );

            _logger?.Error( "No ITickRange supports type '{0}'", minValue.GetType() );

            return new List<object>();
        }

        public bool GetRange( int controlSize, int tickSize, object minValue, object maxValue, out object? result )
        {
            result = null;

            var ranger = _rangers.FirstOrDefault(x => x.IsSupported(minValue));
            if( ranger != null )
                return ranger.GetRange( controlSize, tickSize, minValue, maxValue, out result );

            _logger?.Error("No ITickRange supports type '{0}'", minValue.GetType());

            return false;
        }
    }
}
