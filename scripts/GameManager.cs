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
    const int PIPE_GAP = 200;
    const int PIPE_WIDTH = 52;
    const int MAX_PIPE_Y_OFFSET = 128;
    const double FLOOR_OFFSET = 384;
    const float SPEED = 100f;

    public static GameState State { get; private set; } = GameState.GameStart;
    public static int Score { get; private set; }
    public static GameManager Instance { get; private set; }

    private static readonly PackedScene PipesScene = GD.Load<PackedScene>("res://scenes/pipes.tscn");
    private Player _player;
    private ScorePanel _scorePanel;
    private Control _gameStartPanel;
    private Control _gameOverPanel;

    private Node2D _floor;
    private Pipes[] _pipes = new Pipes[5];

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
        _player = GetNode<Player>("/root/Main/Stage/Player");
        _scorePanel = GetNode<ScorePanel>("/root/Main/Stage/ScorePanel");
        _gameStartPanel = GetNode<Control>("/root/Main/Stage/GameStartPanel");
        _gameOverPanel = GetNode<Control>("/root/Main/Stage/GameOverPanel");
        _floor = GetNode<Node2D>("/root/Main/Stage/Floor");
        _player.Connect(nameof(Player.SignalName.OnPlayerDeath), Callable.From(OnPlayerDeath));
        _player.Connect(nameof(Player.SignalName.OnPlayerHitTheFloor), Callable.From(OnPlayerHitTheFloor));
        _player.Connect(nameof(Player.SignalName.OnPlayerScoreIncrease), Callable.From(OnPlayerScoreIncrease));
        CreatePipes();
        Score = 0;
    }

    private void CreatePipes()
    {
        int spriteNumber = GD.RandRange(1, 2);

        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i] = PipesScene.Instantiate() as Pipes;
            _pipes[i].SpriteNumber = spriteNumber;

            var pipePositionOffset = GetViewport().GetVisibleRect().Size.X / 2 + PIPE_WIDTH / 2;
            var pipeYPosition = new Random().Next(-MAX_PIPE_Y_OFFSET, MAX_PIPE_Y_OFFSET);

            _pipes[i].Position = new Vector2(pipePositionOffset + i * PIPE_GAP, pipeYPosition);
            GetNode<Node2D>("/root/Main/Stage/Pipes").AddChild(_pipes[i]);
        }
    }

    private void MovePipes(double delta)
    {
        foreach (var pipe in _pipes)
        {
            pipe.Position += new Vector2(-SPEED * (float)delta, 0);

            if (pipe.Position.X < -pipe.GetViewportRect().Size.X / 2 - PIPE_WIDTH / 2)
            {
                var rightMostPipe = _pipes.First(item => item.Position.X == _pipes.Max(item => item.Position.X));
                var pipePositionOffset = rightMostPipe.Position.X + PIPE_GAP;
                var pipeYPosition = new Random().Next(-MAX_PIPE_Y_OFFSET, MAX_PIPE_Y_OFFSET);
                pipe.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
                pipe.Position = new Vector2(pipePositionOffset, pipeYPosition);
                pipe.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
            }
        }
    }

    private void MoveFloor(double delta)
    {
        _floor.Position += new Vector2(-SPEED * (float)delta, 0);

        if (_floor.Position.X < -FLOOR_OFFSET)
        {
            _floor.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
            _floor.Position = new Vector2(0, _floor.Position.Y);
            _floor.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("flap"))
        {
            if (State == GameState.GameStart)
            {
                State = GameState.Playing;
                _player.Visible = true;
                _gameStartPanel.Visible = false;
                _scorePanel.Visible = true;
                _player.ProcessMode = Node.ProcessModeEnum.Pausable;
                _player.Flap();
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
        _gameOverPanel.Visible = true;
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
        _scorePanel.UpdateScore(Score);
        AudioManager.PlaySound("point");
    }
}
