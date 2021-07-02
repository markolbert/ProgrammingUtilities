using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Utilities
{
    public partial class LinealMonth
    {
        public LinealMonth( DateTime dt )
        {
            Date = dt.Date.AddDays( 1 - dt.Day );
            EndOfMonth = Date.AddMonths( 1 ).AddDays( -1 );
            StartOfQuarter = Date.AddMonths( 3 * ( ( Date.Month - 1 ) / 3 ) - Date.Month + 1);
            EndOfQuarter = StartOfQuarter.AddMonths( 3 ).AddDays( -1 );
            StartOfYear = StartOfQuarter.AddMonths( 1 - StartOfQuarter.Month );
            EndOfYear = StartOfYear.AddYears( 1 ).AddDays( -1 );
        }

        public LinealMonth( int monthNum )
            : this( new DateTime( monthNum / 12, monthNum - 12 * ( (int) monthNum / 12 ) + 1, 1 ) )
        {
        }

        public DateTime Date { get; }
        public DateTime EndOfMonth { get; }
        public DateTime StartOfQuarter { get; }
        public DateTime EndOfQuarter { get; }
        public DateTime StartOfYear { get; }
        public DateTime EndOfYear { get; }

        public int Year => Date.Year;
        public int Month => Date.Month;
        public int MonthNumber => 12 * Year + Month - 1;

        public string ToString( bool padMonth ) => Date.ToString( padMonth ? "MM/yyyy":"M/yyyy" );
        public override string ToString() => ToString( false );
    }
}
