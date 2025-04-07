namespace TerminalRacer;

internal record CarStats(
    string name,
    double airResistanceCoefficient,
    double steeringCoefficient,
    double breakawayThreshold,
    double kineticFrictionMagnitude,
    double wheelResistanceMagnitude,
    double accelerationPerGear,
    double backWheelsRadius,
    double frontWheelsRadius,
    double backWheelsAngle,
    double frontWheelsAngle
);