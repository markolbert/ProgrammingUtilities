using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class RangeCalculators : IRangeCalculators
    {
        private readonly List<IRangeCalculator> _calculators;
        private readonly IJ4JLogger? _logger;

        public RangeCalculators( 
            IEnumerable<IRangeCalculator> calculators,
            IJ4JLogger? logger
            )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            _calculators = calculators.ToList();
        }

        public bool CalculateAlternatives<TValue>( 
            TValue minValue, 
            TValue maxValue, 
            out List<RangeParameters<TValue>>? result )
            where TValue : notnull
        {
            result = null;

            var calculatorType = typeof(RangeCalculator<>).MakeGenericType( typeof(TValue) );

            var calculator = _calculators.FirstOrDefault( x => calculatorType.IsInstanceOfType( x ) );

            if( calculator == null )
            {
                _logger?.Error( "No RangeCalculator defined for '{0}'", typeof(TValue) );
                return false;
            }

            if( !calculator.Calculate( minValue, maxValue, out var innerResult ) )
                return false;

            result = innerResult!.Cast<RangeParameters<TValue>>().ToList();

            return true;
        }

        public bool GetBestFit<TValue>(
            TValue minValue,
            TValue maxValue,
            out RangeParameters<TValue>? result,
            Func<RangeParameters<TValue>, double>? rankingFunction = null )
            where TValue : notnull
        {
            result = null;

            if( !CalculateAlternatives( minValue, maxValue, out var alternatives ) )
                return false;

            rankingFunction ??= DefaultRankingFunction;

            result = alternatives!.OrderBy( x => rankingFunction( x ) )
                .FirstOrDefault();

            return result != null;
        }

        public static double DefaultRankingFunction<TValue>( RangeParameters<TValue> rangeParameters )
        {
            var majors = Math.Pow( 2, Math.Abs( rangeParameters.MajorTicks - 10 ) );

            var fiveMinors = majors * Math.Pow(2, Math.Abs(rangeParameters.MinorTicksPerMajorTick - 5));
            var tenMinors = majors * Math.Pow(2, Math.Abs(rangeParameters.MinorTicksPerMajorTick - 10));

            return fiveMinors < tenMinors ? fiveMinors : tenMinors;
        }
    }
}
