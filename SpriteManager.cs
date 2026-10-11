namespace TerminalRacer;

internal static class SpriteManager
{
    // Static sprites: placed once, never move (fences, poles, finish line)
    internal static readonly List<SpriteInstance> StaticSprites = new();
    // Dynamic sprites: move/rotate each frame (other cars)
    internal static readonly List<SpriteInstance> DynamicSprites = new();

    // Cached world points for static sprites (rebuilt only when static sprites change)
    private static List<RenderPoint3D>? _cachedStaticPoints;
    private static bool _staticDirty = true;

    // ------------------------------------------------------------------
    // Sprite definitions — add new sprites here
    // ------------------------------------------------------------------

    // Arch that goes over the track near the start line
    // Width=5, Height=5, Depth=2 — matches the 5-wide road
    //
    // Front view (X across, Y up):
    //   Y=4:    ###
    //   Y=3:   #   #
    //   Y=2:   #   #
    //   Y=1:   #   #
    //   Y=0:   #####
    internal static readonly Sprite3D StartArch = BuildStartArch();

    private static Sprite3D BuildStartArch()
    {
        char[,,] texture = new char[5, 5, 2];

        // Floor row (y=0) — full width
        for (int x = 0; x < 5; x++)
        {
            texture[x, 0, 0] = '#';
            texture[x, 0, 1] = '#';
        }

        // Left pillar (x=0)
        for (int y = 1; y <= 3; y++)
        {
            texture[0, y, 0] = '#';
            texture[0, y, 1] = '#';
        }

        // Right pillar (x=4)
        for (int y = 1; y <= 3; y++)
        {
            texture[4, y, 0] = '#';
            texture[4, y, 1] = '#';
        }

        // Arch top (y=4) — center 3 blocks
        for (int x = 1; x <= 3; x++)
        {
            texture[x, 4, 0] = '#';
            texture[x, 4, 1] = '#';
        }

        return new Sprite3D(texture);
    }

    internal static void MarkStaticDirty()
    {
        _staticDirty = true;
    }

    internal static List<RenderPoint3D> GetAllPoints()
    {
        List<RenderPoint3D> allPoints = new();

        // Static sprites (cached — only recomputed when dirty)
        if (_staticDirty || _cachedStaticPoints == null)
        {
            _cachedStaticPoints = new();
            foreach (SpriteInstance sprite in StaticSprites)
            {
                _cachedStaticPoints.AddRange(sprite.GetWorldPoints());
            }
            _staticDirty = false;
        }
        allPoints.AddRange(_cachedStaticPoints!);

        // Dynamic sprites (recomputed every frame)
        foreach (SpriteInstance sprite in DynamicSprites)
        {
            allPoints.AddRange(sprite.GetWorldPoints());
        }

        return allPoints;
    }
}