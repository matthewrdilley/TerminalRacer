namespace TerminalRacer;

internal class RenderPoint3D(double x, double y, double z, char symbol, bool isSprite) : Vector3D(x, y, z)
{
    internal readonly char Symbol = symbol;
    internal readonly bool IsSprite = isSprite;
}