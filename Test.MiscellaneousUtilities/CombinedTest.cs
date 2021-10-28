using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using J4JSoftware.Utilities;
using Xunit;

namespace Test.MiscellaneousUtilities
{
    public class CombinedTest
    {
        private readonly TickRanges _rangers;

        public CombinedTest()
        {
            _rangers = DIContainer.Default.Resolve<TickRanges>();
        }

        [ Theory ]
        [ClassData(typeof(SingleSizeNumbers))]
        public void NumericSingleTickSize( SingleNumbers info )
        {
            _rangers.GetRange<decimal, NumericRange>( info.ControlSize,
                    info.TickSize,
                    info.Minimum,
                    info.Maximum,
                    out var result )
                .Should()
                .BeTrue();

            result.Should().NotBeNull();

            info.RangeStart.CheckValue(result!.RangeStart);
            info.RangeEnd.CheckValue(result.RangeEnd);

            info.MinorTick.CheckValue(result.MinorValue);
            info.MajorTick.CheckValue(result.MajorValue);
        }

        [Theory]
        [ClassData(typeof(RangeSizeNumbers))]
        public void NumericRangeOfTickSizes(RangeOfNumbers info)
        {
            _rangers.GetRange<decimal, NumericRange>(info.ControlSize,
                    info.Minimum,
                    info.Maximum,
                    out var result)
                .Should()
                .BeTrue();

            result.Should().NotBeNull();

            info.RangeStart.CheckValue(result!.RangeStart);
            info.RangeEnd.CheckValue(result.RangeEnd);

            info.MinorTick.CheckValue(result.MinorValue);
            info.MajorTick.CheckValue(result.MajorValue);
        }

        [Theory]
        [ClassData(typeof(SingleSizeDates))]
        public void DateTimeSingleTickSize(SingleDates info)
        {
            _rangers.GetRange<DateTime, MonthRange>(info.ControlSize,
                    info.TickSize,
                    info.Minimum,
                    info.Maximum,
                    out var result,
                    info)
                .Should()
                .BeTrue();

            result.Should().NotBeNull();

            result.Should().NotBeNull();

            result!.RangeStart.Should().Be(info.RangeStart);
            result.RangeEnd.Should().Be(info.RangeEnd);

            result.MinorValue.Should().Be(info.MinorTick);
            result.MajorValue.Should().Be(info.MajorTick);
        }

        [Theory]
        [ClassData(typeof(RangeSizeDates))]
        public void DateTimeRangeOfTickSizes(RangeOfDates info)
        {
            _rangers.GetRange<DateTime, MonthRange>(info.ControlSize,
                    info.Minimum,
                    info.Maximum,
                    out var result,
                    info )
                .Should()
                .BeTrue();

            result.Should().NotBeNull();

            result!.RangeStart.Should().Be(info.RangeStart);
            result.RangeEnd.Should().Be(info.RangeEnd);

            result.MinorValue.Should().Be(info.MinorTick);
            result.MajorValue.Should().Be(info.MajorTick);
        }
    }
}
