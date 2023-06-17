using Godot;

public class Vector2I
{
	public int x, y;

	public Vector2I(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2I(Vector2 v)
	{
		x = (int)v.x;
		y = (int)v.y;
	}

	public static Vector2I One
	{
		get { return new Vector2I(1, 1); }
	}

	public static Vector2I Up
	{
		get { return new Vector2I(0, -1); }
	}

	public static Vector2I Down
	{
		get { return new Vector2I(0, 1); }
	}

	public static Vector2I Left
	{
		get { return new Vector2I(-1, 0); }
	}

	public static Vector2I Right
	{
		get { return new Vector2I(1, 0); }
	}

	public static Vector2I operator +(Vector2I v1, Vector2I v2)
	{
		return new Vector2I(v1.x + v2.x, v1.y + v2.y);
	}

	public static Vector2I operator -(Vector2I v1, Vector2I v2)
	{
		return new Vector2I(v1.x - v2.x, v1.y - v2.y);
	}

	public static Vector2I operator *(Vector2I v1, Vector2I v2)
	{
		return new Vector2I(v1.x * v2.x, v1.y * v2.y);
	}

	public static Vector2I operator /(Vector2I v1, Vector2I v2)
	{
		return new Vector2I(v1.x / v2.x, v1.y / v2.y);
	}

	public static implicit operator Vector2(Vector2I v)
	{
		return new Vector2(v.x, v.y);
	}

	public static implicit operator Vector2I(Vector2 v)
	{
		return new Vector2I((int)v.x, (int)v.y);
	}
}
