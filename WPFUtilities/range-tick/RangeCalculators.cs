using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Markup;
using Accessibility;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

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

        public bool CalculateAlternatives<TValue>( TValue minValue, 
            TValue maxValue, 
            out List<RangeParameters<TValue>>? result,
            int minTickPowerOfTen = 2, 
            MinorTickInfo[]? tickChoices = null )
        {
            result = null;

            minTickPowerOfTen = minTickPowerOfTen < 0 ? 2 : minTickPowerOfTen;

            if( minValue == null || maxValue == null )
            {
                _logger?.Error("The minimum or maximum values, or both, are null"  );
                return false;
            }

            var calculatorType = typeof(RangeCalculator<>).MakeGenericType( typeof(TValue) );

            var calculator = _calculators.FirstOrDefault( x => calculatorType.IsInstanceOfType( x ) );

            if( calculator == null )
            {
                _logger?.Error( "No RangeCalculator defined for '{0}'", typeof(TValue) );
                return false;
            }

            tickChoices ??= MinorTickInfo.GetDefault( calculator.Style );

            if( tickChoices == null )
            {
                _logger?.Error( "Tick choices not supplied and no default defined for tick style '{0}'",
                    calculator.Style );

                return false;
            }

            if( !calculator.Calculate( minValue, maxValue, minTickPowerOfTen, tickChoices, out var innerResult ) )
                return false;

            result = innerResult!.Cast<RangeParameters<TValue>>().ToList();

            return true;
        }

        public bool GetBestFit<TValue>(
            TValue minValue,
            TValue maxValue,
            out RangeParameters<TValue>? result,
            Func<int, int, int>? ranker = null,
            int minTickPowerOfTen = 2,
            MinorTickInfo[]? tickChoices = null )
        {
            result = null;

            if( !CalculateAlternatives( minValue, maxValue, out var alternatives, minTickPowerOfTen, tickChoices ) )
                return false;

            ranker ??= ( major, minor ) => Math.Abs( 10 - major ) * Math.Abs( 10 - minor );

            result = alternatives!.OrderBy( x => ranker( x.MajorTicks, x.MinorTicksPerMajorTick ) )
                .FirstOrDefault();

            return result != null;
        }
    }
}
