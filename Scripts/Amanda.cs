using Godot;
using System;

public class Amanda : KinematicBody2D
{

    [Export]
    public Vector2 speed = new Vector2(500, -1000);

    public Vector2 velocity;
    public int proxyDetected = -1;
    public AnimationNodeStateMachinePlayback stateMachine;

    public AmandaState state;
    private String _prevState = "idle";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AnimationTree _animTree = GetNode<AnimationTree>("AnimationTree");
        _animTree.Active = true;
        stateMachine = (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/playback");

        state  =  new AmandaIdle(this); 
    }

    public override void _PhysicsProcess(float delta)
    {
        state = checkState(stateMachine.GetCurrentNode());
        state._statePhysicsProcess(delta);
    }
    public override void _Process(float delta)
    {
        state._stateProcess(delta);
    }

    //Signal recived every time Amanda is "close" to something (floor,walls,etc)
    private void OnSomethingClose(int bodyId, PhysicsBody2D body, int bodyShape, int areaShape) => proxyDetected = areaShape;



    private AmandaState checkState(String currentState)
    {
        if(this._prevState == currentState) return this.state;

        this._prevState = currentState;
        GD.Print(currentState);
        switch(currentState)
        {
            case "idle":
                return new AmandaIdle(this);

            case "start_run":
                return new AmandaStartRun(this);

            case "run_stepR":
            case "run_stepL":
                return new AmandaRun(this);

            case "jump_up_stepR":
            case "jump_up_stepL":
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
    
}


//-----------------------------------------------------------------
//RUN STATE
//-----------------------------------------------------------------
public abstract class AmandaState
{
    protected Amanda amanda;

    public AmandaState(Amanda amanda)
    {
        this.amanda = amanda;
    }

    public abstract void _statePhysicsProcess(float delta);
    public abstract void _stateProcess(float delta);
}


class AmandaRun : AmandaState
{
    private int _speedDir;

    public AmandaRun(Amanda amanda): base(amanda)
    {
        _speedDir = (int) amanda.GetNode<Sprite>("Sprite").Scale.x;
    }

    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x = amanda.speed.x * _speedDir;
        amanda.velocity.y = 10;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {

        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
        else _speedDir = 0;

        if(_speedDir * amanda.GetNode<Sprite>("Sprite").Scale.x < 0)     //this is only true when the player wants to turn
        {
            amanda.stateMachine.Travel("run_turn");
        }
        else if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
        else if (_speedDir == 0)
        {
            amanda.stateMachine.Travel("idle");
        }   
    }
}

//-----------------------------------------------------------------
//START RUN STATE
//-----------------------------------------------------------------

class AmandaStartRun : AmandaState
{
    private int _speedDir;
    private const int _tau = 20;
    private float _time = 0;
    public AmandaStartRun(Amanda amanda): base(amanda)
    {
        _speedDir = (int) amanda.GetNode<Sprite>("Sprite").Scale.x;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.x += amanda.speed.x * (1 - Mathf.Exp(_time / _tau)) * _speedDir; //horizontal velocity follows an inverse exponential model
        amanda.velocity.y = 10;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
        else _speedDir = 0;

        if(_speedDir * amanda.GetNode<Sprite>("Sprite").Scale.x < 0)     //this is only true when the player wants to turn
        {
            amanda.stateMachine.Travel("run_turn");
        }
        else if (Input.IsActionJustPressed("ui_up"))
        {
            amanda.stateMachine.Travel("jump");
        }
        else if (_speedDir == 0)
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}

//-----------------------------------------------------------------
//IDLE STATE
//-----------------------------------------------------------------

class AmandaIdle : AmandaState
{
    public AmandaIdle(Amanda amanda):base(amanda){}
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
            if((int) amanda.GetNode<Sprite>("Sprite").Scale.x > 0)
                amanda.stateMachine.Travel("start_run");
            else
                amanda.stateMachine.Travel("idle_run");
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            if((int) amanda.GetNode<Sprite>("Sprite").Scale.x < 0)
                amanda.stateMachine.Travel("start_run");
            else
                amanda.stateMachine.Travel("idle_run");
        }
    }
}

//-----------------------------------------------------------------
//JUMP UP STATE
//-----------------------------------------------------------------

class AmandaJumpUp : AmandaState
{
    private int _speedDir;
    private float _time = 0;
    private const int _tau = 40;
    private const int _gravity = 1000;
    public AmandaJumpUp(Amanda amanda): base(amanda)
    {
        _speedDir = (int) amanda.GetNode<Sprite>("Sprite").Scale.x;
        amanda.velocity.y = amanda.speed.y;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta;
        amanda.velocity.x += amanda.speed.x * (1 - Mathf.Exp(_time / _tau)) * _speedDir;  //lazy implementation, not taking into account initial velocity
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        //nothing happens
    }
}

//-----------------------------------------------------------------
//JUMP HOLD STATE
//-----------------------------------------------------------------

class AmandaJumpHold : AmandaState
{
    private int _speedDir;
    private float _time = 0;
    private const int _tau = 40;
    private const int _gravity = 1000;
    public AmandaJumpHold(Amanda amanda): base(amanda)
    {
        _speedDir = (int) amanda.GetNode<Sprite>("Sprite").Scale.x;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta;
        amanda.velocity.x += amanda.speed.x * (1 - Mathf.Exp(_time / _tau)) * _speedDir;  //lazy implementation, not taking into account initial velocity
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
        else _speedDir = 0;

        if (amanda.proxyDetected == 0)
        {
            amanda.proxyDetected = -1;
            amanda.stateMachine.Travel("jump_land");
        }
        else if ((Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left")) && amanda.IsOnFloor()) //in case that the proximity detector fails
        {
            amanda.stateMachine.Travel("run_stepR");
        }
        else if(amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}

//-----------------------------------------------------------------
//JUMP LAND STATE
//-----------------------------------------------------------------

class AmandaJumpLand : AmandaState
{
    private int _speedDir;
    private float _time = 0;
    private const int _tau = 40;
    private const int _gravity = 1000;
    public AmandaJumpLand(Amanda amanda): base(amanda)
    {
        _speedDir = (int) amanda.GetNode<Sprite>("Sprite").Scale.x;
    }
    public override void _statePhysicsProcess(float delta)
    {
        amanda.velocity.y += _gravity * delta;
        amanda.velocity.x += amanda.speed.x * (1 - Mathf.Exp(_time / _tau)) * _speedDir;  //lazy implementation, not taking into account initial velocity
        _time += delta;
        amanda.velocity = amanda.MoveAndSlide(amanda.velocity, new Vector2(0, -1));
    }
    public override void _stateProcess(float delta)
    {
        if (Input.IsActionPressed("ui_right")) _speedDir = 1;
        else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
        else _speedDir = 0;

        if ((Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left")) && amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("run_stepR");
        }
        else if (amanda.IsOnFloor())
        {
            amanda.stateMachine.Travel("idle");
        }
    }
}

//-----------------------------------------------------------------
//TURN STATE
//-----------------------------------------------------------------

class AmandaTurn : AmandaState
{
    public AmandaTurn(Amanda amanda): base(amanda)
    {
        amanda.GetNode<Sprite>("Sprite").Scale *= new Vector2(-1,1);
    }
    public override void _statePhysicsProcess(float delta)
    {
        //nothing happens
    }
    public override void _stateProcess(float delta)
    {
        //nothing happens
    }
}
