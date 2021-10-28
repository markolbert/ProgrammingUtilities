using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Utilities;
using Xunit;

namespace Test.MiscellaneousUtilities
{
    public class NumericTest
    {
        [ Theory ]
        [ ClassData( typeof(SingleSizeNumbers) ) ]
        public void SingleTickSize( SingleNumbers info )
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
        [ClassData(typeof(RangeSizeNumbers))]
        public void RangeOfTickSizes(RangeOfNumbers info)
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
