namespace TerminalRacer;

internal class Vector3D(double x, double y, double z)
{
    internal readonly double X = x;
    internal readonly double Y = y;
    internal readonly double Z = z;
    
    public double magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
    
    public Vector3D unit => magnitude > 0
        ? new Vector3D(X / magnitude, Y / magnitude, Z / magnitude)
        : new Vector3D(0, 0, 0);
    
    public static Vector3D operator -(Vector3D p1, Vector3D p2)
    {
        return new Vector3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
    }
    
    public static Vector3D operator +(Vector3D p1, Vector3D p2)
    {
        return new Vector3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
    }

    public static Vector3D operator *(double scalar, Vector3D p)
    {
        return new Vector3D(scalar * p.X, scalar * p.Y, scalar * p.Z);
    }

    public Vector3D RotateAboutOrigin(
        double yaw, double pitch, double roll
    ) {
        Matrix yawMatrix = new Matrix();
        yawMatrix.Set(0, 0, Math.Cos(yaw));
        yawMatrix.Set(2, 0, Math.Sin(yaw));
        yawMatrix.Set(0, 2, -Math.Sin(yaw));
        yawMatrix.Set(2, 2, Math.Cos(yaw));

        Matrix pitchMatrix = new Matrix();
        pitchMatrix.Set(1, 1, Math.Cos(pitch));
        pitchMatrix.Set(2, 1, -Math.Sin(pitch));
        pitchMatrix.Set(1, 2, Math.Sin(pitch));
        pitchMatrix.Set(2, 2, Math.Cos(pitch));

        Matrix rollMatrix = new Matrix();
        rollMatrix.Set(0, 0, Math.Cos(roll));
        rollMatrix.Set(1, 0, -Math.Sin(roll));
        rollMatrix.Set(0, 1, Math.Sin(roll));
        rollMatrix.Set(1, 1, Math.Cos(roll));

        return this * yawMatrix * pitchMatrix * rollMatrix;
    }
}