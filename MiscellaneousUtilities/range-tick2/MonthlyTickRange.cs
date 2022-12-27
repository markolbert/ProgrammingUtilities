using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public class MonthlyTickRange : ITickRange<DateTime, MonthRange>
{
    public static int[] TraditionalMonthsPerMinor = new int[] { 2, 3, 6, 12, 18 };

    private int _traditionalIndex;
    private int _traditionalYears;

    private readonly IJ4JLogger? _logger;

    public MonthlyTickRange( IJ4JLogger? logger = null )
    {
        _logger = logger;
        _logger?.SetLoggedType( GetType() );
    }

    public bool TraditionalMonthsPerMinorOnly { get; set; } = false;

    public bool IsSupported( Type toCheck ) => toCheck.IsAssignableTo( typeof( DateTime ) );
    public bool IsSupported<T>() => typeof( DateTime ).IsAssignableFrom( typeof( T ) );

    public bool Configure( ITickRangeConfig config )
    {
        if( config is not IDateTimeTickRangeConfig dtConfig )
            return false;

        TraditionalMonthsPerMinorOnly = dtConfig.TraditionalMonthsPerMinorOnly;

        return true;
    }

    public bool GetRange( double controlSize,
        DateTime minValue,
        DateTime maxValue,
        out MonthRange? result )
    {
        var ranges = GetRanges( controlSize, minValue, maxValue );

        result = ranges.OrderByDescending( x => x.Coverage )
                       .FirstOrDefault();

        return result != null;
    }

    public List<MonthRange> GetRanges( double controlSize,
        DateTime minValue,
        DateTime maxValue )
    {
        var retVal = new List<MonthRange>();

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
        DateTime minValue,
        DateTime maxValue,
        out MonthRange? result )
    {
        _traditionalIndex = 0;
        _traditionalYears = 2;

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
            minValue = maxValue.AddMonths( -1 );

        var minMonthNum = minValue.Year * 12 + minValue.Month;
        var maxMonthNum = maxValue.Year * 12 + maxValue.Month;
        var numMonths = maxMonthNum - minMonthNum;

        var monthsPerMinor = 1;

        while( result == null && monthsPerMinor < numMonths )
        {
            var adjMin = ( minMonthNum / monthsPerMinor ) * monthsPerMinor;

            var adjMax = ( maxMonthNum / monthsPerMinor ) * monthsPerMinor;
            if( adjMax < maxMonthNum )
                adjMax += monthsPerMinor;

            var numTicks = ( adjMax - adjMin ) / monthsPerMinor;
            var spaceUsed = numTicks * tickSize / Convert.ToDecimal( controlSize );

            if( ( spaceUsed - 1M ) > 0 )
            {
                monthsPerMinor = TraditionalMonthsPerMinorOnly
                    ? NextTraditionalMonthsPerMinor()
                    : monthsPerMinor + 1;

                continue;
            }

            var surplusTicks = ( controlSize - numTicks * tickSize ) / tickSize;

            var prefixTicksToAdd = IntegerFloor( surplusTicks / 2 );
            var startMonthNum = adjMin - prefixTicksToAdd * monthsPerMinor;
            var startYear = startMonthNum / 12;
            var startMonth = startMonthNum - 12 * startYear + 1;

            var suffixTicksToAdd = surplusTicks - prefixTicksToAdd;
            var endMonthNum = adjMax + suffixTicksToAdd * monthsPerMinor;
            var endYear = IntegerFloor( endMonthNum / 12 );
            var endMonth = IntegerFloor( endMonthNum - 12.0 * endYear + 1 );

            // months per major tick must be a multiple of 12 and a multiple of months per minor tick
            var minorFactors = FactorInfo.GetFactors( monthsPerMinor );

            // ensure 2 is a minor factor twice
            var factorOf2 = minorFactors.FirstOrDefault( x => x.Factor == 2 );
            if( factorOf2 == null )
                minorFactors.Add( new FactorInfo( 2, 2 ) );
            else
            {
                if( factorOf2.Frequency < 2 )
                    factorOf2.Frequency = 2;
            }

            var factorOf3 = minorFactors.FirstOrDefault( x => x.Factor == 3 );
            if( factorOf3 == null )
                minorFactors.Add( new FactorInfo( 3, 1 ) );

            var monthsPerMajor = 1;

            foreach( var factorInfo in minorFactors.Where( x => x.Factor != 1 ) )
            {
                for( var idx = 1; idx <= factorInfo.Frequency; idx++ )
                {
                    monthsPerMajor *= factorInfo.Factor;
                }
            }

            // if minor and major ticks are the same size, scale major ticks by five
            if( monthsPerMajor == monthsPerMinor )
                monthsPerMajor *= 5;

            result = new MonthRange( tickSize,
                                     monthsPerMinor,
                                     monthsPerMajor,
                                     new DateTime( startYear, startMonth, 1 ),
                                     new DateTime( endYear, endMonth, 1 ).AddDays( -1 ),
                                     Convert.ToDouble( spaceUsed ) );
        }

        if( result == null )
            _logger?.Error( "Couldn't determine ticks" );

        return result != null;
    }

    private int NextTraditionalMonthsPerMinor()
    {
        int retVal;

        if( _traditionalIndex < TraditionalMonthsPerMinor.Length )
        {
            retVal = TraditionalMonthsPerMinor[ _traditionalIndex ];
            _traditionalIndex++;
        }
        else
        {
            retVal = 12 * _traditionalYears;
            _traditionalYears++;
        }

        return retVal;
    }

    private static int IntegerFloor( double value ) => Convert.ToInt32( Math.Floor( value ) );

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

    private (DateTime minValue, DateTime maxValue)? ConvertRange( object minimum, object maximum )
    {
        DateTime minDT;
        DateTime maxDT;

        try
        {
            minDT = (DateTime) Convert.ChangeType( minimum, typeof( DateTime ) );
            maxDT = (DateTime) Convert.ChangeType( maximum, typeof( DateTime ) );
        }
        catch
        {
            _logger?.Error<string, string>( "Minimum ({0}) and/or maximum ({1}) values could not be converted to type DateTime",
                                            minimum?.ToString() ?? "** unknown **",
                                            maximum.ToString() ?? "** unknown **" );

            return null;
        }

        return ( minDT, maxDT );
    }
}