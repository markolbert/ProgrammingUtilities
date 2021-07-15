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

namespace J4JSoftware.WPFUtilities
{
    public class MonthNumberTickCollection : MinorTickCollection<MonthNumberTick>
    {
        private readonly List<MonthNumberTick> _monthlyTicks = new List<MonthNumberTick>
        {
            new MonthNumberTick { NormalizedSize = 1, NumberPerMajor = 12 },
            new MonthNumberTick { NormalizedSize = 3, NumberPerMajor = 4 },
            new MonthNumberTick { NormalizedSize = 6, NumberPerMajor = 2 },
            new MonthNumberTick { NormalizedSize = 12, NumberPerMajor = 1 },
        };

        public MonthNumberTickCollection(
            IEnumerable<MonthNumberTick>? yearTicks = null
        )
        {
            NormalizedRangeTicks = yearTicks?.ToList() ?? new List<MonthNumberTick>
            {
                new MonthNumberTick{NormalizedSize = 1, NumberPerMajor = 10},
                new MonthNumberTick{NormalizedSize = 2, NumberPerMajor = 5},
                new MonthNumberTick{NormalizedSize = 5, NumberPerMajor = 2},
                new MonthNumberTick{NormalizedSize = 25, NumberPerMajor = 4},
            };

            Default = NormalizedRangeTicks[ 0 ];
        }

        public override List<MonthNumberTick> GetAlternatives( double minValue, double maxValue )
        {
            var retVal = new List<MonthNumberTick>();

            maxValue = maxValue <= 12 ? 12 : maxValue;
            minValue = minValue <= 12 ? 12 : minValue;

            retVal.AddRange( _monthlyTicks.Select( x => new MonthNumberTick
            {
                NormalizedSize = x.NormalizedSize,
                PowerOfTen = 0,
                NumberPerMajor = x.NumberPerMajor
            } ) );

            retVal.AddRange(base.GetAlternatives(minValue / 12, maxValue / 12)
                .Where(x => x.PowerOfTen >= 0).Select(x=>new MonthNumberTick
                {
                    NormalizedSize = x.NormalizedSize * 12,
                    PowerOfTen = x.PowerOfTen,
                    NumberPerMajor = x.NumberPerMajor
                }));

            return retVal;
        }

        public override RangeParameters GetDefaultRangeParameters(double minValue, double maxValue)
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);
            var minorTick = new MonthNumberTick
            {
                NormalizedSize = 1,
                PowerOfTen = (int)exponent - 1,
                NumberPerMajor = 12
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