using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Coord
{
	public int x;
	public int y;

	public Coord(int newX, int newY)
	{
		x = newX;
		y = newY;
	}

	public Vector2 ToVector()
	{
		return new Vector2(x, y);
	}

	public bool Equals(Coord reference)
	{
		return (x == reference.x && y == reference.y);
	}

	public void  mult(int factor)
	{
		x *= factor; 
		y *= factor;
	}

	public void add(Coord coord)
	{
		x += coord.x;
		y += coord.y;
	}

	public static Coord fromVector(Vector2 vector)
	{
		return new Coord((int)vector.x, (int)vector.y);
	}
	
	public static Coord fromVector(Vector3 vector)
	{
		return new Coord((int)vector.x, (int)vector.y);
	}

	public static Coord mult(Coord coord, int factor)
	{
		return new Coord(coord.x * factor, coord.y * factor);
	}

	public static Coord add(Coord coord1, Coord coord2)
	{
		return new Coord(coord1.x + coord2.x, coord1.y + coord2.y);
	}

	public static Coord clone(Coord coord)
	{
		return new Coord(coord.x, coord.y);
	}
	
	public static Coord zero
	{
		get{ return new Coord(0, 0); }
	}

	public static Coord one
	{
		get{ return new Coord(1, 1); }
	}

	public static Coord up
	{
		get{ return new Coord(0, 1); }
	}

	public static Coord left
	{
		get{ return new Coord(-1, 0); }
	}

	public static Coord right
	{
		get{ return new Coord(1, 0); }
	}

	public static Coord down
	{
		get{ return new Coord(0, -1); }
	}
}
