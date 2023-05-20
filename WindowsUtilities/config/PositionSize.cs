using Windows.Graphics;

namespace J4JSoftware.WindowsUtilities;

public record PositionSize(int UpperLeftX, int UpperLeftY, int Height, int Width)
{
    public static PositionSize Empty { get; } = new PositionSize(0, 0, 0, 0);

    public static implicit operator RectInt32(PositionSize size) =>
        new (size.UpperLeftX, size.UpperLeftY, size.Width, size.Height);
}