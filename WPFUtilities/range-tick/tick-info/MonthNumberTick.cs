namespace J4JSoftware.WPFUtilities
{
    public record MonthNumberTick : ScaledTick
    {
        public override double RoundUp(double toRound)
        {
            var scalingFactor = GetNormalizingFactor(toRound, Size);

            var modulo = (toRound * scalingFactor) % (Size * scalingFactor);
            if (modulo == 0)
                return toRound;

            modulo /= scalingFactor;

            // we use (Size - 1) instead of Size because months are not zero-based
            // (i.e., they start with 1, not 0)
            return toRound < 0 ? toRound - modulo : toRound + (Size - 1) - modulo;
        }

        public override double RoundDown(double toRound)
        {
            var scalingFactor = GetNormalizingFactor(toRound, Size);

            var modulo = (toRound * scalingFactor) % (Size * scalingFactor);
            if (modulo == 0)
                return toRound;

            modulo /= scalingFactor;

            // we use (Size - 1) instead of Size because months are not zero-based
            // (i.e., they start with 1, not 0)
            return toRound < 0 ? toRound - (Size - 1) - modulo : toRound - modulo;
        }
    }
}