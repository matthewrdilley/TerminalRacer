namespace TerminalRacer;

using System.Diagnostics;

internal static class Program
{
    private const uint FramesPerSecond = 30;
    private const uint TargetFrameTimeDeltaMs = 1000 / FramesPerSecond;
    
    private const uint ViewDisplayWidth = 90;
    private const uint ViewDisplayHeight = 30;
    private const double ViewPlaneWidth = 32;
    private const double ViewPlaneHeight = 16 ;
    private const double ViewPlaneZ = 24;
    private const double RenderDistance = 50;
    
    private static readonly char[,] View = new char[ViewDisplayWidth, ViewDisplayHeight];
    
    internal static List<ConsoleKey> pressedKeys { get; private set; } = new();
    internal static float deltaTime { get; private set; }
    internal static CarStats? selectedCar { get; private set; }
    internal static int frameNum { get; private set;  }
    internal static string frameContent { get; set; } = string.Empty;
    
    private static List<RenderPoint3D>? _space;
    private static bool _hasOpponents;
    private static TrackInfo? _selectedTrack;
    private static bool _isInRace;
    private static bool _hasSelectedMode;
    private static char[,]? _blownUpTrack;
    private static List<RenderPoint3D>? _justTrackPoints;
    private static Player? _player;
    
    private static void Main(string[] args)
    {
        if (args.Length != 0)
            Console.WriteLine("?");
        Console.CursorVisible = false;
        while (!pressedKeys.Contains(ConsoleKey.Q))
        {
            var stopWatch = Stopwatch.StartNew(); 
            frameNum++;
            pressedKeys = new(); 
            while (Console.KeyAvailable) 
            { 
                pressedKeys.Add(Console.ReadKey(true).Key);
            }
            frameContent = string.Empty;
            Update();
            Console.Clear();
            Console.Write(frameContent);
            stopWatch.Stop();
            int timeToSleepMs = (int) (TargetFrameTimeDeltaMs - stopWatch.ElapsedMilliseconds);
            if (timeToSleepMs > 0)
            {
                Thread.Sleep(timeToSleepMs);
                deltaTime = TargetFrameTimeDeltaMs / 1000f;
            }
            else
            {
                deltaTime = stopWatch.ElapsedMilliseconds / 1000f;
            }
            if (deltaTime > 1)
                deltaTime = 1;
        }
    }

    private static void Update()
    {
        frameContent += $"FPS: {(uint)(1f / deltaTime)}\n";
        if (_isInRace)
        {
            UpdateRace();
        }
        else if (selectedCar != null)
        {
            UpdateTrackSelectionMenu();    
        }
        else if (_hasSelectedMode)
        {
            UpdateCarSelectionMenu();     
        }
        else
        {
            UpdateStartMenu();
        }
    }

    private static void AddSeparator()
    {
        for (int i = 0; i < ViewDisplayWidth; i++)
        {
            frameContent += '-';
        }
        frameContent += "\n";
    }

    private static void UpdateRace()
    {
        _blownUpTrack ??= _selectedTrack!.GetBlownUp();
        _justTrackPoints ??= GenerateTrackPoints();
        _space = _justTrackPoints.ToList();
        
        _player!.Update();
        
        MapPointsOntoView(GetPerspectiveMappedPoints());
        
        AddSeparator();
        AddView();
        AddSeparator();
    }

    private static void UpdateStartMenu()
    {
        frameContent += "\n\n";
        frameContent += "     1    Time Trail\n";
        frameContent += "     2    Single-Player Race\n";
        frameContent += "     q    Quit\n";
        if (pressedKeys.Contains(ConsoleKey.D1))
        {
            _hasSelectedMode = true; // to move to select track menu
        }
        else if (pressedKeys.Contains(ConsoleKey.D2))
        {
            _hasOpponents = true;
            _hasSelectedMode = true;
        }
    }

    private static void UpdateTrackSelectionMenu()
    {
        frameContent += "\n\nSelect a Track\n\n";
        int i = 0;
        foreach (TrackInfo trackInfo in TrackManager.TrackInfoList)
        {
            i++;
            frameContent += $"    {i}     {trackInfo.Title}\n";
            if (pressedKeys.Contains(GetConsoleKeyFromInt(i)))
            {
                _selectedTrack = trackInfo;
                StartRace();
            }
        }
    }

    private static void UpdateCarSelectionMenu()
    {
        frameContent += "\n\nSelect a Car\n\n";
        int i = 0;
        foreach (CarStats car in CarManager.CarStatsList)
        {
            i++;
            frameContent += $"    {i}     {car.name}\n";
            if (pressedKeys.Contains(GetConsoleKeyFromInt(i)))
            {
                selectedCar = car;
            }
        }
    }

    private static void StartRace()
    {
        _isInRace = true;
        _player = new Player(_selectedTrack!.GetPlayerStartingPosition(), selectedCar!);
    }

