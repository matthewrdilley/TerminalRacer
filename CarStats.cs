namespace TerminalRacer;

internal record CarStats(
    string name,
    double kAirResistance,
    double kTurningAbility,
    double kTurningDrag,
    double kMass
);