using FluentAssertions;
using J4JSoftware.Utilities;
using Xunit;

namespace Test.MiscellaneousUtilities
{
    public class DateTest
    {
        [ Theory ]
        [ ClassData( typeof( SingleSizeDates ) ) ]
        public void SingleTickSize( SingleDates info )
        {
            var ranger = new MonthlyTickRange();
            ranger.Configure( info );

            ranger.GetRange( info.ControlSize,
                            info.TickSize,
                            info.Minimum,
                            info.Maximum,
                            out var result )
                  .Should()
                  .BeTrue();

            result.Should().NotBeNull();

            result!.RangeStart.Should().Be( info.RangeStart );
            result.RangeEnd.Should().Be( info.RangeEnd );

            result.MinorValue.Should().Be( info.MinorTick );
            result.MajorValue.Should().Be( info.MajorTick );
        }

        [ Theory ]
        [ ClassData( typeof( RangeSizeDates ) ) ]
        public void RangeOfTickSizes( RangeOfDates info )
        {
            var ranger = new MonthlyTickRange();
            ranger.Configure( info );

            ranger.GetRange( info.ControlSize,
                            info.Minimum,
                            info.Maximum,
                            out var result )
                  .Should()
                  .BeTrue();

            result.Should().NotBeNull();

            result!.RangeStart.Should().Be( info.RangeStart );
            result.RangeEnd.Should().Be( info.RangeEnd );

            result.MinorValue.Should().Be( info.MinorTick );
            result.MajorValue.Should().Be( info.MajorTick );
        }
    }
}
