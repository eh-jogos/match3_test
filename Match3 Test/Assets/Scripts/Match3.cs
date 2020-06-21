using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Match3 : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField] Sprite[] pieces;
	[SerializeField] RectTransform gameBoard;

	[Header("Prefabs")]
	[SerializeField] GameObject nodePiece;

	int width = 9;
	int height = 14;
	Node[,] board;
	List<NodePiece> piecesToUpdate;
	System.Random random;
	
	// Start is called before the first frame update
	void Start()
	{
		StartGame();
	}

	void StartGame()
	{
		string seed = getRandomSeed();
		random = new System.Random(seed.GetHashCode());
		piecesToUpdate = new List<NodePiece>();

		InitializeBoard();
		VerifyBoard();
		InstantiateBoard();
	}

	// Update is called once per frame
	void Update()
	{
		List<NodePiece> finishedUpdating = new List<NodePiece>();
		for (int i = 0; i < piecesToUpdate.Count; i++)
		{
			NodePiece piece = piecesToUpdate[i];
			if (!piece.UpdatePiece()) 
				finishedUpdating.Add(piece);
		}

		foreach (NodePiece piece in finishedUpdating)
		{
			piecesToUpdate.Remove(piece);
		}
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
		List<int> toRemove;
		for (int column = 0; column < width; column++)
		{
			for (int line = 0; line < height; line++)
			{
				Coord coord = new Coord(column, line);
				int type = getTypeAtCoord(coord);

				if (type <= 0)
					continue;
				
				toRemove = new List<int>();
				while(isConnected(coord, true).Count > 0)
				{
					type = getTypeAtCoord(coord);

					if (!toRemove.Contains(type))
						toRemove.Add(type);
					
					int new_type = getNewValueExcept(ref toRemove);
					setTypeAtCoord(coord, new_type);
				}
				
			}
		}
	}

	void InstantiateBoard()
	{
		for (int column = 0; column < width; column++)
		{
			for (int line = 0; line < height; line++)
			{
				int type = board[column, line].type;
				if (type <= 0)
					continue;
				
				GameObject node_object = Instantiate(nodePiece, gameBoard);
				NodePiece node = node_object.GetComponent<NodePiece>();
				RectTransform rect = node_object.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(32 + (64 * column), -32 - (64 * line));
				node.Initialize(type, new Coord(column, line), pieces[type - 1]);
			}
		}
	}

	public void ResetPiece(NodePiece piece)
	{
		piece.ResetPosition();
		piecesToUpdate.Add(piece);
	}

	List<Coord> isConnected(Coord ref_coord, bool isMain)
	{
		List<Coord> connected = new List<Coord>();
		int type = getTypeAtCoord(ref_coord);
		Coord[] directions = 
		{
			Coord.up,
			Coord.right,
			Coord.down,
			Coord.left
		};

		List<Coord>[] lists_to_add = {
			CheckForConnectedLines(ref directions, ref_coord, type),
			CheckForMiddleGems(ref directions, ref_coord, type),
			CheckForSquares(ref directions, ref_coord, type)
		};

		foreach (List<Coord> list in lists_to_add)
			AddConnectedGems(ref connected, list);
		
		if (isMain)
		{
			for (int i = 0; i < connected.Count; i++)
				AddConnectedGems(ref connected, isConnected(connected[i], false));
		}

		if (connected.Count > 0)
			connected.Add(ref_coord);

		return connected;
	}

	List<Coord> CheckForConnectedLines(ref Coord[] directions, Coord ref_coord, int ref_type)
	{
		// Checking if there are 2 or more of the same shape in any direction
		List<Coord> line = new List<Coord>();
		
		foreach (Coord dir in directions) 
		{
			
			List<Coord> current_matches = new List<Coord>();
			
			for(int i = 1; i < 3; i++)
			{
				Coord adjacent = Coord.add(ref_coord, Coord.mult(dir, i));
				int adjacent_type = getTypeAtCoord(adjacent);
				if (adjacent_type == ref_type)
					current_matches.Add(adjacent);
			}

			if (current_matches.Count > 1)
				line.AddRange(current_matches);
		}

		if (line.Count > 1)
			return line;
		else
		{
			line.Clear();
			return line;
		}

	}
	
	List<Coord> CheckForMiddleGems(ref Coord[] directions, Coord ref_coord, int ref_type)
	{
		// Checking if there are 2 or more of the same shape in any direction
		List<Coord> line = new List<Coord>();
		Coord[][] grouped_directions = new Coord[][]{
			new Coord [] {directions[0], directions[2]}, // Vertical Directions
			new Coord [] {directions[1], directions[3]}  // Horizontal Directions
		};

		foreach (Coord[] group in grouped_directions)
		{
			List<Coord> current_matches = new List<Coord>();
			Coord[] gems_around_current_one = { Coord.add(ref_coord, group[0]), Coord.add(ref_coord, group[1]) };
			foreach(Coord gem in gems_around_current_one)
			{
				if (getTypeAtCoord(gem) == ref_type)
					current_matches.Add(gem);
			}
			
			if (current_matches.Count > 1)
				line.AddRange(current_matches);

		}

		if (line.Count > 1)
			return line;
		else
		{
			line.Clear();
			return line;
		}
	}

	List<Coord> CheckForSquares(ref Coord[] directions, Coord ref_coord, int ref_type)
	{
		// Checking if there are 2 or more of the same shape in any direction
		List<Coord> square = new List<Coord>();
		List<List<Coord>> grouped_directions = new List<List<Coord>>();
		for (int i = 0; i < directions.Length; i++  )
		{
			List<Coord> square_directions = new List<Coord>();
			int next_direction = (i + 1) % directions.Length;
			square_directions.Add(directions[i]);
			square_directions.Add(directions[next_direction]);
			square_directions.Add(Coord.add(directions[i], directions[next_direction])); // diagonal direction
			grouped_directions.Add(square_directions);
		}

		foreach (List<Coord> group in grouped_directions)
		{
			List<Coord> current_matches = new List<Coord>();
			Coord[] gems_around_current_one = { 
				Coord.add(ref_coord, group[0]), 
				Coord.add(ref_coord, group[1]), 
				Coord.add(ref_coord, group[2]) 
			};
			
			foreach(Coord gem in gems_around_current_one)
			{
				if (getTypeAtCoord(gem) == ref_type)
					current_matches.Add(gem);
			}
			
			if (current_matches.Count > 2)
				square.AddRange(current_matches);

		}

		if (square.Count > 2)
			return square;
		else
		{
			square.Clear();
			return square;
		}
	}

	void AddConnectedGems(ref List<Coord> linkedGems, List<Coord> gemsToAdd)
	{
		foreach(Coord gem in gemsToAdd)
		{
			bool should_add = true;
			foreach (Coord alredy_linked_gem in linkedGems)
			{
				if (alredy_linked_gem.Equals(gem))
				{
					should_add = false;
					break;
				}
			}

			if (should_add)
				linkedGems.Add(gem);
		}
	}

	int fillPieces()
	{
		int type = (random.Next(1, pieces.Length+1));
		return type;
	}

	int getNewValueExcept(ref List<int> toRemove)
	{
		List<int> possible_values = new List<int>();
		for (int i = 0; i < pieces.Length; i++)
			possible_values.Add(i+1);
		
		foreach(int i in toRemove)
			possible_values.Remove(i);
		
		if (possible_values.Count <= 0) 
			return 0;
		else
			return possible_values[random.Next(0, possible_values.Count)];

	}
	
	void setTypeAtCoord(Coord coord, int new_type)
	{
		board[coord.x, coord.y].type = new_type;
	}
	
	int getTypeAtCoord(Coord coord)
	{
		if(coord.x < 0 || coord.x >= width || coord.y < 0 || coord.y >= height)
			return -1;
		return board[coord.x, coord.y].type; 
	}

	public Vector2 getPositionFromCoord(Coord coord)
	{
		return new Vector2(36 + (64 * coord.x), -32 - (64 * coord.y));
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