using Godot;

public partial class TitleScreen : Node2D
{
    public ClickArea PlayButton;
    public AnimationPlayer AnimationPlayer;
    public Timer StartGameTimer;

    public bool Active = false, ReadyForInput = false;

    [Signal]
    public delegate void GameStarted();

    public void PlayButtonClicked()
    {
        if (!Active)
        {
            return;
        }

        if (!ReadyForInput)
        {
            return;
        }

        Active = false;

        AnimationPlayer.Play("end_screen");
        StartGameTimer.Start();
    }

    public void StartScreen()
    {
        Active = true;
        ReadyForInput = false;

        AnimationPlayer.Play("start_screen");
    }

    public void StartGameTimerTimeout()
    {
        EmitSignal(nameof(GameStarted));
    }

    public void AnimationPlayerFinished(string animName)
    {
        if (animName == "start_screen")
        {
            ReadyForInput = true;
        }
    }

    public override void _Ready()
    {
        PlayButton = GetNode<ClickArea>("PlayButton");
        PlayButton.Connect(nameof(ClickArea.AreaClicked), this, nameof(PlayButtonClicked));

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.Connect("animation_finished", this, nameof(AnimationPlayerFinished));

        StartGameTimer = GetNode<Timer>("StartGameTimer");
        StartGameTimer.Connect("timeout", this, nameof(StartGameTimerTimeout));
    }
}
