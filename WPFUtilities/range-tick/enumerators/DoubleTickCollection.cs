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
#pragma warning disable 8618

namespace J4JSoftware.WPFUtilities
{
    public class DoubleTickCollection : MinorTickCollection<ScaledTick>
    {
        public DoubleTickCollection(
            IEnumerable<ScaledTick>? baseMultiples = null
        )
        {
            NormalizedRangeTicks = baseMultiples?.ToList() ?? new List<ScaledTick>
            {
                new ScaledTick { NormalizedSize = 1, NumberPerMajor = 10 },
                new ScaledTick { NormalizedSize = 2, NumberPerMajor = 5 },
                new ScaledTick { NormalizedSize = 5, NumberPerMajor = 2 },
                new ScaledTick { NormalizedSize = 25, NumberPerMajor = 4 },
            };

            Default = NormalizedRangeTicks[ 0 ];
        }

        public override RangeParameters GetDefaultRangeParameters( double minValue, double maxValue )
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);
            var minorTick = new ScaledTick
            {
                NormalizedSize = 1,
                PowerOfTen = (int)exponent - 1,
                NumberPerMajor = 10
            };

            var rangeStart = minorTick.RoundDown(minValue);
            var rangeEnd = minorTick.RoundUp(maxValue);

            return new RangeParameters(
                minorTick,
                rangeStart,
                rangeEnd,
                Math.Abs(rangeStart - minValue),
                Math.Abs(rangeEnd - maxValue));
        }
    }
}