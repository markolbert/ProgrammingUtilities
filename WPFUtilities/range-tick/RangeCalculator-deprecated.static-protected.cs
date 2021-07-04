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

namespace J4JSoftware.WPFUtilities
{
    public abstract partial class RangeCalculator<TValue>
    {
        public static double DefaultRankingFunction( RangeParameters<TValue> rangeParameters )
        {
            var majors = Math.Pow( 2, Math.Abs( rangeParameters.MajorTicks - 10 ) );

            var fiveMinors = majors * Math.Pow( 2, Math.Abs( rangeParameters.MinorTicksPerMajorTick - 5 ) );
            var tenMinors = majors * Math.Pow( 2, Math.Abs( rangeParameters.MinorTicksPerMajorTick - 10 ) );

            return fiveMinors < tenMinors ? fiveMinors : tenMinors;
        }

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
    }
}