using Godot;

public partial class GameTileRenderer : Node2D
{
	[Export]
	public Texture TilesTexture;

	[Export]
	public Vector2 TileSize = new Vector2(8, 8);

	public void DrawTile(int tileId, Vector2 pos)
	{
		Rect2 rect = new Rect2(pos, TileSize);
		Rect2 srcRect = new Rect2(new Vector2(TileSize.x * tileId, 0), TileSize);

		DrawTextureRectRegion(TilesTexture, rect, srcRect);
	}

	public void DrawPiece(GamePiece piece, Vector2 pos)
	{
		if (piece.PieceData == null)
		{
			return;
		}

		Vector2I pieceSize = piece.GetPieceSize();

		for (int y = 0; y < pieceSize.y; y++)
		{
			for (int x = 0; x < pieceSize.x; x++)
			{
				if (!piece.PieceData[x, y])
				{
					continue;
				}

				DrawTile(piece.TileId, pos + new Vector2(x, y) * TileSize);
			}
		}
	}
}
