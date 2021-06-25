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
    public class DoubleRangeCalculator : RangeCalculator<double>
    {
        public DoubleRangeCalculator(
            IJ4JLogger? logger = null
        )
            : base(logger)
        {
        }

        protected override decimal GetMinorTicksInRange(double minValue, double maxValue, decimal minorTickWidth)
        {
            var rawRange = maxValue - minValue;

            if( rawRange == 0 )
                return 1;

            decimal range = 0;

            try
            {
                range = Convert.ToDecimal( rawRange );
            }
            catch
            {
                Logger?.Fatal("Range ({0}) too large to convert to decimal", rawRange);
                throw new OverflowException( $"Range ({rawRange}) too large to convert to decimal" );
            }

            return range / minorTickWidth;
        }

        protected override RangeParameters<double> GetDefaultRange(double minValue, double maxValue)
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

            return new RangeParameters<double>(
                numMajor,
                10,
                decMajorSize / 10,
                minValue,
                maxValue
            );
        }

        protected override TickStatus GetScalingFactors(
            int generation,
            double minValue,
            double maxValue,
            out List<TickInfo> result)
        {
            generation = generation < 0 ? 0 : generation;
            result = new List<TickInfo> { new TickInfo( 1, 1 ) };

            var range = Math.Abs(maxValue - minValue);

            if (range == 0)
                return TickStatus.ZeroRange;

            var exponent = (int)Math.Log10((double)range);

            if (Math.Pow(10, (exponent + generation)) >= range)
                return TickStatus.RangeExceeded;

            result = (exponent + generation) switch
            {
                < 0 => CreateTicks(exponent, generation, (1, 10), ((decimal)2.5, 4), (5, 2)),
                0 => CreateTicks(exponent, generation, (1, 10), (2, 5), (5, 2)),
                _ => CreateTicks(exponent, generation, (1, 10), ((decimal)2.5, 4), (5, 2))
            };

            return TickStatus.Normal;
        }

        public override double RoundUp( double toRound, decimal root )
        {
            var modulo = toRound % (double) root;
            if( modulo == 0 )
                return toRound;

            return toRound < 0 ? toRound - modulo : toRound + (double) root - modulo;
        }

        public override double RoundDown( double toRound, decimal root )
        {
            var modulo = toRound % (double) root;
            if( modulo == 0 )
                return toRound;

            return toRound < 0 ? toRound - (double) root - modulo : toRound - modulo;
        }
    }
}