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
    public class MonthNumberTicks : RangeTicks<MonthNumberTick>
    {
        private readonly List<Tick> _monthlyTicks = new List<Tick>
        {
            new Tick{NormalizedSize=1, NumberPerMajor = 12},
            new Tick{NormalizedSize=3, NumberPerMajor = 4},
            new Tick{NormalizedSize=6, NumberPerMajor = 2},
            new Tick{NormalizedSize=12, NumberPerMajor = 1},
        };

        public MonthNumberTicks(
            int maxMonthlyYears = 10,
            IEnumerable<Tick>? yearTicks = null
        )
        {
            MaxMonthlyYears = maxMonthlyYears <= 0 ? 10 : maxMonthlyYears;

            NormalizedRangeTicks = yearTicks?.ToList() ?? new List<Tick>
            {
                new Tick{NormalizedSize = 1, NumberPerMajor = 10},
                new Tick{NormalizedSize = 2, NumberPerMajor = 5},
                new Tick{NormalizedSize = 5, NumberPerMajor = 2},
                new Tick{NormalizedSize = 25, NumberPerMajor = 4},
            };

            Default = NormalizedRangeTicks[ 0 ];
        }

        public int MaxMonthlyYears { get; }

        public override IEnumerable<MonthNumberTick> GetEnumerator( double minValue, double maxValue )
        {
            // for up to MaxMonthlyYears years of months we only work with the month tick sizes
            maxValue = maxValue <= 12 ? 12 : maxValue;
            minValue = minValue <= 12 ? 12 : minValue;

            var years = (int) Math.Ceiling( ( maxValue - minValue ) / 12 );

            if( years <= MaxMonthlyYears )
            {
                foreach( var monthlyTick in _monthlyTicks )
                {
                    yield return new MonthNumberTick
                    {
                        NormalizedSize = monthlyTick.NormalizedSize,
                        PowerOfTen=0,
                        NumberPerMajor = monthlyTick.NumberPerMajor
                    };
                }
            }
            else
            {
                foreach( var baseMT in base.GetEnumerator( minValue / 12, maxValue / 12 )
                    .Where( x => x.PowerOfTen >= 0 ) )
                {
                    yield return new MonthNumberTick
                    {
                        NormalizedSize = baseMT.NormalizedSize * 12,
                        PowerOfTen = baseMT.PowerOfTen,
                        NumberPerMajor = baseMT.NumberPerMajor
                    };
                }
            }
        }

        public override RangeParameters GetDefaultRange(double minValue, double maxValue)
        {
            var range = Math.Abs(maxValue - minValue);
            var exponent = Math.Log10(range);
            var minorTick = new DoubleTick
            {
                NormalizedSize = 1,
                PowerOfTen = (int)exponent - 1,
                NumberPerMajor = 12
            };

            var majorSize = Math.Pow(10, (int)exponent - 1);

            var numMajor = Convert.ToUInt32(range / majorSize);
            if (range % majorSize != 0)
                numMajor++;

            var rangeStart = minorTick.RoundDown(minValue);
            var rangeEnd = minorTick.RoundUp(maxValue);

            return new RangeParameters(
                numMajor,
                10,
                minorTick.Size,
                rangeStart,
                rangeEnd,
                Math.Abs(rangeStart - minValue),
                Math.Abs(rangeEnd - maxValue));
        }
    }
}