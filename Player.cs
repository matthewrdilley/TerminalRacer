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
    private double _linearVelocity; 
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
        _linearVelocity = 0;
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
        
        Console.WriteLine("linear velocity: " + Math.Floor(_linearVelocity*100)/100);
        Console.WriteLine("angular velocity: " + Math.Floor(_angularVelocityYaw*100)/100);
    }

    private void UpdatePosition()
    {
        position += Program.deltaTime * new Vector3D(_linearVelocity * Math.Cos(-posYaw), 0, _linearVelocity * Math.Sin(-posYaw));
        posYaw += Program.deltaTime * _angularVelocityYaw;
    }

    private void UpdateVelocity()
    {
        double oldLinearVelocity = _linearVelocity;
        _linearVelocity += Program.deltaTime * GetLinearForce() / _carStats.kMass;
        if (Math.Sign(oldLinearVelocity) == Math.Sign(_linearVelocity) * -1)
            _linearVelocity = 0;
        
        _angularVelocityYaw = GetAngularVelocity();
    }
    
    private double GetLinearForce()
    {
        double f = 0;
        
        // adds deacceleration from air resistance (F=-kv^2)
        f -= _carStats.kAirResistance * _linearVelocity * Math.Abs(_linearVelocity);
        
        // adds deacceleration from turning (F=-kv^2)
        f -= Math.Sign(_linearVelocity) * _carStats.kMass * _carStats.kTurningDrag 
             * _angularVelocityYaw * _angularVelocityYaw;
        
        // adds acceleration from the motor (F=c {v<max})
        double maxSpeed = GetMaxSpeedForGear();
        if (_isAccelerating && IsBelowMaxSpeed(maxSpeed, _linearVelocity))
        {
            f += GetMotorAccelerationForGear();
            if (!IsBelowMaxSpeed(maxSpeed, _linearVelocity + f*Program.deltaTime))
            {
                // sets to this value to avoid overshooting the max speed for the gear
                f = (maxSpeed - _linearVelocity) / Program.deltaTime;
            }
        }
        
        return f;
    }

    private double GetAngularVelocity()
    {
        return _steeringWheelRotation * _carStats.kTurningAbility * _linearVelocity / _carStats.kMass;
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
                return 3.5;
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
                return -11;
            case 1:
                return 12;
            case 2:
                return 10;
            case 3:
                return 8;
        }
        
        throw new Exception("_gear is set to illegal value");
    }
    
    private bool IsBelowMaxSpeed(double maxSpeed, double currentSpeed)
    {
        return maxSpeed > 0 ? maxSpeed > currentSpeed : maxSpeed < currentSpeed;
    }
}
