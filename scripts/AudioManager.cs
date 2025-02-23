using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public static void PlaySound(string soundName)
    {
        var sound = Instance.GetNode<AudioStreamPlayer2D>(soundName);
        sound.Play();
    }
}