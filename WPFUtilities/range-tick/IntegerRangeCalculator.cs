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
    public class IntegerRangeCalculator : RangeCalculator<int>
    {
        public IntegerRangeCalculator(
            IJ4JLogger? logger = null
        )
            : base(logger)
        {
        }

        protected override decimal GetMinorTicksInRange( int minValue, int maxValue, decimal minorTickWidth )
        {
            var adjRange = maxValue - minValue;

            return adjRange / minorTickWidth;
        }

        protected override RangeParameters<int> GetDefaultRange(int minValue, int maxValue)
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);

            var majorSize = Math.Pow(10, (int)exponent - 1);

            decimal decMajorSize = 100;

            try
            {
                decMajorSize = Convert.ToDecimal(majorSize);
            }
            catch
            {
            }

            var numMajor = Convert.ToInt32((decimal)range / decMajorSize);
            if (range % majorSize != 0)
                numMajor++;

            return new RangeParameters<int>(
                numMajor,
                10,
                decMajorSize / 10,
                minValue,
                maxValue
            );
        }

        protected override TickStatus GetScalingFactors(
            int generation,
            int minValue,
            int maxValue,
            out List<TickInfo> result )
        {
            generation = generation < 0 ? 0 : generation;
            result = new List<TickInfo> { new TickInfo( 1, 1 ) };

            var range = Math.Abs( maxValue - minValue );

            if( range == 0 )
                return TickStatus.ZeroRange;

            var exponent = (int) Math.Log10( range );

            if( Math.Pow( 10, ( exponent + generation ) ) >= range )
                return TickStatus.RangeExceeded;

            result = ( exponent + generation ) switch
            {
                < 0 => CreateTicks( exponent, generation, ( 1, 10 ), ( (decimal) 2.5, 4 ), ( 5, 2 ) ),
                0 => CreateTicks( exponent, generation, ( 1, 10 ), ( 2, 5 ), ( 5, 2 ) ),
                _ => CreateTicks( exponent, generation, ( 1, 10 ), ( (decimal) 2.5, 4 ), ( 5, 2 ) )
            };

            return TickStatus.Normal;
        }

        public override int RoundUp( int toRound, decimal root )
        {
            var modulo = toRound % root;
            if( modulo == 0 )
                return toRound;

            var intModulo = 0;
            var intMinor = 0;

            try
            {
                intModulo = Convert.ToInt32( modulo );
                intMinor = Convert.ToInt32( root );
            }
            catch
            {
                return toRound;
            }

            return toRound < 0 ? toRound - intModulo : toRound + intMinor - intModulo;
        }

        public override int RoundDown( int toRound, decimal root )
        {
            var modulo = toRound % root;
            if( modulo == 0 )
                return toRound;

            var intModulo = 0;
            var intMinor = 0;

            try
            {
                intModulo = Convert.ToInt32( modulo );
                intMinor = Convert.ToInt32( root );
            }
            catch
            {
                return toRound;
            }

            return toRound < 0 ? toRound - intMinor - intModulo : toRound - intModulo;
        }
    }
}