using System.Diagnostics.CodeAnalysis;

namespace TerminalRacer;

internal class Player 
{
    private const int MinGear = -1;
    private const int MaxGear = 3;
    private const int MinWheelRotation = -3;
    private const int MaxWheelRotation = 3;

    // directly controlled by keyboard input
    private bool _isBraking;
    private int _gear = 1;
    private int _steeringWheelRotation;

    private double _gas = 1;
    private double _speed; // linear velocity magnitude
    private double _linearVelocityAngle = 0; // in radians, represents the direction of the velocity
    private double _angularVelocity = 0; // in radians, rate at which PosYaw changes
    internal required Vector3D position { get; set; }

    // in radians, represents the rotation of the vehicle and of the camera
    internal double posPitch => 0;
    internal double posRoll => 0;
    internal double posYaw { get; private set; } = 0;
    
    private readonly Wheel[] _wheels =
    [
      new(WheelIdentifier.BackLeft),
      new(WheelIdentifier.BackRight),
      new(WheelIdentifier.FrontLeft),
      new(WheelIdentifier.FrontRight)
    ];

    [SetsRequiredMembers]
    internal Player(Vector3D position)
    {
        this.position = position;
    }
    
    internal void Update()
    {
        UpdateControls();
        UpdateMovement();
    }

    private void UpdateControls()
    {
        _isBraking = Program.pressedKeys.Contains(ConsoleKey.Spacebar);
        if (Program.pressedKeys.Contains(ConsoleKey.UpArrow) && _gear < MaxGear)
        {
            _gear++;
        }

        if (Program.pressedKeys.Contains(ConsoleKey.DownArrow) && _gear > MinGear)
        {
            _gear--;
        }

        if (Program.pressedKeys.Contains(ConsoleKey.RightArrow) && _steeringWheelRotation < MaxWheelRotation)
        {
            _steeringWheelRotation++;
        }

        if (Program.pressedKeys.Contains(ConsoleKey.LeftArrow) && _steeringWheelRotation > MinWheelRotation)
        {
            _steeringWheelRotation--;
        }
    }
    
    private void UpdateMovement()
    {
        ControlBackWheels();
        ApplyForceOfFriction();
        //ApplyForceOfAirResistance();
        //posYaw += _angularVelocity * Program.deltaTime;
        position += new Vector3D(
            Math.Cos(_linearVelocityAngle) * _speed * Program.deltaTime,
            0,
            Math.Sin(_linearVelocityAngle) * _speed * Program.deltaTime
            );
    }
    
    private void ControlBackWheels()
    {
        foreach (var wheel in _wheels)
        {
            if (wheel.ID is WheelIdentifier.BackLeft or WheelIdentifier.BackRight)
            {
                if (_isBraking)
                    wheel.rotationVelocity = 0;
                else
                    wheel.rotationVelocity += Program.selectedCar!.accelerationPerGear * _gear * Program.deltaTime;
            }
        }
    }

    private void ApplyForceOfAirResistance()
    { 
        //_speed *= GetActualAirResistanceCoefficient();
        // _angularVelocity *= GetActualAirResistanceCoefficient();
    }

    private double GetActualAirResistanceCoefficient()
        => 1 - Program.selectedCar!.airResistanceCoefficient * Program.deltaTime;
    
    private void ApplyForceOfFriction()
    {
        var x = _speed * Math.Sin(_linearVelocityAngle);
        var y = _speed * Math.Cos(_linearVelocityAngle); 
        var relativeVelocityAngle = GetRelativeVelocityAngle();
        var frontWheelRelativeAngle = GetFrontWheelRelativeRotation();
        var angularVelocity = _angularVelocity; // so that the used value doesn't change between each wheel
        
        foreach (var wheel in _wheels)
        {
            // friction of axel to wheel
            wheel.ApplyWheelResistance();
            // friction of ground to wheel and vice versa
            var deltaMotion = wheel.GetFrictionDeltaMotion(
                _speed, angularVelocity, relativeVelocityAngle, frontWheelRelativeAngle
                );
            var theta = deltaMotion.linearTheta + posYaw;
            x += deltaMotion.linearMagnitude * Math.Cos(theta);
            y += deltaMotion.linearMagnitude * Math.Sin(theta);
            //_angularVelocity += deltaMotion.rotational;
            wheel.rotationVelocity += deltaMotion.wheel;
        }
        _speed = Program.Hyp(x, y);
       // _linearVelocityAngle = Math.Atan2(y, x); 
    }

    private double GetFrontWheelRelativeRotation()
        => _steeringWheelRotation * -Program.selectedCar!.steeringCoefficient; 
    
    private double GetRelativeVelocityAngle()
        => _linearVelocityAngle - posYaw;
}
