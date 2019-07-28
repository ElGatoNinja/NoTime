using Godot;

public class TestScene : Node2D
{
    
    public Label control;
    public Amanda Amanda;

    public override void _Ready()
    {
        control = GetNode<Label>("Control");
        Amanda = GetNode<Amanda>("Amanda");
    }


    public override void _Process(float delta)
    {
        control.SetText("Running: " + Amanda._running +"\nJumping: " + Amanda._jumping +"\nFalling: "+Amanda._falling +"\nProximityD: "+ Amanda._proximityDetected );
    }
}
