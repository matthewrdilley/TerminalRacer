using System.Diagnostics.CodeAnalysis;

namespace TerminalRacer;

internal class Player
{
    private readonly CarStats _carStats;
    
    private const int MinGear = 0;
    private const int MaxGear = 3;
    private const int MinWheelRotation = -3;
    private const int MaxWheelRotation = 3;

    // directly controlled by keyboard input
    private bool _isBraking;
    private bool _isAccelerating;
    private int _gear = 1;
    private int _steeringWheelRotation;

    private double _gas = 1;
    private Vector3D _linearVelocity; 
    private double _angularVelocityYaw; // in radians per second, rate at which posYaw changes
    internal required Vector3D position { get; set; }

    // in radians, represents the rotation of the vehicle and of the camera
    internal double posPitch => 0;
    internal double posRoll => 0;
    internal double posYaw { get; private set; }

    [SetsRequiredMembers]
    internal Player(Vector3D position, CarStats carStats)
    {
        this.position = position;
        _carStats = carStats;
        _linearVelocity = new Vector3D(0, 0, 0);
    }
    
    internal void Update()
    {
        UpdateControls();
        UpdateMovement();
    }

    private void UpdateControls()
    {
        _isBraking = Program.pressedKeys.Contains(ConsoleKey.B);
        _isAccelerating = Program.pressedKeys.Contains(ConsoleKey.A);
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
        UpdateVelocity();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        position += Program.deltaTime * _linearVelocity;
        posYaw += Program.deltaTime * _angularVelocityYaw;
    }

    private void UpdateVelocity()
    {
        _linearVelocity += Program.deltaTime * GetLinearAcceleration();
        _angularVelocityYaw += Program.deltaTime * GetAngularAcceleration();
        
        Console.WriteLine(_linearVelocity.magnitude);
    }
    
    private Vector3D GetLinearAcceleration()
    {
        Vector3D a = new Vector3D(0,0,0);

        // dot product between velocity and wheel direction
        // (velocity magnitude multiplied by how aligned it is with wheels)
        double velocityAlignedWithWheels =
            _linearVelocity.magnitude * Math.Cos(Math.Atan2(_linearVelocity.Z, _linearVelocity.X) - posYaw);

        double maxSpeed = GetMaxSpeedForGear();
        double motorAcceleration = GetMotorAccelerationForGear();

        // the motor pushes until the car reaches the gear's max speed;
        // for reverse gear the max speed is negative, so the car still has speed to gain
        // while it is not yet moving fast enough in reverse
        bool canAccelerate = maxSpeed > 0
            ? velocityAlignedWithWheels < maxSpeed
            : velocityAlignedWithWheels > maxSpeed;

        if (_isAccelerating && canAccelerate)
        {
            // scale the motor force by the remaining speed so the car settles at max speed
            // instead of overshooting and oscillating around it
            double p = 1 - velocityAlignedWithWheels / maxSpeed;
            a += new Vector3D(
                Math.Cos(posYaw) * motorAcceleration * p,
                0,
                Math.Sin(posYaw) * motorAcceleration * p);
        }

        // adds deacceleration from air resistance
        a += -1 * _carStats.kAirResistance * _linearVelocity.magnitude * _linearVelocity;
        
        return a;
    }

    private double GetAngularAcceleration()
    {
        return 0;
    }

    private double GetMaxSpeedForGear()
    {
        switch (_gear)
        {
            case 0:
                return -1.5;
            case 1:
                return 2.5;
            case 2:
                return 3.25;
            case 3:
                return 5;
        }

        throw new Exception("_gear is set to illegal value");
    }

    private double GetMotorAccelerationForGear()
    {
        switch (_gear)
        {
            case 0:
                return -30;
            case 1:
                return 35;
            case 2:
                return 25;
            case 3:
                return 15;
        }
        
        throw new Exception("_gear is set to illegal value");
    }
}
