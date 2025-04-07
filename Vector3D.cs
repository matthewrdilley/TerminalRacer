namespace TerminalRacer;

internal class Vector3D(double x, double y, double z)
{
    internal readonly double X = x;
    internal readonly double Y = y;
    internal readonly double Z = z;
    
    public static Vector3D operator -(Vector3D p1, Vector3D p2)
    {
        return new Vector3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
    }
    
    public static Vector3D operator +(Vector3D p1, Vector3D p2)
    {
        return new Vector3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
    }
}