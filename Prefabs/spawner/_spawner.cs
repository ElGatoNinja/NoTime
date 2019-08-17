using Godot;
using System.Collections.Generic;

public class _spawner : Position2D
{
    [Export]
    public PackedScene prefab;
    [Export]
    public uint max = 1;

    [Signal]
    public delegate void Spawned(Node node);
    [Signal]
    public delegate void Despawned(Node node);

    public void Spawn()
    {   
        if(GetChildren().Count >= max)
        {
            Despawn(GetChild(0));     //Avoid infinite items spawned, deletes the first item
        }

        Node aux = prefab.Instance();
        AddChild(aux);
        EmitSignal("Spawned",aux);

    }

    public void Despawn(Node node)
    {
        EmitSignal("Despawned",node);
        node.QueueFree();
    }

    public void Despawn(string node)
    {
        EmitSignal("Despawned",GetNode(node));
        GetNode(node).QueueFree();
    }
}
