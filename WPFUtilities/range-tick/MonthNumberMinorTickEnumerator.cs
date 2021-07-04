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
    public class MonthNumberMinorTickEnumerator : DoubleMinorTickEnumerator
    {
        private readonly List<MinorTick> _monthlyTicks = new List<MinorTick>
        {
            new MinorTick( 1, 12 ),
            new MinorTick( 3, 4 ),
            new MinorTick( 6, 2 ),
            new MinorTick( 12, 1 ),
        };

        public MonthNumberMinorTickEnumerator(
            int maxMonthlyYears = 10,
            IEnumerable<MinorTick>? yearTicks = null
        )
        {
            MaxMonthlyYears = maxMonthlyYears <= 0 ? 10 : maxMonthlyYears;

            MinorTicks = yearTicks?.ToList() ?? new List<MinorTick>
            {
                new MinorTick( 1, 10 ),
                new MinorTick( 2, 5 ),
                new MinorTick( 5, 2 ),
                new MinorTick( 25, 4 ),
            };

            Default = MinorTicks[ 0 ];
        }

        public int MaxMonthlyYears { get; }
        public override bool UpperLimitIsInclusive => true;

        public override IEnumerable<ScaledMinorTick> GetEnumerator( double minValue, double maxValue )
        {
            // for up to MaxMonthlyYears years of months we only work with the month tick sizes
            maxValue = maxValue <= 12 ? 12 : maxValue;
            minValue = minValue <= 12 ? 12 : minValue;

            var years = (int) Math.Ceiling( ( maxValue - minValue ) / 12 );

            if( years <= MaxMonthlyYears )
            {
                foreach( var monthlyTick in _monthlyTicks )
                {
                    yield return new ScaledMinorTick( monthlyTick.NormalizedSize,
                        0,
                        monthlyTick.NumberPerMajor );
                }
            }
            else
            {
                foreach( var baseMT in base.GetEnumerator( minValue / 12, maxValue / 12 )
                    .Where( x => x.PowerOfTen >= 0 ) )
                {
                    yield return new ScaledMinorTick( baseMT.NormalizedSize * 12,
                        baseMT.PowerOfTen,
                        baseMT.NumberPerMajor );
                }
            }
        }
    }
}