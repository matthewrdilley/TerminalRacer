namespace TerminalRacer;

internal class Wheel
{
    internal double rotationVelocity { get; set; }
    internal readonly WheelIdentifier ID;
    private bool _isSlipping;
    private readonly double _radius; // used to convert angular velocity to tangent (linear) velocity
    private readonly double _angle; // used to determine how directed motion is towards center of mass

    internal Wheel(WheelIdentifier id)
    {
        ID = id;
        _radius = ID is WheelIdentifier.BackRight or WheelIdentifier.BackLeft
            ? Program.selectedCar!.backWheelsRadius
            : Program.selectedCar!.frontWheelsRadius;
        _angle = ID is WheelIdentifier.BackRight or WheelIdentifier.BackLeft
            ? Program.selectedCar.backWheelsAngle
            : Program.selectedCar.frontWheelsAngle;
        if (ID is WheelIdentifier.FrontRight or WheelIdentifier.BackRight)
            _angle *= -1;
    }

    // returns resulting linear delta motion, rotational delta motion, and wheel rotation delta motion
    // linear delta motion is returned assuming the x-axis is equal to the direction of the car
    internal (double linearMagnitude, double linearTheta, double rotational, double wheel) GetFrictionDeltaMotion(
        double carVelocity, double carAngularVelocity, double carRelativeVelocityAngle, double frontWheelRelativeAngle
        ) 
    {
        double wheelRelativeAngle = ID is WheelIdentifier.FrontLeft or WheelIdentifier.FrontRight
            ? frontWheelRelativeAngle
            : 0;
        // gets amount of delta motion that is applied in order to resist motion (apply friction)
        (double magnitude, double angle) resultingDeltaMotion = 
            CalculateAppliedDeltaMotion(carVelocity, carRelativeVelocityAngle, carAngularVelocity, wheelRelativeAngle);
        // applies half of motion that is aligned with wheel to the wheel, all else is returned in accordance
        (double magnitude, double angle, double wheel) motionWithWheel = 
            AccountForWheel(resultingDeltaMotion, wheelRelativeAngle);
        // used only for determining applied linear vs. angular motion magnitudes
        double motionAngleToCenterOfMass = GetOppositeAngle(resultingDeltaMotion.angle - _angle); 
        return (
            GetAppliedLinearDeltaMotion(motionWithWheel.magnitude, motionAngleToCenterOfMass) / 4,
            GetOppositeAngle(_angle),
            GetAppliedAngularDeltaMotion(motionWithWheel.magnitude, motionAngleToCenterOfMass) / 4,
            motionWithWheel.wheel / 4
        );
    }

    private (double magnitude, double angle) CalculateAppliedDeltaMotion(
        double carVelocity, double carRelativeVelocityAngle, double carAngularVelocity, double wheelRelativeAngle
        ) 
    {
        (double magnitude, double angle) overallMotion = 
             GetOverallMotion(carVelocity, carRelativeVelocityAngle, carAngularVelocity, wheelRelativeAngle);
        UpdateIsSlippingStatus(overallMotion.magnitude);
        return (
            GetResultingDeltaMotion(overallMotion.magnitude), 
             overallMotion.angle
             );
    }

    private (double magnitude, double angle) GetOverallMotion(
        double carVelocity, double carVelocityAngle, double carAngularVelocity, double wheelRelativeAngle
    )
    {
        double xMotion = GetXMotion(carVelocity, carVelocityAngle, carAngularVelocity, wheelRelativeAngle);
        double yMotion = GetYMotion(carVelocity, carVelocityAngle, carAngularVelocity, wheelRelativeAngle);
        return (
            Program.Hyp(xMotion, yMotion),
            Math.Atan2(yMotion, xMotion)
        );
    }

    private (double magnitude, double angle, double wheel) AccountForWheel(
        (double magnitude, double angle) deltaMotion, double wheelRelativeAngle
        ) 
    {
        double theta = deltaMotion.angle - wheelRelativeAngle; // so wheel angle is parallel to x-axis
        double x = deltaMotion.magnitude * Math.Cos(theta); // therefore this is all delta motion aligned with wheel
        double y = deltaMotion.magnitude * Math.Sin(theta);
        return (
            Program.Hyp(x / 2, y),
            Math.Atan2(y, x / 2) + wheelRelativeAngle, // reverts to where the car's direction is the x-axis
            x / 2
        );
    }

    private double GetYMotion(
        double carVelocity, double carVelocityAngle, double carAngularVelocity, double wheelRelativeAngle
    ) 
        => carVelocity * Math.Sin(carVelocityAngle) - rotationVelocity * Math.Sin(wheelRelativeAngle)
         + GetAngularVelocityTangentValue(carAngularVelocity) * Math.Sin(GetAngularVelocityTangentAngle());

    private double GetXMotion(
        double carVelocity, double carVelocityAngle, double carAngularVelocity, double wheelRelativeAngle
    )
        => carVelocity * Math.Cos(carVelocityAngle) - rotationVelocity * Math.Cos(wheelRelativeAngle)
           + GetAngularVelocityTangentValue(carAngularVelocity) * Math.Cos(GetAngularVelocityTangentAngle());

    // theta should be how directed motion is towards center of mass; not the car relative angle
    private double GetAppliedAngularDeltaMotion(double magnitude, double theta)
        => magnitude * Math.Sin(theta) / _radius;

    private double GetAppliedLinearDeltaMotion(double magnitude, double theta)
        => magnitude * Math.Cos(theta);

    private void UpdateIsSlippingStatus(double motionMagnitude)
    {
        if (motionMagnitude > Program.selectedCar!.breakawayThreshold)
            _isSlipping = true;
        else if (motionMagnitude <= Program.selectedCar.kineticFrictionMagnitude)
            _isSlipping = false;
    }

    private double GetAngularVelocityTangentAngle()
        => _angle + Math.PI * 0.5;

    private double GetAngularVelocityTangentValue(double carAngularVelocity)
        => carAngularVelocity * _radius;

    private double GetResultingDeltaMotion(double motionMagnitude)
        => _isSlipping ? Program.selectedCar!.kineticFrictionMagnitude * Program.deltaTime : motionMagnitude;

    private double GetOppositeAngle(double angle)
        => (angle + Math.PI * 2) % (Math.PI * 2) - Math.PI;

    internal void ApplyWheelResistance()
    {
        var deltaWheelRotationVelocity = Program.selectedCar!.wheelResistanceMagnitude * Program.deltaTime;
        if (Math.Abs(rotationVelocity) <= deltaWheelRotationVelocity)
            rotationVelocity = 0;
        else if (rotationVelocity > 0)
            rotationVelocity -= deltaWheelRotationVelocity;
        else
            rotationVelocity += deltaWheelRotationVelocity;
    }
}