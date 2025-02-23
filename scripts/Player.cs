using Godot;
using System;

public enum PlayerState
{
    Alive,
    Dead
}

public partial class Player : Area2D
{
    const float STRENGTH = -300f;
    const float FLOOR_OFFSET = 190f;
    const float TITLT_ANGLE_RATIO = 0.08f;

    [Signal] public delegate void OnPlayerDeathEventHandler();
    [Signal] public delegate void OnPlayerHitTheFloorEventHandler();
    [Signal] public delegate void OnPlayerScoreIncreaseEventHandler();

    public PlayerState State { get; private set; } = PlayerState.Alive;
    private AnimatedSprite2D _sprite;
    private Vector2 _direction;

    public override void _Ready()
    {
        string spriteName = "AnimatedSprite" + GD.RandRange(1, 3);
        _sprite = GetNode<AnimatedSprite2D>(spriteName);
        _sprite.Visible = true;
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
        _direction.Y += (float)(Gravity * delta);

        var nextPosition = Position + _direction * (float)delta;

        if (nextPosition.Y > FLOOR_OFFSET)
        {
            nextPosition.Y = FLOOR_OFFSET;
            _direction.Y = 0;
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

        _direction.Y = STRENGTH;
        AudioManager.PlaySound("wing");
    }

    private void RotateSprite()
    {
        // slightly tilt the sprite up based on the direction
        var angle = _direction.Y * TITLT_ANGLE_RATIO;

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
        _sprite.Play("dead");
        State = PlayerState.Dead;
        _direction = Vector2.Zero;
        EmitSignal(nameof(SignalName.OnPlayerDeath));

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);

        if (_direction.Y != 0)
        {
            AudioManager.PlaySound("die");
        }
    }

}
