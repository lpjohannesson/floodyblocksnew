using Godot;
using System.Collections.Generic;

public partial class GameBoard : Node2D
{
	[Export]
	public PackedScene FloodFlashScene;

    public float floodSpeed = 0.05f;

	public int[,] TileIds;

	public Game Game;
	public GameTileRenderer BoardRenderer, PiecePreviewRenderer;
	public Node2D FloodFlashes;

	public Queue<Vector2I> FloodQueue = new Queue<Vector2I>();

	public float FloodTimer = 0;
	public int FloodTileId;

	[Signal]
	public delegate void FloodStarted();

	[Signal]
	public delegate void FloodEnded();

	[Signal]
	public delegate void PiecesDeleted(int amount);

	public Vector2I GetSize()
	{
		return new Vector2I(TileIds.GetLength(0), TileIds.GetLength(1));
	}

	public Vector2 GetScreenSize()
	{
		return (Vector2)GetSize() * BoardRenderer.TileSize;
	}

	public Vector2 GetScreenPos()
	{
		return GetScreenSize() * -0.5f;
	}

	public Vector2I GetPiecePos(GamePiece piece)
	{
		return WorldToTile(piece.Renderer.GlobalPosition);
	}

	public int GetTileId(Vector2I tilePos)
	{
		return TileIds[tilePos.x, tilePos.y];
	}

	public void SetTileId(Vector2I tilePos, int tileId)
	{
		TileIds[tilePos.x, tilePos.y] = tileId;
	}

	public bool TileInRange(Vector2I tilePos)
	{
		Vector2I size = GetSize();

		if (tilePos.x < 0 || tilePos.x >= size.x)
		{
			return false;
		}

		if (tilePos.y < 0 || tilePos.y >= size.y)
		{
			return false;
		}

		return true;
	}

	public Vector2I WorldToTile(Vector2 worldPos)
	{
		return (Vector2I)((ToLocal(worldPos) - GetScreenPos()) / BoardRenderer.TileSize).Round();
	}

	public Vector2 TileToWorld(Vector2I tilePos)
	{
		return ToGlobal(GetScreenPos() + (Vector2)tilePos * BoardRenderer.TileSize);
	}

