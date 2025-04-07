namespace TerminalRacer;

internal static class TrackManager
{
    internal static readonly List<TrackInfo> TrackInfoList = [
            new(
                "Standard Motor Speedway",
                30,
                5,
                50,
                """
                .   000>000
                   0       0
                  0         0
                  0         0
                   0       0
                    0000000
                """,
                [',', '.'],
                ['V', 'Y', 'N', 'W', 'M']
            )
        ];
}
