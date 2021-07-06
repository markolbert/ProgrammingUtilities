using System;

namespace J4JSoftware.WPFUtilities
{
    public record ScaledTick : Tick
    {
        protected ScaledTick()
        {
        }

        public int PowerOfTen { get; init; }

        public override double Size => base.Size * Math.Pow( 10, PowerOfTen );
    }
}