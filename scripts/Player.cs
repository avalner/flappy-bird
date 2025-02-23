using Godot;
using System;

public enum PlayerState
{
    Alive,
    Dead
}

public partial class Player : Area2D
{
    public float Strength = -300f;
    public float Speed = 50f;
    public float floorOffset = 190f;
    public float tiltAngleRatio = 0.08f;
    public PlayerState State { get; private set; } = PlayerState.Alive;

    [Signal] public delegate void OnPlayerDeathEventHandler();
    [Signal] public delegate void OnPlayerHitTheFloorEventHandler();
    [Signal] public delegate void OnPlayerScoreIncreaseEventHandler();

    private AnimatedSprite2D sprite;
    private Vector2 direction;

    public override void _Ready()
    {
        string spriteName = "AnimatedSprite" + GD.RandRange(1, 3);
        sprite = GetNode<AnimatedSprite2D>(spriteName);
        sprite.Visible = true;
        State = PlayerState.Alive;
        Gravity = 1000f;
        AreaEntered += OnAreaEntered;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("flap"))
        {
            Flap();
        }
    }

    override public void _PhysicsProcess(double delta)
    {
        direction.Y += (float)(Gravity * delta);

        var nextPosition = Position + direction * (float)delta;

        if (nextPosition.Y > floorOffset)
        {
            nextPosition.Y = floorOffset;
            direction.Y = 0;
        }

        Position = nextPosition;
        RotateSprite();

    }

    public void Flap()
    {
        if (State == PlayerState.Dead)
        {
            return;
        }

        direction.Y = Strength;
        AudioManager.PlaySound("wing");
    }

    private void RotateSprite()
    {
        // slightly tilt the sprite up based on the direction
        var angle = direction.Y * tiltAngleRatio;

        Rotation = Mathf.DegToRad(angle);
    }

    private async void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("Floor"))
        {
            EmitSignal(nameof(SignalName.OnPlayerHitTheFloor));
        }

        if (State == PlayerState.Dead)
        {
            return;
        }

        if (area.IsInGroup("ScoreBox"))
        {
            EmitSignal(nameof(SignalName.OnPlayerScoreIncrease));
            return;
        }

        AudioManager.PlaySound("hit");
        sprite.Play("dead");
        State = PlayerState.Dead;
        direction = Vector2.Zero;
        EmitSignal(nameof(SignalName.OnPlayerDeath));

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);

        if (direction.Y != 0)
        {
            AudioManager.PlaySound("die");
        }
    }

}
