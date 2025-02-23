using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata;

public enum GameState
{
    GameStart,
    Playing,
    GamePaused,
    GameOver
}

public partial class GameManager : Node
{
    public static readonly PackedScene PipesScene = GD.Load<PackedScene>("res://scenes/pipes.tscn");
    public static readonly int PipeGap = 200;
    public static readonly int PipeWidth = 52;
    public static readonly int maxPipeYOffset = 128;
    public static readonly double FloorOffset = 384;
    public static GameManager Instance { get; private set; }
    public static float Speed { get; private set; } = 100f;
    public static Player Player { get; private set; }
    public static ScorePanel ScorePanel { get; private set; }
    public static Control GameStartPanel { get; private set; }
    public static Control GameOverPanel { get; private set; }
    public static GameState State { get; private set; } = GameState.GameStart;
    public static int Score { get; private set; }

    public static Node2D Floor { get; private set; }
    private Pipes[] pipes = new Pipes[5];
    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        Init();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (State == GameState.Playing)
        {
            MovePipes(delta);
            MoveFloor(delta);
        }
    }

    private void Init()
    {
        Player = GetNode<Player>("/root/Main/Stage/Player");
        ScorePanel = GetNode<ScorePanel>("/root/Main/Stage/ScorePanel");
        GameStartPanel = GetNode<Control>("/root/Main/Stage/GameStartPanel");
        GameOverPanel = GetNode<Control>("/root/Main/Stage/GameOverPanel");
        Floor = GetNode<Node2D>("/root/Main/Stage/Floor");
        Player.Connect(nameof(Player.SignalName.OnPlayerDeath), Callable.From(OnPlayerDeath));
        Player.Connect(nameof(Player.SignalName.OnPlayerHitTheFloor), Callable.From(OnPlayerHitTheFloor));
        Player.Connect(nameof(Player.SignalName.OnPlayerScoreIncrease), Callable.From(OnPlayerScoreIncrease));
        CreatePipes();
        Score = 0;
    }

    private void CreatePipes()
    {
        int spriteNumber = GD.RandRange(1, 2);

        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i] = PipesScene.Instantiate() as Pipes;
            pipes[i].SpriteNumber = spriteNumber;

            var pipePositionOffset = GetViewport().GetVisibleRect().Size.X / 2 + PipeWidth / 2;
            var pipeYPosition = new Random().Next(-maxPipeYOffset, maxPipeYOffset);

            pipes[i].Position = new Vector2(pipePositionOffset + i * PipeGap, pipeYPosition);
            GetNode<Node2D>("/root/Main/Stage/Pipes").AddChild(pipes[i]);
        }
    }

    private void MovePipes(double delta)
    {
        foreach (var pipe in pipes)
        {
            pipe.Position += new Vector2(-Speed * (float)delta, 0);

            if (pipe.Position.X < -pipe.GetViewportRect().Size.X / 2 - PipeWidth / 2)
            {
                var rightMostPipe = pipes.First(item => item.Position.X == pipes.Max(item => item.Position.X));
                var pipePositionOffset = rightMostPipe.Position.X + PipeGap;
                var pipeYPosition = new Random().Next(-maxPipeYOffset, maxPipeYOffset);
                pipe.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
                pipe.Position = new Vector2(pipePositionOffset, pipeYPosition);
                pipe.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
            }
        }
    }

    private void MoveFloor(double delta)
    {
        Floor.Position += new Vector2(-Speed * (float)delta, 0);

        if (Floor.Position.X < -FloorOffset)
        {
            Floor.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
            Floor.Position = new Vector2(0, Floor.Position.Y);
            Floor.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("flap"))
        {
            if (State == GameState.GameStart)
            {
                State = GameState.Playing;
                Player.Visible = true;
                GameStartPanel.Visible = false;
                ScorePanel.Visible = true;
                Player.ProcessMode = Node.ProcessModeEnum.Pausable;
                Player.Flap();
            }
        }

        if (@event.IsActionPressed("pause"))
        {
            ProcessGamePause();
        }
    }

    private void OnPlayerDeath()
    {
        State = GameState.GameOver;
    }

    private void OnPlayerHitTheFloor()
    {
        GameOverPanel.Visible = true;
        // wait for 3 seconds and restart the game
        GetTree().CreateTimer(3).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(RestartGame));
    }


    private void ProcessGamePause()
    {
        if (State == GameState.Playing)
        {
            PauseGame();
        }
        else if (State == GameState.GamePaused)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        State = GameState.GamePaused;
        // Pause physics and animations
        GetTree().Paused = true;
        GD.Print("Game Paused!");
    }

    public void ResumeGame()
    {
        State = GameState.Playing;
        GetTree().Paused = false;
        GD.Print("Game Resumed!");
    }

    private async void RestartGame()
    {
        State = GameState.GameStart;
        var error = GetTree().ReloadCurrentScene();

        if (error == Error.Ok)
        {
            // Wait for the tree to change
            await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);
            Init();
        }
        else
        {
            GD.Print("Reload failed: ", error);
        }
    }

    private void OnPlayerScoreIncrease()
    {
        Score++;
        ScorePanel.UpdateScore(Score);
        AudioManager.PlaySound("point");
    }
}
