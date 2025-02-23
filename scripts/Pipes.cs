using Godot;
using System;

public partial class Pipes : Node2D
{
    [Export] public int SpriteNumber { get; set; } = 1;
    private Sprite2D topPipe;
    private Sprite2D bottomPipe;

    public override void _Ready()
    {
        topPipe = GetNode<Sprite2D>("TopPipe/Sprite" + SpriteNumber);
        bottomPipe = GetNode<Sprite2D>("BottomPipe/Sprite" + SpriteNumber);
        topPipe.Visible = true;
        bottomPipe.Visible = true;
    }
}
