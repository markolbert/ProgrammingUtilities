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
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public class DecimalRangeCalculator : RangeCalculator<decimal>
    {
        public DecimalRangeCalculator(
            IJ4JLogger? logger = null
        )
            : base(logger)
        {
        }

        protected override decimal GetMinorTicksInRange( decimal minValue, decimal maxValue, decimal minorTickWidth )
        {
            var adjRange = maxValue - minValue;

            return adjRange == 0 ? 1 : adjRange / minorTickWidth;
        }

        protected override RangeParameters<decimal> GetDefaultRange( decimal minValue, decimal maxValue )
        {
            var range = (double) Math.Abs(  maxValue - minValue );
            var exponent = Math.Log10( range );

            var majorSize = Math.Pow( 10, (int) exponent - 1 );

            decimal decMajorSize = 100;

            try
            {
                decMajorSize = Convert.ToDecimal( majorSize );
            }
            catch
            {
            }

            var numMajor = Convert.ToInt32( (decimal) range / decMajorSize );
            if( range % majorSize != 0 )
                numMajor++;

            return new RangeParameters<decimal>(
                numMajor,
                10,
                decMajorSize / 10,
                minValue,
                maxValue
            );
        }

        protected override TickStatus GetScalingFactors(
            int generation,
            decimal minValue,
            decimal maxValue,
            out List<TickInfo> result )
        {
            result = new List<TickInfo> { new TickInfo( 1, 1 ) };

            generation = generation < 0 ? 0 : generation;

            var range = Math.Abs( maxValue - minValue );

            if( range == 0 )
                return TickStatus.ZeroRange;

            var exponent = (int) Math.Log10( (double) range );

            if( (decimal) Math.Pow( 10, ( exponent + generation ) ) >= range )
                return TickStatus.RangeExceeded;

            result = ( exponent + generation ) switch
            {
                < 0 => CreateTicks( exponent, generation, ( 1, 10 ), ( (decimal) 2.5, 4 ), ( 5, 2 ) ),
                0 => CreateTicks( exponent, generation, ( 1, 10 ), ( 2, 5 ), ( 5, 2 ) ),
                _ => CreateTicks( exponent, generation, ( 1, 10 ), ( (decimal) 2.5, 4 ), ( 5, 2 ) )
            };

            return TickStatus.Normal;
        }

        public override decimal RoundUp(decimal toRound, decimal root )
        {
            var modulo = toRound % root;
            if (modulo == 0)
                return toRound;

            return toRound < 0 ? toRound - modulo : toRound + root - modulo;
        }

        public override decimal RoundDown(decimal toRound, decimal root)
        {
            var modulo = toRound % root;
            if (modulo == 0)
                return toRound;

            return toRound < 0 ? toRound - root - modulo : toRound - modulo;
        }
    }
}