using Godot;
using System.Collections.Generic;

public partial class ScorePanel : Control
{
    [Export]
    private HBoxContainer DigitContainer;

    private Texture2D[] _digitTextures = new Texture2D[10];
    private readonly List<TextureRect> _digitNodes = [];
    private int _currentScore = 0;

    public override void _Ready()
    {
        // Load digit textures (0-9)
        for (int i = 0; i < 10; i++)
        {
            _digitTextures[i] = GD.Load<Texture2D>($"res://assets/{i}.png");
        }

        // Initialize with a score of 0
        UpdateScore(0);
    }

    public void UpdateScore(int newScore)
    {
        _currentScore = newScore;
        string scoreStr = newScore.ToString();

        // Adjust the number of TextureRect nodes to match the score length
        while (_digitNodes.Count < scoreStr.Length)
        {
            var digitNode = new TextureRect();
            DigitContainer.AddChild(digitNode);
            _digitNodes.Add(digitNode);
        }
        while (_digitNodes.Count > scoreStr.Length)
        {
            var digitNode = _digitNodes[^1];
            _digitNodes.RemoveAt(_digitNodes.Count - 1);
            digitNode.QueueFree();
        }

        // Update each digit's texture
        for (int i = 0; i < scoreStr.Length; i++)
        {
            int digit = int.Parse(scoreStr[i].ToString());
            _digitNodes[i].Texture = _digitTextures[digit];
        }
    }

    // Optional: Getter for testing or debugging
    public int GetScore()
    {
        return _currentScore;
    }
}