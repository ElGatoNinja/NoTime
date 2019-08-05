using Godot;
using System;

public class TestScene : Node2D
{
    
    public Label control;
    public Amanda Amanda;

    public GridContainer GravityEdit;
    public GridContainer SpeedEdit;


    public override void _Ready()
    {
        control = GetNode<Label>("Control");
        Amanda = GetNode<Amanda>("Amanda");
        GravityEdit = GetNode<GridContainer>("GravityEdit");
        SpeedEdit = GetNode<GridContainer>("Speed Edit");

        GravityEdit.GetNode<LineEdit>("Gravity_1").Text = Convert.ToString(Amanda.amandaGravities[0]);
        GravityEdit.GetNode<LineEdit>("Gravity_2").Text = Convert.ToString(Amanda.amandaGravities[0]);
        GravityEdit.GetNode<LineEdit>("Gravity_3").Text = Convert.ToString(Amanda.amandaGravities[0]);

        SpeedEdit.GetNode<LineEdit>("Speed_X").Text = Convert.ToString(Amanda.speed.x);
        SpeedEdit.GetNode<LineEdit>("Speed_Y").Text = Convert.ToString(Amanda.speed.y);
    }


    public override void _Process(float delta)
    {
        control.SetText(Amanda._stateMachine.GetCurrentNode() + "\n\n onFloor: " + Amanda.IsOnFloor());
    }

    private void OnGravity1TextEntered(string text)
    {
        Amanda.amandaGravities[0] = Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_1").ReleaseFocus();
    }
    private void OnGravity2TextEntered(string text)
    {
        Amanda.amandaGravities[1] = Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_2").ReleaseFocus();
    }
    private void OnGravity3TextEntered(string text)
    {
        Amanda.amandaGravities[2] = Convert.ToInt64(text);
        GravityEdit.GetNode<LineEdit>("Gravity_3").ReleaseFocus();
    }

    private void OnSpeedXTextEntered(string text)
    {
        Amanda.speed.x = Convert.ToInt64(text);
        SpeedEdit.GetNode<LineEdit>("Speed_X").ReleaseFocus();
    }

    private void OnSpeedYTextEntered(string text)
    {
        Amanda.speed.y = Convert.ToInt64(text);
        SpeedEdit.GetNode<LineEdit>("Speed_Y").ReleaseFocus();
    }
}
