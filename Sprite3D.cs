namespace TerminalRacer;

internal class Sprite3D
{
    internal readonly char[,,] Texture;
    internal readonly int Width;
    internal readonly int Height;
    internal readonly int Depth;

    internal Sprite3D(char[,,] texture)
    {
        Texture = texture;
        Width = texture.GetLength(0);
        Height = texture.GetLength(1);
        Depth = texture.GetLength(2);
    }

    internal List<(int X, int Y, int Z, char Symbol)> GetVoxels()
    {
        List<(int X, int Y, int Z, char Symbol)> voxels = new();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    char symbol = Texture[x, y, z];
                    if (symbol != '\0')
                    {
                        voxels.Add((x, y, z, symbol));
                    }
                }
            }
        }
        return voxels;
    }
}