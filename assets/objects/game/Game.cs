using Godot;

public partial class Game : Node2D
{
	[Export]
	public PackedScene PieceScene;

    public GameBoard Board;

    public Node2D Pieces;
    public GamePiece CurrentPiece;

    public CenterContainer ScoreLabelContainer;
    public Label ScoreLabel;

    public Tween SelectTween;

    public AudioStreamPlayer PlaceSound, FloodSound, FloodStartSound, FloodEndSound, PickUpSound, PutDownSound;

    public AnimationPlayer AnimationPlayer;

    public float PieceScreenSeparation = 48;
	public float PiecesScreenMargin = 24;

	public Vector2 SelectedPieceOffsetMax = new Vector2(0, -40);
	public Vector2 SelectedPieceOffset;

	public float SelectPieceSpeed = 0.15f;
	public float DeselectPieceSpeed = 0.25f;

	public Tween.TransitionType TweenType = Tween.TransitionType.Sine;

	public bool Active = false;

	public float ScoreLabelMargin = 16;

	public int Score = 0;

	public int TileTypeCount = 3;

    public bool ReadyForInput()
	{
		return Board.FloodQueue.Count == 0;
	}

	public void AddScore(int amount)
	{
		Score += amount;
		ScoreLabel.Text = Score.ToString();

		if (Score >= 30000)
		{
			TileTypeCount = 5;
		}
		else if (Score >= 10000)
		{
			TileTypeCount = 4;
		}
	}

	public void OnPiecesDeleted(int amount)
	{
		AddScore(amount * 50);
	}

	public void OnFloodStarted()
	{
		FloodSound.Play();
		FloodStartSound.Play();
	}

	public void OnFloodEnded()
	{
		FloodSound.Stop();
		FloodEndSound.Play();
	}

	public Vector2 GetPieceListPos(GamePiece piece)
	{
		return new Vector2(((Pieces.GetChildCount() - 1) * -0.5f + piece.GetIndex()) * -PieceScreenSeparation, 0);
	}

	public void AlignPieces()
	{
		foreach (GamePiece piece in Pieces.GetChildren())
		{
			piece.Position = GetPieceListPos(piece);
		}
	}

	public int GetRandomTileId()
	{
		return 1 + (int)(GD.Randi() % TileTypeCount);
	}

	public void SpawnPiece()
	{
		GamePiece piece = (GamePiece)PieceScene.Instance();
		Pieces.AddChild(piece);
		Pieces.MoveChild(piece, 0);

		piece.Game = this;
		piece.Connect(nameof(GamePiece.PieceClicked), this, nameof(SelectPiece));

		piece.RandomizePiece();
		piece.TileId = GetRandomTileId();

		piece.Position = GetPieceListPos(piece);
	}

	public void SelectPiece(GamePiece piece)
	{
		if (!Active)
		{
			return;
		}

		if (!ReadyForInput())
		{
			return;
		}

		if (piece.Deselecting)
		{
			return;
		}

		CurrentPiece = piece;

		CurrentPiece.Deselecting = false;

		CurrentPiece.SizeTween.InterpolateProperty(
			CurrentPiece, "scale", CurrentPiece.Scale, Board.Scale,
			SelectPieceSpeed, TweenType, Tween.EaseType.Out);

		CurrentPiece.SizeTween.Start();

		SelectedPieceOffset = CurrentPiece.GlobalPosition - GetGlobalMousePosition();

		SelectTween.InterpolateProperty(
			this, nameof(SelectedPieceOffset), SelectedPieceOffset, SelectedPieceOffsetMax,
			SelectPieceSpeed, TweenType, Tween.EaseType.Out);

		SelectTween.Start();

		CurrentPiece.ZIndex = 1;

		PickUpSound.Play();
	}

