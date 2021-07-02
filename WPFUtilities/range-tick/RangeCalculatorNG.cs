using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class RangeCalculatorNG
    {
        private enum EndPoint
        {
            StartOfRange,
            EndOfRange
        }

        public static double DefaultRankingFunction(RangeParametersNG rangeParameters)
        {
            var majors = Math.Abs( rangeParameters.MajorTicks - 10 );

            var fiveMinors = majors * Math.Abs(rangeParameters.MinorTicksPerMajorTick - 5);
            var tenMinors = majors * Math.Abs(rangeParameters.MinorTicksPerMajorTick - 10);

            return ( fiveMinors < tenMinors ? fiveMinors : tenMinors ) 
                   + rangeParameters.LowerInactiveRegion 
                   + rangeParameters.UpperInactiveRegion;
        }

        private readonly IMinorTickEnumerator _minorTickEnumerator;
        private readonly IJ4JLogger? _logger;

        public RangeCalculatorNG(
            IMinorTickEnumerator minorTickEnumerator,
            IJ4JLogger? logger
        )
        {
            _minorTickEnumerator = minorTickEnumerator;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public Func<RangeParametersNG, double> RankingFunction { get; set; } = DefaultRankingFunction;
        public EndPointNature StartingPointNature { get; set; } = EndPointNature.Inclusive;
        public EndPointNature EndingPointNature { get; set; } = EndPointNature.Inclusive;

        public bool IsValid => Alternatives.Any() && BestFit != null;
        public List<RangeParametersNG> Alternatives { get; } = new();
        public RangeParametersNG? BestFit { get; private set; }

        public RangeParametersNG Evaluate( double minValue, double maxValue )
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
                var adjMinValue = GetAdjustedEndPoint( minValue, minorTick.Size, EndPoint.StartOfRange );
                var adjMaxValue = GetAdjustedEndPoint( maxValue, minorTick.Size, EndPoint.EndOfRange );

                var totalMinorTicks = GetMinorTicksInRange( adjMinValue, adjMaxValue, minorTick.Size );

                var majorTicks = (int) totalMinorTicks / minorTick.NumberPerMajor;

                var modulo = totalMinorTicks % minorTick.NumberPerMajor;
                if( modulo != 0 ) majorTicks++;

                Alternatives.Add( new RangeParametersNG(
                    majorTicks,
                    minorTick.NumberPerMajor,
                    minorTick.Size,
                    adjMinValue,
                    adjMaxValue,
                    Math.Abs(adjMinValue - minValue),
                    Math.Abs(adjMaxValue - maxValue) )
                );
            }

            if( !Alternatives.Any() )
                Alternatives.Add(GetDefaultRange(minValue, maxValue));

            BestFit = Alternatives!.OrderBy(x => RankingFunction(x))
                .First();

            return BestFit;
        }

        private double GetAdjustedEndPoint(double toAdjust, double minorTickSize, EndPoint endPoint)
        {
            return endPoint switch
            {
                EndPoint.StartOfRange => StartingPointNature switch
                {
                    EndPointNature.Inclusive => RoundDown(toAdjust, minorTickSize),
                    EndPointNature.Exclusive => RoundUp(toAdjust, minorTickSize),
                    _ => log_error()
                },
                EndPoint.EndOfRange => EndingPointNature switch
                {
                    EndPointNature.Inclusive => RoundUp(toAdjust, minorTickSize),
                    EndPointNature.Exclusive => RoundDown(toAdjust, minorTickSize),
                    _ => log_error()
                },
                _ => log_error()
            };

            double log_error()
            {
                _logger?.Error("Unsupported EndPoint value or EndPointNature");
                return toAdjust;
            }
        }

        private double RoundUp(double toRound, double minorTickSize)
        {
            var modulo = toRound % minorTickSize;
            if (modulo == 0)
                return toRound;

            return toRound < 0 ? toRound - modulo : toRound + minorTickSize - modulo;
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

        private RangeParametersNG GetDefaultRange(double minValue, double maxValue)
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);

            var majorSize = Math.Pow(10, (int)exponent - 1);
            var minorTickSize = majorSize / 10;

            var numMajor = Convert.ToInt32(range / majorSize);
            if (range % majorSize != 0)
                numMajor++;

            var rangeStart = GetAdjustedEndPoint( minValue, minorTickSize, EndPoint.StartOfRange );
            var rangeEnd = GetAdjustedEndPoint( maxValue, minorTickSize, EndPoint.EndOfRange );

            return new RangeParametersNG(
                numMajor,
                10,
                minorTickSize,
                rangeStart,
                rangeEnd,
                Math.Abs( rangeStart - minValue ),
                Math.Abs( rangeEnd - maxValue ) );
        }
    }
}
