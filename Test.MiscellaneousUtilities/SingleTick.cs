#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.MiscellaneousUtilities' is free software: you can redistribute it
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

namespace Test.MiscellaneousUtilities
{
    public class RangeOfNumbers
    {
        public int ControlSize { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public RoundedDecimal MinorTick { get; set; } = new(0);
        public RoundedDecimal MajorTick { get; set; } = new(0);
        public RoundedDecimal RangeStart { get; set; } = new(0);
        public RoundedDecimal RangeEnd { get; set; } = new(0);
    }

    public class SingleNumbers : RangeOfNumbers
    {
        public int TickSize { get; set; }
    }

    public class RangeOfDates
    {
        public bool TraditionalMonthsPerMinorOnly { get; set; }
        public int ControlSize { get; set; }
        public DateTime Minimum { get; set; }
        public DateTime Maximum { get; set; }
        public int MinorTick { get; set; }
        public int MajorTick { get; set; }
        public DateTime RangeStart { get; set; }
        public DateTime RangeEnd { get; set; }
    }

    public class SingleDates : RangeOfDates
    {
        public int TickSize { get; set; }
    }
}