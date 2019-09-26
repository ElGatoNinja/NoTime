using Godot;
using System;

public class _ParentPlatform : KinematicBody2D
{
    public Vector2 speed = new Vector2(1000,1000);
    public override void _Ready()
    {
        
    }
    public override void _PhysicsProcess(float delta)
    {
        MoveAndSlide(speed);
    }
//  public override void _Process(float delta)
//  {
//      
//  }
}
