using Godot;
using System;

public class PeriodicSpawner : _spawner
{
    [Export]
    public float time = 1;
    public override void _Ready()
    {
        GetNode<Timer>("Timer").WaitTime = time;
    }

    public void OnTimerTimeout()
    {
        Spawn();
    }
    
}
