using Godot;
using System;

public partial class Pipes : Node2D
{
    [Export] public int SpriteNumber { get; set; } = 1;
    private Sprite2D _topPipe;
    private Sprite2D _bottomPipe;

    public override void _Ready()
    {
        _topPipe = GetNode<Sprite2D>("TopPipe/Sprite" + SpriteNumber);
        _bottomPipe = GetNode<Sprite2D>("BottomPipe/Sprite" + SpriteNumber);
        _topPipe.Visible = true;
        _bottomPipe.Visible = true;
    }
}
