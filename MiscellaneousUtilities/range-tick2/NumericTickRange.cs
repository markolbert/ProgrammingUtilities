using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

public class NumericTickRange : ITickRange<decimal, NumericRange>
{
    public static List<NumericTick> NumericTicks { get; } = new List<NumericTick>();

    static NumericTickRange()
    {
        NumericTicks.Add( new NumericTick( 2 ) );
        NumericTicks.Add( new NumericTick( 4 ) );
        NumericTicks.Add( new NumericTick( 5 ) );
        NumericTicks.Add( new NumericTick( 8 ) );
        NumericTicks.Add( new NumericTick( 10 ) );
    }

    private readonly IJ4JLogger? _logger;

    public NumericTickRange( IJ4JLogger? logger = null )
    {
        _logger = logger;
        _logger?.SetLoggedType( GetType() );
    }

    public TickSizePreference TickSizePreference { get; set; } = TickSizePreference.Smallest;

    public bool IsSupported( Type toCheck )
    {
        if( !toCheck.IsValueType )
            return false;

        try
        {
            var value = Activator.CreateInstance( toCheck );
            if( value == null )
                return false;

            var temp = (decimal) Convert.ChangeType( value, typeof( decimal ) );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool IsSupported<T>() => IsSupported( typeof( T ) );

    public bool Configure( ITickRangeConfig config )
    {
        if( config is not INumericTickRangeConfig numericConfig )
            return false;

        TickSizePreference = numericConfig.TickSizePreference;

        return true;
    }

    public bool GetRange( double controlSize,
        decimal minValue,
        decimal maxValue,
        out NumericRange? result )
    {
        var ranges = GetRanges( controlSize, minValue, maxValue );

        var sorted = ranges.OrderByDescending( x => x.Coverage );

        result = TickSizePreference switch
        {
            TickSizePreference.Smallest => sorted.ThenBy( x => x.TickSize ).FirstOrDefault(),
            _                           => sorted.ThenByDescending( x => x.TickSize ).FirstOrDefault()
        };

        return result != null;
    }

    public List<NumericRange> GetRanges( double controlSize,
        decimal minValue,
        decimal maxValue )
    {
        var retVal = new List<NumericRange>();

        for( var tickSize = 2; tickSize < 11; tickSize++ )
        {
            if( !GetRange( controlSize, tickSize, minValue, maxValue, out var temp ) )
                continue;

            retVal.Add( temp! );
        }

        return retVal;
    }

    public bool GetRange( double controlSize,
        int tickSize,
        decimal minValue,
        decimal maxValue,
        out NumericRange? result )
    {
        result = null;

        if( controlSize <= 0 )
            controlSize = 100;

        if( tickSize <= 0 )
            tickSize = 2;

        // normalize the range
        if( maxValue < minValue )
            ( maxValue, minValue ) = ( minValue, maxValue );

        // expand when no range
        if( maxValue == minValue )
            minValue -= 1M;

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

        foreach( var numericTick in NumericTicks )
        {
            var minorValue = numericTick.NormalizedSize * PowerOf10( pow10 );

            var adjMin = minValue < 0
                ? Math.Floor( minValue / minorValue ) * minorValue
                : Math.Ceiling( minValue / minorValue ) * minorValue;

            var adjMax = maxValue < 0
                ? Math.Floor( maxValue / minorValue ) * minorValue
                : Math.Ceiling( maxValue / minorValue ) * minorValue;

            var numTicks = ( adjMax - adjMin ) / minorValue;
            var spaceUsed = numTicks * tickSize / (decimal) controlSize;

            if( ( spaceUsed - 1M ) > 0 || spaceUsed <= prevSpaceUsed )
                continue;

            foundAnswer = true;
            prevSpaceUsed = spaceUsed;

            var surplusTicks = ( ( (decimal) controlSize ) - numTicks * tickSize ) / tickSize;
            var prefixTicksToAdd = Math.Floor( surplusTicks / 2 );
            var suffixTicksToAdd = Math.Ceiling(surplusTicks - prefixTicksToAdd);

            result = new NumericRange( tickSize,
                                       minorValue,
                                       minorValue * numericTick.TicksPer10,
                                       adjMin - minorValue * prefixTicksToAdd,
                                       adjMax + minorValue * suffixTicksToAdd,
                                       Convert.ToDouble( spaceUsed ) );

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

    bool ITickRange.GetRange( double controlSize,
        object minValue,
        object maxValue,
        out object? result )
    {
        result = null;

        var converted = ConvertRange( minValue, maxValue );
        if( converted == null )
            return false;

        if( GetRange( controlSize, converted.Value.minValue, converted.Value.maxValue, out var innerResult ) )
            result = innerResult;

        return result != null;
    }

    List<object> ITickRange.GetRanges( double controlSize, object minValue, object maxValue )
    {
        var converted = ConvertRange( minValue, maxValue );
        if( converted == null )
            return new List<object>();

        return GetRanges( controlSize, converted.Value.minValue, converted.Value.maxValue )
              .Cast<object>()
              .ToList();
    }

    bool ITickRange.GetRange( double controlSize,
        int tickSize,
        object minValue,
        object maxValue,
        out object? result )
    {
        result = null;

        var converted = ConvertRange( minValue, maxValue );
        if( converted == null )
            return false;

        if( GetRange( controlSize,
                      tickSize,
                      converted.Value.minValue,
                      converted.Value.maxValue,
                      out var innerResult ) )
            result = innerResult;

        return result != null;
    }

    private (decimal minValue, decimal maxValue )? ConvertRange( object minimum, object maximum )
    {
        decimal minDecimal;
        decimal maxDecimal;

        try
        {
            minDecimal = (decimal) Convert.ChangeType( minimum, typeof( decimal ) );
            maxDecimal = (decimal) Convert.ChangeType( maximum, typeof( decimal ) );
        }
        catch
        {
            _logger?.Error<string, string>( "Minimum ({0}) and/or maximum ({1}) values could not be converted to type decimal",
                                            minimum?.ToString() ?? "** unknown **",
                                            maximum.ToString() ?? "** unknown **" );

            return null;
        }

        return ( minDecimal, maxDecimal );
    }
}