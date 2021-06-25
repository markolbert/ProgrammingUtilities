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
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public enum EndPointNature
    {
        Inclusive,
        Exclusive
    }

    public abstract partial class RangeCalculator<TValue> : IRangeCalculator<TValue> 
        where TValue : notnull, IComparable<TValue>
    {
        protected RangeCalculator(
            IJ4JLogger? logger = null
        )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public Func<RangeParameters<TValue>, double> RankingFunction { get; set; } = DefaultRankingFunction;
        public EndPointNature StartingPointNature { get; set; } = EndPointNature.Inclusive;
        public EndPointNature EndingPointNature { get; set; } = EndPointNature.Inclusive;

        public bool IsValid => Alternatives.Any() && BestFit != null;
        public List<RangeParameters<TValue>> Alternatives { get; } = new();
        public RangeParameters<TValue>? BestFit { get; private set; }

        public void Evaluate( TValue minValue, TValue maxValue )
        {
            Alternatives.Clear();

            if( minValue.CompareTo( maxValue ) > 0 )
            {
                Logger?.Warning<TValue, TValue>( "Minimum ({0}) and maximum ({1}) values were reversed, correcting",
                    minValue,
                    maxValue );

                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            var generation = 0;
            var tickStatus = TickStatus.Normal;

            while( ( tickStatus = GetScalingFactors( generation, minValue, maxValue, out var ticks ) )
                   != TickStatus.RangeExceeded )
            {
                foreach( var tickInfo in ticks! )
                {
                    var adjMinValue = GetAdjustedEndPoint( minValue, tickInfo.ScalingFactor, EndPoint.StartOfRange );
                    var adjMaxValue = GetAdjustedEndPoint( maxValue, tickInfo.ScalingFactor, EndPoint.EndOfRange );

                    var totalMinorTicks = GetMinorTicksInRange( adjMinValue!,
                        adjMaxValue!,
                        tickInfo.ScalingFactor );

                    var majorTicks = (int) totalMinorTicks / tickInfo.MinorTicksPerMajorTick;

                    var modulo = totalMinorTicks % tickInfo.MinorTicksPerMajorTick;
                    if( modulo != 0 ) majorTicks++;

                    Alternatives.Add( new RangeParameters<TValue>(
                        majorTicks,
                        tickInfo.MinorTicksPerMajorTick,
                        tickInfo.ScalingFactor,
                        adjMinValue!,
                        adjMaxValue! )
                    );
                }

                if( tickStatus == TickStatus.ZeroRange )
                    break;

                generation++;
            }

            if( !Alternatives.Any() )
                Alternatives.Add( GetDefaultRange( minValue, maxValue ) );

            BestFit = Alternatives!.OrderBy(x => RankingFunction(x))
                .First();
        }

        public abstract TValue RoundUp( TValue toRound, decimal root );
        public abstract TValue RoundDown( TValue toRound, decimal root );
        
        protected abstract TickStatus GetScalingFactors( 
            int generation, 
            TValue minValue, 
            TValue maxValue,
            out List<TickInfo> result );

        // must return a value <= rawExponent
        protected virtual int StartingExponent( int rawExponent ) => rawExponent - 2;

        protected List<TickInfo> CreateTicks( 
            int exponent,
            int generation,
            params (decimal unitFactor, int minorPerMajor)[] ticks )
        {
            var retVal = new List<TickInfo>();

            for( var curExp = StartingExponent(exponent + generation); curExp <= exponent; curExp++ )
            {
                foreach( var tick in ticks )
                {
                    var curScale = (double) tick.unitFactor * Math.Pow( 10, ( generation + curExp ) );

                    try
                    {
                        var decScale = Convert.ToDecimal( curScale );
                        retVal.Add( new TickInfo( decScale, tick.minorPerMajor ) );
                    }
                    catch
                    {
                    }
                }
            }

            if( !retVal.Any())
                retVal = new List<TickInfo> { new TickInfo(1, 1) };

            return retVal;
        }

        protected abstract decimal GetMinorTicksInRange( TValue minValue, TValue maxValue, decimal minorTickWidth );

        protected TValue GetAdjustedEndPoint( TValue toAdjust, decimal minorTickWidth, EndPoint endPoint )
        {
            return endPoint switch
            {
                EndPoint.StartOfRange => StartingPointNature switch
                {
                    EndPointNature.Inclusive => RoundDown( toAdjust, minorTickWidth ),
                    EndPointNature.Exclusive => RoundUp( toAdjust, minorTickWidth ),
                    _ => log_error()
                },
                EndPoint.EndOfRange => EndingPointNature switch
                {
                    EndPointNature.Inclusive => RoundUp( toAdjust, minorTickWidth ),
                    EndPointNature.Exclusive => RoundDown( toAdjust, minorTickWidth ),
                    _ => log_error()
                },
                _ => log_error()
            };

            TValue log_error()
            {
                Logger?.Error("Unsupported EndPoint value or EndPointNature");
                return toAdjust;
            }
        }

        protected abstract RangeParameters<TValue> GetDefaultRange( TValue minValue, TValue maxValue );
    }
}