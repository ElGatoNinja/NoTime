using Godot;
using System;

public class Amanda : KinematicBody2D
{

    public Vector2 speed = new Vector2(600, -1900);

    public Vector2 velocity;
    public int proxyDetected = -1;

    public int[] amandaGravities = { 5000, 2500, 2000 };
    public AnimationNodeStateMachinePlayback stateMachine;
    private AnimationTree _animTree;

    public AmandaState state;
    private String _prevState = "idle";
    public float timeScale = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animTree = GetNode<AnimationTree>("AnimationTree");
        _animTree.Active = true;
        stateMachine = (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/StateMachine/playback");
        state = new AmandaIdle(this);
    }

    public override void _PhysicsProcess(float delta)
    {
        state = checkState(stateMachine.GetCurrentNode());
        state._statePhysicsProcess(delta);
        if (Input.IsActionJustPressed("ui_accept"))
        {
            if (timeScale == 1)
                ChangeTime(5);
            else
                ChangeTime(1);
        }
    }
    public override void _Process(float delta)
    {
        state._stateProcess(delta);
        proxyDetected = -1;
    }

    //Signal recived every time Amanda is "close" to something (floor,walls,etc)
    private void OnSomethingClose(int bodyId, PhysicsBody2D body, int bodyShape, int areaShape)
    {
        proxyDetected = areaShape;
    }



    private AmandaState checkState(String currentState)
    {
        if (this._prevState == currentState) return this.state;

        this._prevState = currentState;
        GD.Print(currentState);
        switch (currentState)
        {
            case "idle":
                return new AmandaIdle(this);

            case "start_run":
                return new AmandaStartRun(this);

            case "stop_run":
                return new AmandaStopRun(this);

            case "run_stepR":
            case "run_stepL":
                return new AmandaRun(this);

            case "jump_up_stepR":
            case "jump_up_stepL":
            case "jump_up_vert":
                return new AmandaJumpUp(this);

            case "jump":
                return new AmandaJumpHold(this);

            case "jump_land":
                return new AmandaJumpLand(this);

            case "run_turn":
            case "idle_turn":
            default:
                return new AmandaTurn(this);
        }
    }

    private void ChangeTime(float scale)
    {
        _animTree.Set("parameters/TimeScale/scale", scale);
        timeScale = scale;
        Engine.TimeScale = 1 / scale;
    }

}


public abstract class AmandaState
{
    protected Amanda amanda;
    protected int _speedDir;
    public string debugInfo = "";

    public AmandaState(Amanda amanda)
    {
        this.amanda = amanda;
        _speedDir = (int)amanda.GetNode<Sprite>("Sprite").Scale.x;
    }

    public abstract void _statePhysicsProcess(float delta);
    public abstract void _stateProcess(float delta);

}

//-----------------------------------------------------------------
//RUN STATE
//-----------------------------------------------------------------
#region RUN STATE

class AmandaRun : AmandaState
{

    public AmandaRun(Amanda amanda) : base(amanda)
    {
        _speedDir = (int)amanda.GetNode<Sprite>("Sprite").Scale.x;
    }

    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x = amanda.speed.x * _speedDir * amanda.timeScale;
        amanda.velocity.y = 10;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {

        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;


        if (_speedDir * amanda.GetNode<Sprite>("Sprite").Scale.x < 0)     //this is only true when the player wants to turn
        {
            amanda.stateMachine.Travel("run_turn");
        }
        else if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
        else if (!Input.IsActionPressed("ui_right") && !Input.IsActionPressed("ui_left"))
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}
#endregion

//-----------------------------------------------------------------
//START_RUN STATE
//-----------------------------------------------------------------
#region START_RUN STATE
class AmandaStartRun : AmandaState
{
    private const float _tau = 4;
    private float _time = 0;