	public void DeselectPiece()
	{
		if (CurrentPiece == null)
		{
			return;
		}

		if (Board.CanPlacePiece(CurrentPiece))
		{
			Board.PlacePiece(CurrentPiece);

			Pieces.RemoveChild(CurrentPiece);
			CurrentPiece.QueueFree();

			SpawnPiece();
			
			foreach (GamePiece piece in Pieces.GetChildren())
			{
				piece.PosTween.InterpolateProperty(
					piece, "position", piece.Position, GetPieceListPos(piece),
					0.25f, TweenType, Tween.EaseType.Out);

				piece.PosTween.Start();
			}

			Pieces.GetChild<GamePiece>(0).AnimationPlayer.Play("fade_in");

			AddScore(100);

			PlaceSound.Play();
		}
		else
		{
			CurrentPiece.PosTween.InterpolateProperty(
				CurrentPiece, "position", CurrentPiece.Position, GetPieceListPos(CurrentPiece),
				DeselectPieceSpeed, TweenType, Tween.EaseType.InOut);

			CurrentPiece.PosTween.Start();

			CurrentPiece.SizeTween.InterpolateProperty(
				CurrentPiece, "scale", CurrentPiece.Scale, Vector2.One,
				DeselectPieceSpeed, TweenType, Tween.EaseType.InOut);

			CurrentPiece.SizeTween.Start();

			CurrentPiece.Deselecting = true;

			PutDownSound.Play();
		}

		CurrentPiece = null;

		Board.PiecePreviewRenderer.Update();
	}

	public void StartBoard(Vector2I size)
	{
		Board.StartBoard(size);

		for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				if (GD.Randi() % 8 != 0)
				{
					continue;
				}

				Vector2I tilePos = new Vector2I(x, y);

				Board.SetTileId(tilePos, GetRandomTileId());
			}

		}

		Vector2 boardEnd = Board.ToGlobal(Board.GetScreenPos() + Board.GetScreenSize());

		Pieces.GlobalPosition = new Vector2(GlobalPosition.x, boardEnd.y + PiecesScreenMargin);
	}

	public void PositionScoreLabel()
	{
		ScoreLabelContainer.RectGlobalPosition = new Vector2(0, Board.ToGlobal(Board.GetScreenPos()).y - ScoreLabelMargin);
	}

	public void StartGame()
	{
		Active = true;

        StartBoard(new Vector2I(10, 10));

		for (int i = 0; i < 3; i++)
		{
			SpawnPiece();
		}

		AlignPieces();

		PositionScoreLabel();
        ScoreLabel.Text = "0";

        AnimationPlayer.Play("start_game");

        Board.BoardRenderer.Update();
        Board.PiecePreviewRenderer.Update();
	}

    public override void _Ready()
    {
        Board = GetNode<GameBoard>("GameBoard");
        Board.Game = this;

        Board.Connect(nameof(GameBoard.FloodStarted), this, nameof(OnFloodStarted));
        Board.Connect(nameof(GameBoard.FloodEnded), this, nameof(OnFloodEnded));

        Board.Connect(nameof(GameBoard.PiecesDeleted), this, nameof(OnPiecesDeleted));

        Pieces = GetNode<Node2D>("Pieces");

        ScoreLabelContainer = GetNode<CenterContainer>("ScoreLabelContainer");
        ScoreLabel = ScoreLabelContainer.GetNode<Label>("ScoreLabel");

        SelectTween = GetNode<Tween>("SelectTween");

        PlaceSound = GetNode<AudioStreamPlayer>("Sounds/Place");
        FloodSound = GetNode<AudioStreamPlayer>("Sounds/Flood");
        FloodStartSound = GetNode<AudioStreamPlayer>("Sounds/FloodStart");
        FloodEndSound = GetNode<AudioStreamPlayer>("Sounds/FloodEnd");
        PickUpSound = GetNode<AudioStreamPlayer>("Sounds/PickUp");
        PutDownSound = GetNode<AudioStreamPlayer>("Sounds/PutDown");

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(float delta)
	{
		if (CurrentPiece != null)
		{
			CurrentPiece.GlobalPosition = GetGlobalMousePosition() + SelectedPieceOffset;
			Board.PiecePreviewRenderer.Update();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (!eventMouseButton.Pressed && eventMouseButton.ButtonIndex == (int)ButtonList.Left)
			{
				DeselectPiece();
			}
		}
	}
}
