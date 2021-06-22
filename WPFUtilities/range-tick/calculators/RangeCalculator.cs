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
using System.Runtime.InteropServices;
using System.Windows.Xps.Serialization;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public abstract class RangeCalculator<TValue> : IRangeCalculator<TValue> 
        where TValue : IComparable<TValue>
    {
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
            int minTickPowerOfTen, 
            MinorTickInfo[] tickChoices,
            out List<RangeParametersNG<TValue>>? result )
        {
            result = null;

            if( minValue.CompareTo(maxValue) > 0  )
            {
                Logger?.Warning<TValue, TValue>( "Minimum ({0}) and maximum ({1}) values were reversed, correcting",
                    minValue, 
                    maxValue );

                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            var powerOfTen = GetPowerOfTen( minValue, maxValue, minTickPowerOfTen );

            var retVal = new List<RangeParametersNG<TValue>>();

            foreach (var mtChoice in tickChoices)
            {
                if( !GetAdjustedEndPoint( minValue, 
                    mtChoice.NormalizedTickWidth * powerOfTen, 
                    EndPoint.StartOfRange,
                    out var adjMinValue ) )
                    return false;

                if( !GetAdjustedEndPoint( maxValue, 
                    mtChoice.NormalizedTickWidth * powerOfTen, 
                    EndPoint.EndOfRange,
                    out var adjMaxValue ) )
                    return false;

                var totalMinorTicks = GetMinorTicksInRange( adjMinValue!, adjMaxValue!, mtChoice.NormalizedTickWidth * powerOfTen );

                var majorTicks = (int)totalMinorTicks / mtChoice.MinorTicksPerMajorTick;

                var modulo = totalMinorTicks % mtChoice.MinorTicksPerMajorTick;
                if (modulo != 0) majorTicks++;

                retVal.Add( new RangeParametersNG<TValue>(
                    majorTicks,
                    mtChoice.MinorTicksPerMajorTick,
                    mtChoice.NormalizedTickWidth * powerOfTen,
                    adjMinValue!,
                    adjMaxValue! )
                );
            }

            result = retVal;

            return true;
        }

        protected abstract decimal GetMinorTicksInRange( TValue minValue, TValue maxValue, decimal minorTickWidth );
        protected abstract decimal GetPowerOfTen( TValue minValue, TValue maxValue, int minTickPowerOfTen );
        protected abstract bool GetAdjustedEndPoint( TValue toAdjust, decimal minorTickWidth, EndPoint endPoint, out TValue? result );

        bool IRangeCalculator.Calculate( object minValue, 
            object maxValue, 
            int minTickPowerOfTen,
            MinorTickInfo[] tickChoices,
            out List<object>? result )
        {
            result = null;

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
                    minTickPowerOfTen,
                    tickChoices,
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