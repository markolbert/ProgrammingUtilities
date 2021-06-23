using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.WPFUtilities
{
    public class MonthRangeRounder : IRangeRounder<DateTime>
    {
        private readonly decimal _minorTickWidth;

        public MonthRangeRounder(
            RangeParameters<DateTime> rangeParameters
        )
        {
            _minorTickWidth = rangeParameters.MinorTickWidth;
        }

        public DateTime RoundUp(DateTime toRound)
        {
            var monthNum = MonthRangeCalculator.GetMonthNumber( toRound );

            var modulo = monthNum % _minorTickWidth;
            if( modulo == 0 )
                return MonthRangeCalculator.GetFirstDay( toRound, 1 );

            return MonthRangeCalculator.GetFirstDay(toRound, _minorTickWidth - modulo + 1);
        }

        public DateTime RoundDown(DateTime toRound)
        {
            var monthNum = MonthRangeCalculator.GetMonthNumber(toRound);

            var modulo = monthNum % _minorTickWidth;
            if (modulo == 0)
                return MonthRangeCalculator.GetFirstDay(toRound, 0);

            return MonthRangeCalculator.GetFirstDay(toRound, -modulo);
        }
    }
}
