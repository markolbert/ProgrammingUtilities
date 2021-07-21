using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class RangeCalculator : IRangeCalculator
    {
        private readonly ITickManagers _managers;
        private readonly IJ4JLogger? _logger;

        public RangeCalculator(
            ITickManagers managers,
            IJ4JLogger? logger
        )
        {
            _managers = managers;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsValid => Alternatives.Any();
        public List<RangeParameters> Alternatives { get; } = new();

        public void Evaluate<TSource>( [DisallowNull] TSource min, [DisallowNull] TSource max, ITickManager? manager = null )
        {
            if( !ValidateManager<TSource>( ref manager ) ) 
                return;

            if( !manager!.ExtractDouble( min, out var minValue ) )
            {
                _logger?.Error<string>( "Could not extract a double value from min value '{0}'",
                    min.ToString() ?? "**undefined Type**" );
                return;
            }

            if (!manager.ExtractDouble(max, out var maxValue))
            {
                _logger?.Error<string>("Could not extract a double value from min value '{0}'",
                    min.ToString() ?? "**undefined Type**");
                return;
            }

            if (minValue > maxValue)
            {
                _logger?.Warning("Minimum ({0}) and maximum ({1}) values were reversed, correcting",
                    minValue,
                    maxValue);

                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            EvaluateInternal(manager, minValue!.Value, maxValue!.Value);
        }

        public void Evaluate<TSource>( IEnumerable<TSource> source, ITickManager? manager = null)
        {
            if (!ValidateManager<TSource>(ref manager))
                return;

            if ( !manager!.ExtractDoubles( source.Cast<object>(), out var sourceList ) )
            {
                _logger?.Error("Could not convert source objects into list of doubles");
                return;
            }

            if ( !sourceList!.Any() )
            {
                _logger?.Error("source list was empty");
                return;
            }

            var minValue = sourceList!.Min();
            var maxValue = sourceList!.Max();

            EvaluateInternal( manager, minValue, maxValue );
        }

        private bool ValidateManager<TSource>(ref ITickManager? manager)
        {
            manager ??= _managers[typeof(TSource)];

            if (manager == null)
            {
                _logger?.Error("No ITickManager defined for type '{0}'", typeof(TSource));
                return false;
            }

            Alternatives.Clear();

            return true;
        }

        private void EvaluateInternal( ITickManager manager, double minValue, double maxValue )
        {
            foreach( var minorTick in manager.GetTickValues( minValue, maxValue ) )
            {
                var roundedMin = minorTick.RoundDown( minValue );
                var roundedMax = minorTick.RoundUp( maxValue );

                Alternatives.Add( new RangeParameters(
                    minorTick,
                    minValue,
                    maxValue,
                    roundedMin,
                    roundedMax )
                );
            }

            if( !Alternatives.Any() )
                Alternatives.Add( manager.GetDefaultRange( minValue, maxValue ) );
        }
    }
}
