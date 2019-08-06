using Godot;
using System;

public class Amanda : KinematicBody2D
{

    [Export]
    public Vector2 speed = new Vector2(0, 0);
    [Export]
    public int[] amandaGravities = { 100, 100, 100 };
    [Export]
    public float velocityDelayX = 100;
    private float accTime = 0;

    private Vector2 _velocity = Vector2.Zero;
    public int _speedDir = 1;
    public int _proximityDetected = -1;

    private AnimationTree _animTree;
    public AnimationNodeStateMachinePlayback _stateMachine;
    private String _prevState;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animTree = GetNode<AnimationTree>("AnimationTree");
        _animTree.Active = true;
        _stateMachine = (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/playback");

    }

    public override void _PhysicsProcess(float delta)
    {
        StateTransition(_prevState, _stateMachine.GetCurrentNode());
        _prevState = _stateMachine.GetCurrentNode();
        switch (_stateMachine.GetCurrentNode())
        {
            case "idle":
                _velocity = Vector2.Zero;
                break;

            case "start_run":
                //accTime += delta;
                //_velocity.x += speed.x * (1 - Mathf.Exp(accTime / velocityDelayX)) * _speedDir; //horizontal velocity follows an inverse exponential model
                _velocity.x = speed.x * _speedDir;
                _velocity.y = 10;
                break;

            case "run_stepR":
            case "run_stepL":

                _velocity.x = speed.x * _speedDir;
                _velocity.y = 10;
                break;

            case "jump_up_stepR":
            case "jump_up_stepL":
                //diferent gravities for each jump stage
                _velocity.y += amandaGravities[0] * delta;
                break;

            case "jump":
            case "jump_land":
                _velocity.y += amandaGravities[2] * delta;
                _velocity.x += speed.x * (1 - Mathf.Exp(accTime / velocityDelayX)) * _speedDir;
                break;

        }

        _velocity = MoveAndSlide(_velocity, new Vector2(0, -1));

    }
    public override void _Process(float delta)
    {

        switch (_stateMachine.GetCurrentNode())
        {
            case "idle":
                if (Input.IsActionJustPressed("ui_up"))
                {
                    _stateMachine.Travel("jump");
                }
                else if (Input.IsActionPressed("ui_right"))
                {
                    _speedDir = 1;
                    _stateMachine.Travel("start_run");
                }
                else if (Input.IsActionPressed("ui_left"))
                {
                    _speedDir = -1;
                    _stateMachine.Travel("start_run");
                }
                break;

            case "start_run":  //differs in physics
            case "run_stepR":
            case "run_stepL":
                if (Input.IsActionPressed("ui_right")) _speedDir = 1;
                else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
                else _speedDir = 0;

                if(_speedDir * GetNode<Sprite>("Sprite").Scale.x < 0)     //this is only true when the player wants to turn
                {
                    _stateMachine.Travel("run_turn");
                }
                else if (Input.IsActionJustPressed("ui_up"))
                {
                    _stateMachine.Travel("jump");
                }
                else if (_speedDir == 0)
                {
                    _stateMachine.Travel("idle");
                }
                break;

            case "jump_up_stepR":
            case "jump_up_stepL":
                break;

            case "jump":
                if (Input.IsActionPressed("ui_right")) _speedDir = 1;
                else if (Input.IsActionPressed("ui_left")) _speedDir = -1;
                else _speedDir = 0;

                if (_proximityDetected == 0)
                {
                    _proximityDetected = -1;
                    _stateMachine.Travel("jump_land");
                }
                else if ((Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left")) && IsOnFloor()) //in case that the proximity detector fails
                {
                    _stateMachine.Travel("run_stepR");
                }
                else if (IsOnFloor())
                {
                    _stateMachine.Travel("idle");
                }
                break;

            case "jump_land":

                if ((Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left")) && IsOnFloor())
                {
                    _stateMachine.Travel("run_stepR");
                }
                else if (IsOnFloor())
                {
                    _stateMachine.Travel("idle");
                }
                break;

            case "run_turn":
            case "idle_turn":
                break;
        }

        CleanProximityDetected();

    }

    //Function that activate some events that should happen just one time in the transition between 2 states, (place in phisycPrecess())
    private void StateTransition(String prevState, String nextState)
    {
        
        if (nextState == prevState) return;
        GD.Print(nextState);
        if (nextState == "jump_up_stepR" || nextState == "jump_upstepL")
        {
            _velocity.y = speed.y;
            accTime = 0;
        }
        else if (nextState == "start_run")
        {
            accTime = 0;
        }
        else if (nextState == "run_turn")
        {
            GetNode<Sprite>("Sprite").Scale *= new Vector2(-1,1);
            GD.Print(GetNode<Sprite>("Sprite").Scale);
        }
    }




    //Signal recived every time Amanda is "close" to something (floor,walls,etc)
    private void OnSomethingClose(int bodyId, PhysicsBody2D body, int bodyShape, int areaShape)
    {
        _proximityDetected = areaShape;
    }

    void CleanProximityDetected() => _proximityDetected = -1;

}