	public void StartBoard(Vector2I size)
	{
		TileIds = new int[size.x, size.y];

		for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				Vector2I tilePos = new Vector2I(x, y);

				SetTileId(tilePos, 0);
			}
		}

		BoardRenderer.Position = PiecePreviewRenderer.Position = GetScreenPos();
	}

	public void SpawnFloodFlash(Vector2I tilePos)
	{
		FloodFlash floodFlash = (FloodFlash)FloodFlashScene.Instance();
		FloodFlashes.AddChild(floodFlash);

		floodFlash.GlobalPosition = TileToWorld(tilePos);
		floodFlash.StartFlash(GetTileId(tilePos));
	}

	public static void EnqueueFloodNeighbors(Queue<Vector2I> queue, Vector2I tilePos)
	{
		queue.Enqueue(tilePos + Vector2I.Left);
		queue.Enqueue(tilePos + Vector2I.Right);
		queue.Enqueue(tilePos + Vector2I.Up);
		queue.Enqueue(tilePos + Vector2I.Down);
	}

	public void StepFloodPoints()
	{
		int queueCount = FloodQueue.Count;
		int deletedCount = 0;

		for (int i = 0; i < queueCount; i++)
		{
			Vector2I floodPoint = FloodQueue.Dequeue();

			if (!TileInRange(floodPoint))
			{
				continue;
			}

			int tileId = GetTileId(floodPoint);

			if (tileId != FloodTileId)
			{
				continue;
			}

			SpawnFloodFlash(floodPoint);
			SetTileId(floodPoint, 0);

			EnqueueFloodNeighbors(FloodQueue, floodPoint);

			deletedCount++;
		}

		if (deletedCount > 0)
		{
			EmitSignal(nameof(PiecesDeleted), deletedCount);
		}

		BoardRenderer.Update();
	}

	public void UpdateFloodPoints(float delta)
	{
		FloodTimer += delta;

		while (FloodTimer >= floodSpeed)
		{
			FloodTimer -= floodSpeed;

			StepFloodPoints();
		}

		if (FloodQueue.Count == 0)
		{
			EmitSignal(nameof(FloodEnded));
			return;
		}
	}

	public void CheckForClearedSections(GamePiece piece)
	{
		Vector2I size = GetSize();

		Vector2I piecePos = GetPiecePos(piece);
		Vector2I pieceSize = piece.GetPieceSize();

		List<Vector2I> searchStartPoints = new List<Vector2I>();

		for (int y = 0; y < pieceSize.y; y++)
		{
			for (int x = 0; x < pieceSize.x; x++)
			{
				if (!piece.PieceData[x, y])
				{
					continue;
				}

				Vector2I tilePos = piecePos + new Vector2I(x, y);

				searchStartPoints.Add(tilePos);
			}
		}

		Queue<Vector2I> searchQueue = new Queue<Vector2I>();

		foreach (Vector2I tilePos in searchStartPoints)
		{
			searchQueue.Enqueue(tilePos);
		}

		bool[,] visitedMap = new bool[size.x, size.y];

		bool reachesLeft = false, reachesRight = false, reachesTop = false, reachesBottom = false;
		bool reachesTwoEnds = false;

		while (searchQueue.Count > 0)
		{
			Vector2I nextTilePos = searchQueue.Dequeue();

			if (!TileInRange(nextTilePos))
			{
				continue;
			}

			if (visitedMap[nextTilePos.x, nextTilePos.y])
			{
				continue;
			}

			visitedMap[nextTilePos.x, nextTilePos.y] = true;

			int nextTileId = GetTileId(nextTilePos);

			if (nextTileId != piece.TileId)
			{
				continue;
			}

			reachesLeft |= nextTilePos.x == 0;
			reachesRight |= nextTilePos.x == size.x - 1;
			reachesTop |= nextTilePos.y == 0;
			reachesBottom |= nextTilePos.y == size.y - 1;

			if ((reachesTop && reachesBottom) || (reachesLeft && reachesRight))
			{
				reachesTwoEnds = true;
				break;
			}

			EnqueueFloodNeighbors(searchQueue, nextTilePos);
		}

		if (!reachesTwoEnds)
		{
			return;
		}

		FloodTimer = 0;
		FloodTileId = piece.TileId;

		foreach (Vector2I tilePos in searchStartPoints)
		{
			FloodQueue.Enqueue(tilePos);
		}

		StepFloodPoints();

		EmitSignal(nameof(FloodStarted));
	}

	public bool CanPlacePiece(GamePiece piece)
	{
		Vector2I piecePos = GetPiecePos(piece);
		Vector2I pieceSize = piece.GetPieceSize();

		for (int y = 0; y < pieceSize.y; y++)
		{
			for (int x = 0; x < pieceSize.x; x++)
			{
				if (!piece.PieceData[x, y])
				{
					continue;
				}

				Vector2I tilePos = piecePos + new Vector2I(x, y);

				if (!TileInRange(tilePos))
				{
					return false;
				}

				if (GetTileId(tilePos) != 0)
				{
					return false;
				}
			}
		}

		return true;
	}

	public bool PlacePiece(GamePiece piece)
	{
		if (!CanPlacePiece(piece))
		{
			return false;
		}

		Vector2I piecePos = GetPiecePos(piece);
		Vector2I pieceSize = piece.GetPieceSize();

		for (int y = 0; y < pieceSize.y; y++)
		{
			for (int x = 0; x < pieceSize.x; x++)
			{
				if (!piece.PieceData[x, y])
				{
					continue;
				}

				Vector2I tilePos = piecePos + new Vector2I(x, y);

				SetTileId(tilePos, piece.TileId);
			}
		}

		CheckForClearedSections(piece);

		BoardRenderer.Update();

		return true;
	}

	public void DrawBoard()
	{
		if (TileIds == null)
		{
			return;
		}

		Vector2I size = GetSize();

		for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				BoardRenderer.DrawTile(TileIds[x, y], new Vector2(x, y) * BoardRenderer.TileSize);
			}
		}
	}

	public void DrawPiecePreview()
	{
		GamePiece previewPiece = Game.CurrentPiece;

		if (previewPiece == null)
		{
			return;
		}

		if (!CanPlacePiece(previewPiece))
		{
			return;
		}

		Vector2 tileSize = BoardRenderer.TileSize;
		Vector2 piecePos = (Vector2)GetPiecePos(previewPiece) * tileSize;

		PiecePreviewRenderer.DrawPiece(previewPiece, piecePos);
		PiecePreviewRenderer.Update();
	}

	public override void _Ready()
	{
		BoardRenderer = GetNode<GameTileRenderer>("BoardRenderer");
		BoardRenderer.Connect("draw", this, nameof(DrawBoard));

		PiecePreviewRenderer = GetNode<GameTileRenderer>("PiecePreviewRenderer");
		PiecePreviewRenderer.Connect("draw", this, nameof(DrawPiecePreview));

		FloodFlashes = GetNode<Node2D>("FloodFlashes");
	}

	public override void _Process(float delta)
	{
		if (FloodQueue.Count > 0)
		{
			UpdateFloodPoints(delta);
		}
	}
}