    public AmandaStartRun(Amanda amanda) : base(amanda)
    {

    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x = amanda.speed.x * amanda.timeScale * (1 - Mathf.Exp(-_time * amanda.timeScale * _tau)) * _speedDir; //horizontal velocity follows an inverse exponential model
        amanda.velocity.y = 10;
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
        else _speedDir = 0;

        if (_speedDir * amanda.GetNode<Sprite>("Sprite").Scale.x < 0)     //this is only true when the player wants to turn
        {
            amanda.stateMachine.Travel("run_turn");
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
        else if (_speedDir == 0)
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}
#endregion

//-----------------------------------------------------------------
//STOP_RUN STATE
//-----------------------------------------------------------------
#region STOP_RUN STATE

class AmandaStopRun : AmandaState
{
    private float _time = 0;
    private const float _tau = 2;
    private float _initVel;
    public AmandaStopRun(Amanda amanda) : base(amanda)
    {
        _initVel = amanda.velocity.x;
        _speedDir = -_speedDir;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x = (_initVel) * Mathf.Exp(-_time * amanda.timeScale * _tau); //horizontal velocity follows an inverse exponential model
        amanda.velocity.y = 10;
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
    }
}

#endregion

//-----------------------------------------------------------------
//IDLE STATE
//-----------------------------------------------------------------
#region IDLE STATE
class AmandaIdle : AmandaState
{
    public AmandaIdle(Amanda amanda) : base(amanda)
    {
        _speedDir = 0;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x = 0;
        amanda.velocity.y = 10;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            if ((int)amanda.GetNode<Sprite>("Sprite").Scale.x > 0)
                amanda.stateMachine.Travel("start_run");
            else
                amanda.stateMachine.Travel("idle_turn");
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            if ((int)amanda.GetNode<Sprite>("Sprite").Scale.x < 0)
                amanda.stateMachine.Travel("start_run");
            else
                amanda.stateMachine.Travel("idle_turn");
        }
    }
}
#endregion

//-----------------------------------------------------------------
//JUMP UP STATE
//-----------------------------------------------------------------
#region JUMP UP STATE
class AmandaJumpUp : AmandaState
{
    private float _time = 0;
    private const int _tau = 40;
    private int _gravity = 1000;
    public AmandaJumpUp(Amanda amanda) : base(amanda)
    {
        amanda.velocity.y = amanda.speed.y * amanda.timeScale;
        _gravity = amanda.amandaGravities[0]; //<--------------------------debug

        if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right"))
            amanda.velocity.x = amanda.speed.x * _speedDir * amanda.timeScale;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta * amanda.timeScale * amanda.timeScale;
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));

        debugInfo = "speedDir: " + Convert.ToString(_speedDir);
    }
    public override void _stateProcess(float delta)
    {
        //nothing yet
    }
}
#endregion

//-----------------------------------------------------------------
//JUMP HOLD STATE
//-----------------------------------------------------------------
#region JUMP HOLD STATE
class AmandaJumpHold : AmandaState
{
    private float _time = 0;
    private const float _tau = 0.2f;
    private int _gravity = 1000;
    private float _initVel;
    public AmandaJumpHold(Amanda amanda) : base(amanda)
    {
        _initVel = amanda.velocity.x;

        _gravity = amanda.amandaGravities[1];  //<-------------------DEBUG
        if (_initVel < 10) _speedDir = 0;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta * amanda.timeScale * amanda.timeScale;
        amanda.velocity.x = amanda.speed.x * amanda.timeScale * _speedDir + (_initVel - amanda.speed.x* amanda.timeScale * _speedDir) * Mathf.Exp(-_time * amanda.timeScale * _tau);  //inverse exponential aproach
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));

        debugInfo = "dir: " + _speedDir;
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_right") && _speedDir <= 0)
        {
            _speedDir = 1;
            _initVel = amanda.velocity.x;
        }
        else if (Input.IsActionJustPressed("ui_left") && _speedDir >= 0)
        {
            _speedDir = -1;
            _initVel = amanda.velocity.x;
        }
        else if (!Input.IsActionPressed("ui_right") && !Input.IsActionPressed("ui_left"))
        {
            _speedDir = 0;
            _initVel = amanda.velocity.x;
        }


        if (amanda.proxyDetected == 0)
        {
            amanda.stateMachine.Travel("jump_land");
        }
        else if(amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("jump_land");   
        }
    }
}
#endregion 

//-----------------------------------------------------------------
//JUMP LAND STATE
//-----------------------------------------------------------------
#region JUMP LAND STATE
class AmandaJumpLand : AmandaState
{
    private float _time = 0;
    private const float _tau = 0.2f;
    private int _gravity = 1000;
    private float _initVel;
    public AmandaJumpLand(Amanda amanda) : base(amanda)
    {
        _initVel = amanda.velocity.x;
        _gravity = amanda.amandaGravities[2];
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta * amanda.timeScale * amanda.timeScale;
        amanda.velocity.x = amanda.speed.x * amanda.timeScale * _speedDir + (_initVel - amanda.speed.x * amanda.timeScale * _speedDir) * Mathf.Exp(-_time * _tau);  //inverse exponential aproach  //lazy implementation, not taking into account initial velocity
        _time += delta * amanda.timeScale;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));


    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_right") && _speedDir <= 0)
        {
            _speedDir = 1;
            _initVel = amanda.velocity.x;
        }
        else if (Input.IsActionJustPressed("ui_left") && _speedDir >= 0)
        {
            _speedDir = -1;
            _initVel = amanda.velocity.x;
        }
        else if (!Input.IsActionPressed("ui_right") && !Input.IsActionPressed("ui_left"))
        {
            _speedDir = 0;
            _initVel = amanda.velocity.x;
        }

        if (Input.IsActionPressed("ui_up"))
            amanda.stateMachine.Travel("jump");

        else if ((Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left")) && amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("run_stepR");
        }
        else if (amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}
#endregion

//-----------------------------------------------------------------
//RUN_TURN STATE
//-----------------------------------------------------------------
#region RUN_TURN STATE
class AmandaTurn : AmandaState
{
    public AmandaTurn(Amanda amanda) : base(amanda)
    {
        amanda.GetNode<Sprite>("Sprite").Scale *= new Vector2(-1, 1);
    }
    public override void _statePhysicsProcess(float delta)
    {

    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
    }

}
#endregion