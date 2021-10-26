using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Utilities;
using Xunit;
using Range = J4JSoftware.Utilities.Range;

namespace Test.MiscellaneousUtilities
{
    public class NumericTest
    {
        [ Theory ]
        [ ClassData( typeof(SingleSizeData) ) ]
        public void SingleTickSize( SingleTick info )
        {
            var ranger = new NumericTickRange();

            ranger.GetRange( info.ControlSize,
                    info.TickSize,
                    info.Minimum,
                    info.Maximum,
                    out var result )
                .Should()
                .BeTrue();

            result.Should().NotBeNull();

            info.RangeStart.CheckValue( result!.RangeStart );
            info.RangeEnd.CheckValue( result.RangeEnd );

            info.MinorTick.CheckValue( result.MinorValue );
            info.MajorTick.CheckValue( result.MajorValue );
        }

        [Theory]
        [ClassData(typeof(RangeSizeData))]
        public void RangeOfTickSizes(RangeOfTick info)
        {
            var ranger = new NumericTickRange();

            ranger.GetRange(info.ControlSize,
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
    }
}
