using Godot;
using System;

public class PeriodicSpawner : _spawner
{
    
    public override void _Ready()
    {
        GetNode<Timer>("Timer").Start();
    }

    public void OnTimerTimeout()
    {
        Spawn();
    }
    
}
