﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Utilities
{
    public class MonthlyTickRange : ITickRange<DateTime, MonthRange>
    {
        public static int[] TraditionalMonthsPerMinor = new int[] { 2, 3, 6, 12, 18 };

        private readonly bool _traditionalMonthsPerMinorOnly;

        private int _monthsPerMinor;
        private int _traditionalIndex;
        private int _traditionalYears;

        private readonly IJ4JLogger? _logger;

        public MonthlyTickRange(
            bool traditionalMonthsPerMinorOnly = false,
            IJ4JLogger? logger = null
        )
        {
            _traditionalMonthsPerMinorOnly = traditionalMonthsPerMinorOnly;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsSupported(object value)
        {
            try
            {
                var temp = (DateTime)Convert.ChangeType(value, typeof(DateTime));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool GetRange(
            int controlSize,
            DateTime minValue,
            DateTime maxValue,
            out MonthRange? result )
        {
            var ranges = GetRanges( controlSize, minValue, maxValue );

            result = ranges.OrderByDescending( x => x.Coverage )
                .FirstOrDefault();

            return result != null;
        }

        public List<MonthRange> GetRanges(
            int controlSize,
            DateTime minValue,
            DateTime maxValue )
        {
            var retVal = new List<MonthRange>();

            for (var tickSize = 2; tickSize < 11; tickSize++)
            {
                if (!GetRange(controlSize, tickSize, minValue, maxValue, out var temp ))
                    continue;

                retVal.Add(temp!);
            }

            return retVal;
        }

        public bool GetRange(
            int controlSize,
            int tickSize,
            DateTime minValue,
            DateTime maxValue,
            out MonthRange? result )
        {
            _traditionalIndex = 0;
            _traditionalYears = 2;

            result = null;

            if( controlSize <= 0 )
            {
                _logger?.Warning( "Control size <= 0, adjusting to 100" );
                controlSize = 100;
            }

            if( tickSize <= 0 )
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
                minValue = maxValue.AddMonths( -1 );
                _logger?.Warning( "Minimum and maximum ({0}) values are the same, adjusting minimum to {1}", maxValue,
                    minValue );
            }

            var minMonthNum = minValue.Year * 12 + minValue.Month;
            var maxMonthNum = maxValue.Year * 12 + maxValue.Month;
            var numMonths = maxMonthNum - minMonthNum;

            _monthsPerMinor = 1;

            while( result == null && _monthsPerMinor < numMonths )
            {
                var adjMin = ( minMonthNum / _monthsPerMinor ) * _monthsPerMinor;

                var adjMax = ( maxMonthNum / _monthsPerMinor ) * _monthsPerMinor;
                if( adjMax < maxMonthNum )
                    adjMax += _monthsPerMinor;

                var numTicks = ( adjMax - adjMin ) / _monthsPerMinor;
                var spaceUsed = numTicks * tickSize / Convert.ToDecimal( controlSize );

                if( (spaceUsed - 1M) > 0 )
                {
                    NextMonthsPerMinor();
                    continue;
                }

                var surplusTicks = ( controlSize - numTicks * tickSize ) / tickSize;
                var prefixTicksToAdd = surplusTicks / 2;
                var suffixTicksToAdd = surplusTicks - prefixTicksToAdd;

                var startMonthNum = adjMin - prefixTicksToAdd * _monthsPerMinor;
                var startYear = startMonthNum / 12;
                var startMonth = startMonthNum - 12 * startYear + 1;

                var endMonthNum = adjMax + suffixTicksToAdd * _monthsPerMinor;
                var endYear = endMonthNum / 12;
                var endMonth = endMonthNum - 12 * endYear + 1;

                // months per major tick must be a multiple of 12 and a multiple of months per minor tick
                var minorFactors = FactorInfo.GetFactors( _monthsPerMinor );

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
                if (factorOf3 == null)
                    minorFactors.Add(new FactorInfo(3, 1));

                var monthsPerMajor = 1;

                foreach( var factorInfo in minorFactors.Where(x=>x.Factor != 1  ) )
                {
                    for( var idx = 1; idx <= factorInfo.Frequency; idx++)
                    {
                        monthsPerMajor *= factorInfo.Factor;
                    }
                }

                // if minor and major ticks are the same size, scale major ticks by five
                if( monthsPerMajor == _monthsPerMinor )
                    monthsPerMajor *= 5;

                result = new MonthRange( tickSize,
                    _monthsPerMinor,
                    monthsPerMajor,
                    new DateTime( startYear, startMonth, 1 ),
                    new DateTime( endYear, endMonth, 1 ).AddDays(-1),
                    Convert.ToDouble(spaceUsed) );
            }

            if( result == null )
                _logger?.Error( "Couldn't determine ticks" );

            return result != null;
        }

        private void NextMonthsPerMinor()
        {
            if( !_traditionalMonthsPerMinorOnly )
            {
                _monthsPerMinor++;
                return;
            }

            if( _traditionalIndex < TraditionalMonthsPerMinor.Length )
            {
                _monthsPerMinor = TraditionalMonthsPerMinor[ _traditionalIndex ];
                _traditionalIndex++;

                return;
            }

            _monthsPerMinor = 12 * _traditionalYears;
            _traditionalYears++;
        }

        bool ITickRange.GetRange(
            int controlSize,
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

        List<object> ITickRange.GetRanges( int controlSize, object minValue, object maxValue )
        {
            var converted = ConvertRange( minValue, maxValue );
            if( converted == null )
                return new List<object>();

            return GetRanges( controlSize, converted.Value.minValue, converted.Value.maxValue )
                .Cast<object>()
                .ToList();
        }

        bool ITickRange.GetRange(
            int controlSize,
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

        private (DateTime minValue, DateTime maxValue)? ConvertRange(object minimum, object maximum)
        {
            DateTime minDT;
            DateTime maxDT;

            try
            {
                minDT = (DateTime)Convert.ChangeType(minimum, typeof(DateTime));
                maxDT = (DateTime)Convert.ChangeType(maximum, typeof(DateTime));
            }
            catch
            {
                _logger?.Error<string, string>(
                    "Minimum ({0}) and/or maximum ({1}) values could not be converted to type DateTime",
                    minimum?.ToString() ?? "** unknown **",
                    maximum.ToString() ?? "** unknown **");

                return null;
            }

            return (minDT, maxDT);
        }
    }
}