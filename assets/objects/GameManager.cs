using Godot;

public class GameManager : Node
{
    public TitleScreen TitleScreen;
    public Game Game;

    public Timer StartGameTimer;

    public void TitleScreenGameStarted()
    {
        Game.StartGame();
    }

    public override void _Ready()
    {
        VisualServer.SetDefaultClearColor(Colors.Black);

        GD.Randomize();

        GamePiece.StartPieceDataDb();

        TitleScreen = GetNode<TitleScreen>("TitleScreen");
        TitleScreen.Connect(nameof(TitleScreen.GameStarted), this, nameof(TitleScreenGameStarted));

        Game = GetNode<Game>("Game");

        TitleScreen.StartScreen();
    }
}
