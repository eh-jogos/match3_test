using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{

	[SerializeField] Sprite[] pieces;

	int width = 9;
	int height = 14;
	Node[,] board;

	System.Random random;
	
	// Start is called before the first frame update
	void Start()
	{
		
	}

	void StartGame()
	{
		string seed = getRandomSeed();
		random = new System.Random(seed.GetHashCode());

		InitializeBoard();
	}

	void InitializeBoard()
	{
		board = new Node[width, height];
		for (int line = 0; line < height; line++)
		{
			for (int column = 0; column < width; column++)
			{

				board[column, line] = new Node(fillPieces(), new Coord(column, line));
			}
		}
	}

	void VerifyBoard()
	{
		for (int column = 0; column < width; column++)
		{
			for (int line = 0; line < height; line++)
			{
				Coord coord = new Coord(column, line);
				int type = getTypeAtCoord(coord);
			}
		}
	}

	int fillPieces()
	{
		int type = 1; // We don't want it to be empty, so we don't start at 0
		type = (random.Next(0,100) / (100 / pieces.Length)) + 1;
		return type;
	}

	int getTypeAtCoord(Coord coord)
	{
		return board[coord.x, coord.y]; 
	}

	// Update is called once per frame
	void Update()
	{
		
	}


	string getRandomSeed()
	{
		string seed = "";
		string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%&*()^";
		for (int i = 0; i < 20; i++)
		{
			seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
		}
		return seed;
	}
}

[System.Serializable]
public class Node
{
	// Represents the type of "gem" that is in this node
	// -1 = hole
	// 0 = empty
	// 1 = milk
	// 2 = apple
	// 3 = orange
	// 4 = bread 
	// 5 = brocolis
	// 6 = conconut
	// 7 = start
	public int type; 
	public Coord index;

	public Node(int value, Coord coord)
	{		
		type = value;
		index = coord;
	}
}