namespace TerminalRacer;

internal static class CarManager
{
    internal static readonly List<CarStats> CarStatsList =
    [
        new("Standard Car", 0.15, 0.1 * Math.PI,
            0.5, 0.3, 0.1, 0.3,
            2, 2, Math.PI * 0.85, Math.PI * 0.15
            )
    ];
}