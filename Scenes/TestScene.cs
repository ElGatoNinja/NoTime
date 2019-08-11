using Godot;
using System;

public class TestScene : Node2D
{
    
    public Label control;
    public Amanda amanda;

    public GridContainer GravityEdit;
    public GridContainer SpeedEdit;

    public GridContainer AccEdit;


    public override void _Ready()
    {
        control = GetNode<Label>("Control");
        amanda = GetNode<Amanda>("Amanda");
        GravityEdit = GetNode<GridContainer>("GravityEdit");
        SpeedEdit = GetNode<GridContainer>("Speed Edit");
        AccEdit = GetNode<GridContainer>("Acc Edit");

        //GravityEdit.GetNode<LineEdit>("Gravity_1").Text = Convert.ToString(Amanda.amandaGravities[0]);
        //GravityEdit.GetNode<LineEdit>("Gravity_2").Text = Convert.ToString(Amanda.amandaGravities[0]);
        //GravityEdit.GetNode<LineEdit>("Gravity_3").Text = Convert.ToString(Amanda.amandaGravities[0]);

        SpeedEdit.GetNode<LineEdit>("Speed_X").Text = Convert.ToString(amanda.speed.x);
        SpeedEdit.GetNode<LineEdit>("Speed_Y").Text = Convert.ToString(amanda.speed.y);
        //AccEdit.GetNode<LineEdit>("acc").Text = Convert.ToString(Amanda.velocityDelayX);
    }


    public override void _Process(float delta)
    {
        control.Text = (amanda.stateMachine.GetCurrentNode() + "\nproxyDetec: " + amanda.proxyDetected +"\n"+amanda.state.debugInfo +"\n vel: " +amanda.velocity.x);
    }

    private void OnGravity1TextEntered(string text)
    {
       // Amanda.amandaGravities[0] = (int) Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_1").ReleaseFocus();
    }
    private void OnGravity2TextEntered(string text)
    {
        //Amanda.amandaGravities[1] = (int)Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_2").ReleaseFocus();
    }
    private void OnGravity3TextEntered(string text)
    {
        //Amanda.amandaGravities[2] = (int)Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_3").ReleaseFocus();
    }

    private void OnSpeedXTextEntered(string text)
    {
        amanda.speed.x = Convert.ToInt64(text);
        SpeedEdit.GetNode<LineEdit>("Speed_X").ReleaseFocus();
    }

    private void OnSpeedYTextEntered(string text)
    {
        amanda.speed.y = Convert.ToInt64(text);
        SpeedEdit.GetNode<LineEdit>("Speed_Y").ReleaseFocus();
    }

    private void OnDelayTextEntered(String text)
    {
        //Amanda.velocityDelayX = Convert.ToInt64(text);
        AccEdit.GetNode<LineEdit>("acc").ReleaseFocus();
    }
}
