using System.Diagnostics.CodeAnalysis;

namespace TerminalRacer;

internal class Player
{
    private double kAirResistance = 0.3;
    private double inactiveMotorDeacceleration = 0.15;
    
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
    internal Player(Vector3D position)
    {
        this.position = position;
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
        // (velocity magitude multiplied by how aligned it is with wheels)
        double velocityAlignedWithWheels =
            _linearVelocity.magnitude * Math.Cos(Math.Atan2(_linearVelocity.Z, _linearVelocity.X) - posYaw);
        
        // adds acceleration from active motor if the velocity aligned with wheels is below max speed for gear
        if (_isAccelerating && velocityAlignedWithWheels < GetMaxSpeedForGear() ^ GetMaxSpeedForGear() == 0)
            a += Program.deltaTime * new Vector3D(
                    Math.Cos(posYaw) * GetMotorAccelerationForGear(),
                    0, 
                    Math.Sin(posYaw) * GetMotorAccelerationForGear());
        
        // adds deacceleration from inactive motor
        // exists to stop cars slightly quicker when off gas
        
        
        // adds deacceleration from air resistance
        // primary cause of deacceleration
        a += -1 * kAirResistance * Program.deltaTime *_linearVelocity.magnitude * _linearVelocity;
        
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
                return 4.5;
            case 3:
                return 6;
        }

        throw new Exception("_gear is set to illegal value");
    }

    private double GetMotorAccelerationForGear()
    {
        switch (_gear)
        {
            case 0:
                return -36;
            case 1:
                return 36;
            case 2:
                return 30;
            case 3:
                return 24;
        }
        
        throw new Exception("_gear is set to illegal value");
    }
}
