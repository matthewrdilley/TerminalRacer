namespace TerminalRacer;

internal class RenderPoint3D(double x, double y, double z, char symbol) : Vector3D(x, y, z)
{
    internal readonly char Symbol = symbol;
}