    private static void AddView()
    {
        for (int j = 0; j < View.GetLength(1); j++)
        {
            for (int i = 0; i < View.GetLength(0); i++)
            {
                frameContent += View[i, j].ToString();
            }
            frameContent += "\n";
        }
    }

    private static ConsoleKey GetConsoleKeyFromInt(int x)
    {
        switch (x)
        {
            default: return ConsoleKey.D0;
            case 1: return ConsoleKey.D1;
            case 2: return ConsoleKey.D2;
            case 3: return ConsoleKey.D3;
            case 4: return ConsoleKey.D4;
            case 5: return ConsoleKey.D5;
            case 6: return ConsoleKey.D6;
            case 7: return ConsoleKey.D7;
            case 8: return ConsoleKey.D8;
            case 9: return ConsoleKey.D9;
        }
    }

    private static List<RenderPoint3D> GenerateTrackPoints()
    {
        List<RenderPoint3D> pointList = new();
        for (int x = 0; x < _blownUpTrack!.GetLength(0); x++)
        {
            for (int z = 0; z < _blownUpTrack!.GetLength(1); z++)
            {
                pointList.Add(new(x, 0, z, _blownUpTrack[x, z]));
            }
        }

        return pointList;
    }

    private static List<RenderPoint3D> GetPerspectiveMappedPoints()
    {
        List<RenderPoint3D> results = new();
        foreach (RenderPoint3D point in _space!)
        {
            if (
                Math.Sqrt(
                    Square(point.X-_player!.position.X)
                    + Square(point.Y-_player.position.Y)
                    + Square(point.Z-_player.position.Z)
                ) < RenderDistance
                )
                results.Add(MapUsingPerspectiveView(TransformWithPovToOrigin(point))); 
        }

        return results;
    }

    private static RenderPoint3D MapUsingPerspectiveView(RenderPoint3D p)
    {
        double slopeX = p.X / p.Z;
        double slopeY = p.Y / p.Z;
        double depth = Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2));
        return new(
            slopeX * ViewPlaneZ,
            slopeY * ViewPlaneZ,
            p.Z > 0 ? depth : -depth,
            p.Symbol
            );
    }

    // translates and rotates a point so that it's as if the POV was transformed to the origin and set at an angle of 0 degrees
    // while ensuring the old space is congruent to the new space
    private static RenderPoint3D TransformWithPovToOrigin(RenderPoint3D p)
    {
        Vector3D translatedPoint = p - _player!.position;
        Vector3D rotatedPoint = translatedPoint.RotateAboutOrigin(-_player.posYaw-Math.PI/2, -_player.posPitch, -_player.posRoll);
        return new(rotatedPoint.X, rotatedPoint.Y, rotatedPoint.Z, p.Symbol);
    }

    private static void MapPointsOntoView(List<RenderPoint3D> points)
    {
        RenderPoint3D?[,] mappedPoints = new RenderPoint3D[ViewDisplayWidth, ViewDisplayHeight];
        foreach (RenderPoint3D p in points)
        {
            int x = (int) Math.Round(p.X / ViewPlaneWidth * ViewDisplayWidth) + (int) ViewDisplayWidth / 2;
            int y = (int) -Math.Round(p.Y / ViewPlaneHeight * ViewDisplayHeight) + (int) ViewDisplayHeight / 2;
            if (x < ViewDisplayWidth && y < ViewDisplayHeight && x >= 0 && y >= 0 && p.Z > 0 
                && (mappedPoints[x, y] == null || mappedPoints[x, y]!.Z > p.Z))
            {
                mappedPoints[x, y] = p; 
            }
        }

        double minZ = double.MaxValue;
        double maxZ = double.MinValue;
        foreach (RenderPoint3D? p in mappedPoints)
        {
            if (p != null)
            {
                if (p.Z < minZ)
                    minZ = p.Z;
                if (p.Z > maxZ)
                    maxZ = p.Z;
            }
        }
        
        for (int x = 0; x < ViewDisplayWidth; x++)
        {
            for (int y = 0; y < ViewDisplayHeight; y++)
            {
                if (mappedPoints[x, y] == null)
                {
                    View[x, y] = ' ';
                }
                else
                {
                    double normalizedDistance = (mappedPoints[x, y]!.Z - minZ) / (maxZ - minZ); 
                    if (minZ.Equals(maxZ))
                        normalizedDistance = 1;
                    char[] chars = mappedPoints[x, y]!.Symbol == TrackInfo.RoadChar ? _selectedTrack!.RoadTexture : _selectedTrack!.TurfTexture;
                    int charIndex = (int) Math.Round(normalizedDistance * (chars.Length-1));
                    View[x, y] = chars[charIndex];
                }
            }
        } 
    }

    private static double Square(double x) => x * x;
}
