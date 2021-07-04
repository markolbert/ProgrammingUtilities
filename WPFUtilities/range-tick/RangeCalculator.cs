using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class RangeCalculator
    {
        public static double DefaultRankingFunction(RangeParameters rangeParameters)
        {
            var majors = Math.Abs( rangeParameters.MajorTicks - 10 );

            var fiveMinors = Math.Abs(rangeParameters.MinorTicksPerMajorTick - 5);
            var tenMinors = Math.Abs(rangeParameters.MinorTicksPerMajorTick - 10);

            return ( fiveMinors < tenMinors ? fiveMinors : tenMinors ) 
                   + majors
                   + rangeParameters.LowerInactiveRegion 
                   + rangeParameters.UpperInactiveRegion;
        }

        private readonly IMinorTickEnumerator _minorTickEnumerator;
        private readonly IJ4JLogger? _logger;

        public RangeCalculator(
            IMinorTickEnumerator minorTickEnumerator,
            IJ4JLogger? logger
        )
        {
            _minorTickEnumerator = minorTickEnumerator;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public Func<RangeParameters, double> RankingFunction { get; set; } = DefaultRankingFunction;

        public bool IsValid => Alternatives.Any() && BestFit != null;
        public List<RangeParameters> Alternatives { get; } = new();
        public RangeParameters? BestFit { get; private set; }

        public RangeParameters Evaluate( double minValue, double maxValue )
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

            foreach( var minorTick in _minorTickEnumerator.GetEnumerator( minValue, maxValue ) )
            {
                var roundedMin = RoundDown( minValue, minorTick.Size );
                var roundedMax = RoundUp( maxValue, minorTick.Size );

                var totalMinorTicks = GetMinorTicksInRange( roundedMin, roundedMax, minorTick.Size );

                var majorTicks = (int) totalMinorTicks / minorTick.NumberPerMajor;

                var modulo = totalMinorTicks % minorTick.NumberPerMajor;
                if( modulo != 0 ) majorTicks++;

                Alternatives.Add( new RangeParameters(
                    majorTicks,
                    minorTick.NumberPerMajor,
                    minorTick.Size,
                    roundedMin,
                    roundedMax,
                    Math.Abs(roundedMin - minValue),
                    Math.Abs(roundedMax - maxValue) )
                );
            }

            if( !Alternatives.Any() )
                Alternatives.Add(GetDefaultRange(minValue, maxValue));

            //var junk = Alternatives.OrderBy(x => RankingFunction(x))
            //    .Select(x => new
            //    {
            //        Parameters = x,
            //        FigureOfMerit = RankingFunction(x)
            //    })
            //    .ToList();

            BestFit = Alternatives!.OrderBy(x => RankingFunction(x))
                .First();

            return BestFit;
        }

        private double RoundUp(double toRound, double minorTickSize)
        {
            var modulo = toRound % minorTickSize;
            if (modulo == 0)
                return toRound;

            var upperOffset = _minorTickEnumerator.UpperLimitIsInclusive ? 1 : 0;

            return toRound < 0 ? toRound - modulo : toRound + minorTickSize - modulo - upperOffset;
        }

        private double RoundDown(double toRound, double root)
        {
            var modulo = toRound % root;
            if (modulo == 0)
                return toRound;

            return toRound < 0 ? toRound - root - modulo : toRound - modulo;
        }

        private int GetMinorTicksInRange(double minValue, double maxValue, double minorTickWidth)
        {
            var range = maxValue - minValue;

            if (range == 0)
                return 1;

            return (int) Math.Round( range / minorTickWidth );
        }

        private RangeParameters GetDefaultRange(double minValue, double maxValue)
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);
            var minorTickSize = new ScaledMinorTick(1, (int)exponent - 1, 10);

            var majorSize = Math.Pow(10, (int)exponent - 1);

            var numMajor = Convert.ToInt32(range / majorSize);
            if (range % majorSize != 0)
                numMajor++;

            var rangeStart = RoundDown( minValue, minorTickSize.Size );
            var rangeEnd = RoundUp( maxValue, minorTickSize.Size );

            return new RangeParameters(
                numMajor,
                10,
                minorTickSize.Size,
                rangeStart,
                rangeEnd,
                Math.Abs( rangeStart - minValue ),
                Math.Abs( rangeEnd - maxValue ) );
        }
    }
}
