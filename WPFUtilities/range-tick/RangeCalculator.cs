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
        public static double DefaultRankingFunction( RangeParameters<T> rangeParameters ) =>
            Math.Abs( 10 - (int) rangeParameters.MajorTicks )
            + Math.Abs( 10 - (int) rangeParameters.TickInfo.NumberPerMajor )
            + Math.Abs( rangeParameters.LowerInactiveRegion )
            + Math.Abs( rangeParameters.UpperInactiveRegion );

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

        public Func<RangeParameters<T>, double> RankingFunction { get; set; } = DefaultRankingFunction;

        public bool IsValid => Alternatives.Any() && BestFit != null;
        public List<RangeParameters<T>> Alternatives { get; } = new();
        public RangeParameters<T>? BestFit { get; private set; }

        public RangeParameters<T> Evaluate( double minValue, double maxValue )
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

                var totalMinorTicks = minorTick.GetMinorTicksInRange( roundedMin, roundedMax );

                var majorTicks = totalMinorTicks / minorTick.NumberPerMajor;

                var modulo = totalMinorTicks % minorTick.NumberPerMajor;
                if( modulo != 0 ) majorTicks++;

                Alternatives.Add( new RangeParameters<T>(
                    majorTicks,
                    minorTick,
                    roundedMin,
                    roundedMax,
                    Math.Abs(roundedMin - minValue),
                    Math.Abs(roundedMax - maxValue) )
                );
            }

            if( !Alternatives.Any() )
                Alternatives.Add(_ticks.GetDefaultRange(minValue, maxValue));

            var junk = Alternatives.OrderBy(x => RankingFunction(x))
                .Select(x => new
                {
                    Parameters = x,
                    FigureOfMerit = RankingFunction(x)
                })
                .ToList();

            BestFit = Alternatives!.OrderBy(x => RankingFunction(x))
                .First();

            return BestFit;
        }
    }
}
