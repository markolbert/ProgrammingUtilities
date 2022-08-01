namespace J4JSoftware.VisualUtilities;

public record CartesianCenter
(
    float CartesianCenterX,
    float CartesianCenterY
);

public record WindowsCartesianContext(
    float CartesianCenterX,
    float CartesianCenterY,
    float WindowsWidth,
    float WindowsHeight
) : CartesianCenter( CartesianCenterX, CartesianCenterY )
{
    public WindowsCartesianContext(
        float windowsWidth,
        float windowsHeight
    )
        : this( 0f, 0f, windowsWidth, windowsHeight )
    {
    }
}
