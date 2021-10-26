using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public enum TickSizePreference
    {
        Smallest,
        Largest
    }

    public class NumericTickRange
    {
        private readonly List<NumericTick> _numericTicks = new();
        private readonly IJ4JLogger? _logger;

        public NumericTickRange(
            IJ4JLogger? logger = null
        )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            _numericTicks.Add(new NumericTick(2));
            _numericTicks.Add(new NumericTick(4));
            _numericTicks.Add(new NumericTick(5));
            _numericTicks.Add(new NumericTick(8));
            _numericTicks.Add(new NumericTick(10));
        }

        public ReadOnlyCollection<NumericTick> AlternativeTicks => _numericTicks.AsReadOnly();

        public bool GetRange(
            uint controlSize,
            decimal minValue,
            decimal maxValue,
            out Range? result,
            TickSizePreference sizePref = TickSizePreference.Smallest )
        {
            var ranges = GetRanges( controlSize, minValue, maxValue );

            var sorted = ranges.OrderByDescending( x => x.Coverage );
            result = sizePref switch
                {
                    TickSizePreference.Smallest => sorted.ThenBy(x=>x.TickSize).FirstOrDefault(),
                    _=> sorted.ThenByDescending(x=>x.TickSize).FirstOrDefault()
                };
            
            return result != null;
        }

        public List<Range> GetRanges(
            uint controlSize,
            decimal minValue,
            decimal maxValue)
        {
            var retVal = new List<Range>();

            for (uint tickSize = 2; tickSize < 11; tickSize++)
            {
                if (!GetRange(controlSize, tickSize, minValue, maxValue, out var temp))
                    continue;

                retVal.Add(temp!);
            }

            return retVal;
        }

        public bool GetRange(
            uint controlSize,
            uint tickSize,
            decimal minValue,
            decimal maxValue,
            out Range? result )
        {
            result = null;

            if( controlSize == 0 )
            {
                _logger?.Warning( "Control size is 0, adjusting to 100" );
                controlSize = 100;
            }

            if( tickSize == 0 )
            {
                _logger?.Warning( "Minimum tick size is 0, adjusting to 2" );
                tickSize = 2;
            }

            // normalize the range
            if( maxValue < minValue )
            {
                _logger?.Warning( "Swapping minimum ({0}) and maximum ({1}) values", minValue, maxValue );
                ( maxValue, minValue ) = ( minValue, maxValue );
            }

            // expand when no range
            if( maxValue == minValue )
            {
                minValue -= 1M;
                _logger?.Warning( "Minimum and maximum ({0}) values are the same, adjusting minimum to {1}", maxValue,
                    minValue );
            }

            var range = maxValue - minValue;

            var foundAnswer = false;
            var prevSpaceUsed = 0M;

            // determine power of 10 scaling factor
            var tickValue = Convert.ToDouble( range ) * tickSize / controlSize;
            var pow10 = 0;

            switch( tickValue )
            {
                case >= 10:
                    while( tickValue >= 10 )
                    {
                        tickValue /= 10;
                        pow10++;
                    }

                    break;

                case < 1:
                    while( tickValue < 1 )
                    {
                        tickValue *= 10;
                        pow10--;
                    }

                    break;

                default:
                    // no scaling needed
                    break;
            }

            foreach( var numericTick in _numericTicks )
            {
                var minorValue = numericTick.NormalizedSize * PowerOf10( pow10 );

                var adjMin = minValue < 0
                    ? Math.Ceiling( minValue / minorValue ) * minorValue
                    : Math.Floor( minValue / minorValue ) * minorValue;

                var adjMax = maxValue < 0
                    ? Math.Floor( maxValue / minorValue ) * minorValue
                    : Math.Ceiling( maxValue / minorValue ) * minorValue;

                var numTicks = ( adjMax - adjMin ) / minorValue;
                var spaceUsed = numTicks * tickSize / controlSize;

                if( spaceUsed > 1M || spaceUsed <= prevSpaceUsed )
                    continue;

                foundAnswer = true;
                prevSpaceUsed = spaceUsed;

                var surplusTicks = ( controlSize - numTicks * tickSize ) / tickSize;
                var prefixTicksToAdd = Math.Floor( surplusTicks / 2 );
                var suffixTicksToAdd = surplusTicks - prefixTicksToAdd;

                result = new Range( tickSize,
                    minorValue,
                    minorValue * numericTick.TicksPer10,
                    adjMin - minorValue * prefixTicksToAdd,
                    adjMax + minorValue * suffixTicksToAdd,
                    spaceUsed );

                if( spaceUsed == 1.0M )
                    break;
            }

            if( !foundAnswer )
                _logger?.Error( "Couldn't determine ticks" );

            return foundAnswer;
        }

        private decimal PowerOf10( int exponent )
        {
            var retVal = 1M;

            switch( exponent )
            {
                case > 0:
                    while( exponent > 0 )
                    {
                        retVal *= 10;
                        exponent--;
                    }

                    break;

                case < 0:
                    while( exponent < 0 )
                    {
                        retVal /= 10;
                        exponent++;
                    }

                    break;
            }

            return retVal;
        }
    }
}
