using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class GamePiece : Node2D
{
	public const string PieceDataFilePath = "res://assets/data/pieces.json";
	public static readonly List<bool[,]> PieceDataDb = new List<bool[,]>();

	public bool[,] PieceData;
	public int TileId;

	public Game Game;

	public GameTileRenderer Renderer;
	public Tween PosTween, SizeTween;

	public Area2D ClickArea;
	public CollisionShape2D ClickShape;

	public AnimationPlayer AnimationPlayer;

	public bool Deselecting = false;

	[Signal]
	public delegate void PieceClicked(GamePiece piece);

	public static Vector2I GetPieceSize(bool[,] pieceData)
	{
		return new Vector2I(pieceData.GetLength(0), pieceData.GetLength(1));
	}

	public Vector2I GetPieceSize()
	{
		return GetPieceSize(PieceData);
	}

	public static void StartPieceDataDb()
	{
		PieceDataDb.Clear();

		File piecesFile = new File();
		piecesFile.Open(PieceDataFilePath, File.ModeFlags.Read);

		var json = JSON.Parse(piecesFile.GetAsText()).Result;

		Array piecesFileDicts = (Array)JSON.Parse(piecesFile.GetAsText()).Result;

		foreach (Dictionary pieceFileDict in piecesFileDicts)
		{
			Array pieceFileData = (Array)pieceFileDict["data"];

			Vector2I pieceSize = new Vector2I(((Array)pieceFileData[0]).Count, pieceFileData.Count);
			bool[,] piece = new bool[pieceSize.x, pieceSize.y];

			for (int y = 0; y < pieceSize.y; y++)
			{
				Array pieceFileRow = (Array)pieceFileData[y];

				for (int x = 0; x < pieceSize.x; x++)
				{
					piece[x, y] = (int)(float)pieceFileRow[x] != 0;
				}
			}

			int chance = (int)(float)pieceFileDict["chance"];

			for (int i = 0; i < chance; i++)
			{
				PieceDataDb.Add(piece);
			}
		}
	}

	public void RandomizePiece()
	{
		bool[,] randomPiece = PieceDataDb[(int)(GD.Randi() % PieceDataDb.Count)];

		Vector2I pieceSize = GetPieceSize(randomPiece);
		Vector2I pieceEnd = pieceSize - Vector2I.One;

		int orientation = (int)(GD.Randi() % 4);
		bool flipped = (int)(GD.Randi() % 2) == 0;

		Vector2I finalPieceSize;

		if (orientation == 0 || orientation == 2)
		{
			finalPieceSize = new Vector2I(pieceSize.x, pieceSize.y);
		}
		else
		{
			finalPieceSize = new Vector2I(pieceSize.y, pieceSize.x);
		}

		bool[,] finalPiece = new bool[finalPieceSize.x, finalPieceSize.y];

		for (int y = 0; y < pieceSize.y; y++)
		{
			for (int x = 0; x < pieceSize.x; x++)
			{
				Vector2I tilePos;

				switch (orientation)
				{
					case 0:
						tilePos = new Vector2I(x, y);
						break;
					case 1:
						tilePos = new Vector2I(pieceEnd.y - y, x);
						break;
					case 2:
						tilePos = new Vector2I(pieceEnd.x - x, pieceEnd.y - y);
						break;
					case 3:
						tilePos = new Vector2I(y, pieceEnd.x - x);
						break;
					default:
						tilePos = new Vector2I(x, y);
						break;
				}

				if (flipped)
				{
					tilePos = new Vector2I(finalPieceSize.x - 1 - tilePos.x, tilePos.y);
				}

				finalPiece[tilePos.x, tilePos.y] = randomPiece[x, y];
			}
		}

		PieceData = finalPiece;

		Renderer.Position = (Vector2)finalPieceSize * -0.5f * Renderer.TileSize;

		Renderer.Update();
	}

	public void ClickAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == (int)ButtonList.Left)
			{
				EmitSignal(nameof(PieceClicked), this);
			}
		}
	}

	public void OnPosTweenEnded()
	{
        if (Deselecting)
		{
			Deselecting = false;

            ZIndex = 0;
        }
    }

	public void DrawPiece()
	{
		Renderer.DrawPiece(this, Vector2.Zero);
	}

	public override void _Ready()
	{
		Renderer = GetNode<GameTileRenderer>("Renderer");
		Renderer.Connect("draw", this, nameof(DrawPiece));

		ClickArea = GetNode<Area2D>("ClickArea");
		ClickArea.Connect("input_event", this, nameof(ClickAreaInputEvent));

		ClickShape = ClickArea.GetNode<CollisionShape2D>("CollisionShape2D");

		PosTween = GetNode<Tween>("PosTween");
		SizeTween = GetNode<Tween>("SizeTween");

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		PosTween.Connect("tween_all_completed", this, nameof(OnPosTweenEnded));
    }
}
