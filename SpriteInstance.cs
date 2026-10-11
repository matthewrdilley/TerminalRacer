namespace TerminalRacer;

internal class SpriteInstance
{
    internal readonly Sprite3D Sprite;
    internal Vector3D Position;
    internal double Yaw;
    internal double Pitch;
    internal double Roll;

    internal SpriteInstance(Sprite3D sprite, Vector3D position, double yaw = 0, double pitch = 0, double roll = 0)
    {
        Sprite = sprite;
        Position = position;
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
    }

    internal List<RenderPoint3D> GetWorldPoints()
    {
        List<RenderPoint3D> points = new();
        double centerX = (Sprite.Width - 1) / 2.0;
        double centerZ = (Sprite.Depth - 1) / 2.0;

        foreach ((int vx, int vy, int vz, char symbol) in Sprite.GetVoxels())
        {
            // Center on X/Z, bottom-anchor on Y so Position is the base
            Vector3D localOffset = new Vector3D(vx - centerX, vy, vz - centerZ);
            Vector3D worldOffset = localOffset.RotateAboutOrigin(Yaw, Pitch, Roll);
            Vector3D worldPos = Position + worldOffset;
            points.Add(new RenderPoint3D(worldPos.X, worldPos.Y, worldPos.Z, symbol, true));
        }

        return points;
    }
}