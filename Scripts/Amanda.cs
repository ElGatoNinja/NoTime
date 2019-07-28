using Godot;
using System;

public class Amanda : KinematicBody2D
{

    //varaibles preceded by underScore are state variables. And contain the info of the state of the player

    [Export]
    public Vector2 speed = new Vector2(0,0);
    [Export]
    public float AmandasGravity = 9.8f;

    public Vector2 velocity = Vector2.Zero;

    public bool _isLookingAtRight = true;
    public bool _running = false;
    public bool _jumping = false;
    public bool _falling = true;
    public int _proximityDetected = -1;
    private AnimatedSprite animatedSprite;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        animatedSprite = animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {

        this.ProcessInput();

        if (_jumping)
        {
            velocity.y = -speed.y;
            _falling = true;
        }
        else if(_running)
        {
            if(_isLookingAtRight)
                velocity.x = speed.x;
            else
                velocity.x = -speed.x;
        }
        else 
        {
            velocity.x = 0;
        }


        velocity = MoveAndSlide(velocity,new Vector2(0,-1));
        if(!IsOnFloor())
            velocity.y += AmandasGravity*delta;
        else
        {
            velocity.y = 10;
            _falling = false;
        }

    }
    public override void _Process(float delta)
    {

        if (_jumping)
        {
            animatedSprite.Animation = "jump_up";
            _jumping = false;
       
        }else if (_falling)
        {
            if(_proximityDetected == 0)
                animatedSprite.Animation = "jump_down";
        }
        else if(_running)
        {
            animatedSprite.Animation = "run";
        }
        else
        {
            animatedSprite.Animation = "idle";
        }

        //if moving to left, flip the animation
        animatedSprite.FlipH = !_isLookingAtRight;

        animatedSprite.Play();
        CleanProximityDetected();

    }

    //GetInput Manages the respose to input events for Amanda
    private void ProcessInput()
    {
        _running = false;

        if(Input.IsActionJustPressed("ui_up") && IsOnFloor())
        {
            _jumping = true;
        }

        if (Input.IsActionPressed("ui_right"))
        {
            _running = true;
            _isLookingAtRight = true;
        }
        if (Input.IsActionPressed("ui_left"))
        {
            _running = true;
            _isLookingAtRight = false;
        } 
        if(Input.IsActionJustReleased("ui_right") || Input.IsActionJustReleased("ui_left"))
        {
            _running = false;
        }
    }


    //Signal recived every time Amanda is "close" to something (floor,walls,etc)
    private void OnSomethingClose(int bodyId, PhysicsBody2D body, int bodyShape, int areaShape)
    {
        _proximityDetected = areaShape;
    }

    void CleanProximityDetected() => _proximityDetected = -1;

}


