using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using J4JSoftware.Utilities;
using Xunit;

namespace Test.MiscellaneousUtilities
{
    public class LinealMonthTests
    {
        [ Theory ]
        [ InlineData( "1/15/2021", "1/2021", "1/1/2021", "1/31/2021", "1/1/2021", "3/31/2021", "1/1/2021",
            "12/31/2021" ) ]
        [ InlineData( "2/15/2020", "2/2020", "2/1/2020", "2/29/2020", "1/1/2020", "3/31/2020", "1/1/2020",
            "12/31/2020" ) ]
        [ InlineData( "12/31/1992", "12/1992", "12/1/1992", "12/31/1992", "10/1/1992", "12/31/1992", "1/1/1992",
            "12/31/1992" ) ]
        public void Basic(
            string dateText,
            string lmText,
            string monthStart,
            string monthEnd,
            string qtrStart,
            string qtrEnd,
            string yearStart,
            string yearEnd )
        {
            var linealMonth = new LinealMonth( DateTime.Parse( dateText ) );

            linealMonth.ToString().Should().Be( lmText );
            linealMonth.Date.Should().Be( DateTime.Parse( monthStart ) );
            linealMonth.EndOfMonth.Should().Be( DateTime.Parse( monthEnd ) );
            linealMonth.StartOfQuarter.Should().Be( DateTime.Parse( qtrStart ) );
            linealMonth.EndOfQuarter.Should().Be( DateTime.Parse( qtrEnd ) );
            linealMonth.StartOfYear.Should().Be( DateTime.Parse( yearStart ) );
            linealMonth.EndOfYear.Should().Be( DateTime.Parse( yearEnd ) );
        }

        [Theory]
        [InlineData("7/31/2021", -9, "10/1/2020")]
        [InlineData("3/31/2021", -5, "10/1/2020")]
        [InlineData("1/15/2021",0,"1/1/2021")]
        [InlineData("3/31/2021", 5, "8/1/2021")]
        [InlineData("7/31/2021", 9, "4/1/2022")]
        public void AddMonths( string startingDate, int monthsToAdd, string endingDate )
        {
            var linealMonth = new LinealMonth( DateTime.Parse( startingDate ) );

            ( linealMonth + monthsToAdd ).Date.Should().Be( DateTime.Parse( endingDate ) );
        }

        [Theory]
        [InlineData("7/31/2021", -9, "4/1/2022")]
        [InlineData("3/31/2021", -5, "8/1/2021")]
        [InlineData("1/15/2021", 0, "1/1/2021")]
        [InlineData("3/31/2021", 5, "10/1/2020")]
        [InlineData("7/31/2021", 9, "10/1/2020")]
        public void SubtractMonths(string startingDate, int monthsToAdd, string endingDate)
        {
            var linealMonth = new LinealMonth(DateTime.Parse(startingDate));

            (linealMonth - monthsToAdd).Date.Should().Be(DateTime.Parse(endingDate));
        }

        [Theory]
        [InlineData("2/15/2021", 0, "2/1/2021")]
        [InlineData("2/15/2021", 5, "7/1/2021")]
        [InlineData("2/15/2021", -5, "9/1/2020")]
        public void MonthNumber( string startingDate, int monthsToAdd, string endingDate )
        {
            var startDate = DateTime.Parse( startingDate );
            var linealMonth = new LinealMonth( startDate );

            linealMonth.MonthNumber.Should().Be( LinealMonth.ToMonthNumber( startDate ) );

            LinealMonth.DateFromMonthNumber( linealMonth.MonthNumber + monthsToAdd )
                .Should()
                .Be( DateTime.Parse( endingDate ) );
        }

        [Theory]
        [InlineData("2/15/2021")]
        [InlineData("7/15/1936")]
        public void MonthNumberConstructor( string dateText )
        {
            var date = DateTime.Parse( dateText );

            var mnDate = new LinealMonth( date );
            var numDate = new LinealMonth( mnDate.MonthNumber );

            numDate.Date.Should().Be( date.AddDays( 1 - date.Day ) );
        }
    }
}
