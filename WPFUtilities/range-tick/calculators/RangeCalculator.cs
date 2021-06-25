#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public abstract class RangeCalculator<TValue> : IRangeCalculator<TValue> 
        where TValue : notnull, IComparable<TValue>
    {
        protected record TickInfo( decimal ScalingFactor, int MinorTicksPerMajorTick );

        protected enum TickStatus
        {
            Normal,
            ZeroRange,
            RangeExceeded
        }

        protected enum EndPoint
        {
            StartOfRange,
            EndOfRange
        }

        protected RangeCalculator(
            TickStyle style,
            IJ4JLogger? logger
        )
        {
            Style = style;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public TickStyle Style { get; }

        public bool Calculate(
            TValue minValue,
            TValue maxValue,
            out List<RangeParameters<TValue>> result )
        {
            result = new List<RangeParameters<TValue>>();

            if( minValue.CompareTo( maxValue ) > 0 )
            {
                Logger?.Warning<TValue, TValue>( "Minimum ({0}) and maximum ({1}) values were reversed, correcting",
                    minValue,
                    maxValue );

                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            var generation = 0;
            var tickStatus = TickStatus.Normal;

            while( ( tickStatus = GetScalingFactors( generation, minValue, maxValue, out var ticks ) ) 
                   != TickStatus.RangeExceeded )
            {
                foreach( var tickInfo in ticks! )
                {
                    if( !GetAdjustedEndPoint( minValue,
                        tickInfo.ScalingFactor,
                        EndPoint.StartOfRange,
                        out var adjMinValue ) )
                        return false;

                    if( !GetAdjustedEndPoint( maxValue,
                        tickInfo.ScalingFactor,
                        EndPoint.EndOfRange,
                        out var adjMaxValue ) )
                        return false;

                    var totalMinorTicks = GetMinorTicksInRange( adjMinValue!,
                        adjMaxValue!,
                        tickInfo.ScalingFactor );

                    var majorTicks = (int) totalMinorTicks / tickInfo.MinorTicksPerMajorTick;

                    var modulo = totalMinorTicks % tickInfo.MinorTicksPerMajorTick;
                    if( modulo != 0 ) majorTicks++;

                    result.Add( new RangeParameters<TValue>(
                        majorTicks,
                        tickInfo.MinorTicksPerMajorTick,
                        tickInfo.ScalingFactor,
                        adjMinValue!,
                        adjMaxValue! )
                    );
                }

                if( tickStatus == TickStatus.ZeroRange )
                    break;

                generation++;
            }

            if( !result.Any() )
                result.Add( new RangeParameters<TValue>( 1, 1, 1, minValue, maxValue ) );

            return true;
        }

        protected abstract TickStatus GetScalingFactors( 
            int generation, 
            TValue minValue, 
            TValue maxValue,
            out List<TickInfo> result );

        // must return a value <= rawExponent
        protected virtual int StartingExponent( int rawExponent ) => rawExponent - 2;

        protected List<TickInfo> CreateTicks( 
            int exponent,
            int generation,
            params (decimal unitFactor, int minorPerMajor)[] ticks )
        {
            var retVal = new List<TickInfo>();

            for( var curExp = StartingExponent(exponent + generation); curExp <= exponent; curExp++ )
            {
                foreach( var tick in ticks )
                {
                    var curScale = (double) tick.unitFactor * Math.Pow( 10, ( generation + curExp ) );

                    try
                    {
                        var decScale = Convert.ToDecimal( curScale );
                        retVal.Add( new TickInfo( decScale, tick.minorPerMajor ) );
                    }
                    catch
                    {
                    }
                }
            }

            if( !retVal.Any())
                retVal = new List<TickInfo> { new TickInfo(1, 1) };

            return retVal;
        }

        protected abstract decimal GetMinorTicksInRange( TValue minValue, TValue maxValue, decimal minorTickWidth );
        protected abstract bool GetAdjustedEndPoint( TValue toAdjust, decimal minorTickWidth, EndPoint endPoint, out TValue? result );

        bool IRangeCalculator.Calculate( object minValue, 
            object maxValue, 
            out List<object> result )
        {
            result = new List<object>();

            var minType = minValue.GetType();

            if( minType != typeof(TValue) )
            {
                Logger?.Error( "Expected a '{0}' but got a '{1}'", typeof(TValue), minType );
                return false;
            }

            if( minType == maxValue.GetType() )
            {
                if( !Calculate( (TValue) minValue,
                    (TValue) maxValue,
                    out var innerResult ) ) 
                    return false;

                result = innerResult!.Cast<object>().ToList();
                
                return true;
            }

            Logger?.Error( "Minimum ({0}) and maximum ({1}) values are not the same type", minValue, maxValue );

            return false;
        }
    }
}