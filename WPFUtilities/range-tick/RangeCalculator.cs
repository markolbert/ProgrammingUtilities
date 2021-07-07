using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class RangeCalculator<T>
        where T : ScaledTick, new()
    {
        private readonly IRangeTicks<T> _ticks;
        private readonly IJ4JLogger? _logger;

        public RangeCalculator(
            IRangeTicks<T> ticks,
            IJ4JLogger? logger
        )
        {
            _ticks = ticks;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsValid => Alternatives.Any();
        public List<RangeParameters<T>> Alternatives { get; } = new();

        public void Evaluate( double minValue, double maxValue )
        {
            Alternatives.Clear();

            if (minValue.CompareTo(maxValue) > 0)
            {
                _logger?.Warning("Minimum ({0}) and maximum ({1}) values were reversed, correcting",
                    minValue,
                    maxValue);

                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            foreach( var minorTick in _ticks.GetEnumerator( minValue, maxValue ) )
            {
                var roundedMin = minorTick.RoundDown( minValue );
                var roundedMax = minorTick.RoundUp( maxValue );

                Alternatives.Add( new RangeParameters<T>(
                    minorTick,
                    minValue,
                    maxValue,
                    roundedMin,
                    roundedMax )
                );
            }

            if( !Alternatives.Any() )
                Alternatives.Add(_ticks.GetDefaultRange(minValue, maxValue));
        }
    }
}
