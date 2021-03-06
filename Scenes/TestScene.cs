using Godot;
using System;

public class TestScene : Node2D
{
    
    public Label control;
    public Amanda amanda;
    
    public PeriodicSpawner ps; 


    public override void _Ready()
    {
        control = GetNode<Label>("Control");
        amanda = GetNode<Amanda>("Amanda");
        ps = GetNode<PeriodicSpawner>("PeriodicSpawner");
    }


    public override void _Process(float delta)
    {
        control.Text = (amanda.stateMachine.GetCurrentNode() + "\nproxyDetec: " + amanda.proxyDetected +"\n"+amanda.state.debugInfo +"\n vel: " +amanda.velocity.x);
    }
}