using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Match3 : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField] Sprite[] sprites;
	[SerializeField] RectTransform gameBoard;
	[SerializeField] RectTransform killedBoard;

	[Header("Prefabs")]
	[SerializeField] GameObject nodePiece;
	[SerializeField] GameObject killedPiece;

	int width = 9;
	int height = 14;
	int[] fills;
	Node[,] board;
	List<NodePiece> piecesToUpdate;
	List<KilledPiece> piecesToKill;
	List<FlippedPieces> piecesToFlip;
	List<NodePiece> dead;
	ScoreSystem scoreSystem;
	
	System.Random random;
	
	// Start is called before the first frame update
	void Start()
	{
		scoreSystem = GetComponent<ScoreSystem>();
		enabled = false;
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void StartGame()
	{
		fills = new int[width];
		string seed = getRandomSeed();
		random = new System.Random(seed.GetHashCode());
		piecesToUpdate = new List<NodePiece>();
		piecesToKill = new List<KilledPiece>();
		piecesToFlip = new List<FlippedPieces>();
		dead = new List<NodePiece>();

		InitializeBoard();
		VerifyBoard();
		InstantiateBoard();
		enabled = true;
	}

	void ApplyGravityToBoard()
	{
		for (int column = 0; column < width; column++)
		{
			for (int line = (height-1); line >= 0; line--)
			{
				Coord coord = new Coord(column, line);
				Node empty_node = getNodeAtCoord(coord);
				int type = getTypeAtCoord(coord);
				
				if (type != 0) // If it's not empty, than you don't need to do anything
					continue;
				
				for (int new_line = (line - 1); new_line >= -1; new_line--)
				{
					Coord next_coord = new Coord(column, new_line);
					int next_type = getTypeAtCoord(next_coord);
					if (next_type == 0)
						continue;
					else if (next_type != -1) // Did not hit the end, but it's not 0
					{
						Node gem_node = getNodeAtCoord(next_coord);
						NodePiece gem = gem_node.GetPiece();

						empty_node.SetPiece(gem);
						piecesToUpdate.Add(gem);

						gem_node.SetPiece(null);
					}
					else // Hit an end
					{
						int new_type = fillPieces();
						NodePiece new_piece;
						Coord fallCoord = new Coord(column, -1 - fills[column]);
						if (dead.Count > 0)
						{
							new_piece = dead[0];
							new_piece.gameObject.SetActive(true);
							dead.RemoveAt(0);
						}
						else
						{
							GameObject obj = Instantiate(nodePiece, gameBoard);
							new_piece = obj.GetComponent<NodePiece>();
						}

						new_piece.Rect.anchoredPosition = getPositionFromCoord(fallCoord);
						new_piece.Initialize(new_type, coord, sprites[new_type - 1]);

						empty_node.SetPiece(new_piece);
						ResetPiece(new_piece);
						fills[column]++;
					}
					break;
				}
			}
		}
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
			FlippedPieces flipDuo = getFlipped(piece);
			bool wasFlipped = (flipDuo != null);
			
			int x = piece.index.x;
			fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

			List<Coord> connected = isConnected(piece.index, true);
			
			if (wasFlipped) // If we flipped to make this update
			{
				NodePiece flippedPiece = flipDuo.getOtherPiece(piece);
				AddConnectedGems(ref connected, isConnected(flippedPiece.index, true));
				if (connected.Count == 0) // If we didn't make a match...
					FlipPieces(piece.index, flippedPiece.index, false); // ...flip back
			}
			if (connected.Count > 0) // if we did make a match
			{
				scoreSystem.IncrementScore(connected.Count);
				foreach(Coord coord in connected) // Remove the node pieces connected
				{
					KillPiece(coord);
					Node node = getNodeAtCoord(coord);
					NodePiece nodePiece = node.GetPiece();
					if (nodePiece != null)
					{
						nodePiece.gameObject.SetActive(false);
						dead.Add(nodePiece);
					}
					node.SetPiece(null);
				}

				ApplyGravityToBoard();
			}

			piecesToFlip.Remove(flipDuo); // Remove the flip after done
			piecesToUpdate.Remove(piece);
		}
	}

	FlippedPieces getFlipped(NodePiece ref_piece)
	{
		FlippedPieces flip_duo = null;
		for (int i = 0; i < piecesToFlip.Count; i++)
		{
			if (piecesToFlip[i].getOtherPiece(ref_piece) != null)
			{
				flip_duo = piecesToFlip[i];
				break;
			}
		}
		return flip_duo;
	}

	NodePiece removeFlipped(NodePiece ref_piece)
	{
		NodePiece piece = null;
		for (int i = 0; i < piecesToFlip.Count; i++)
		{
			piece = piecesToFlip[i].getOtherPiece(ref_piece);
			if (piece != null)
				break;
		}
		return piece;
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
				Node node = getNodeAtCoord(new Coord(column, line));
				int type = node.type;
				if (type <= 0)
					continue;
				
				GameObject node_object = Instantiate(nodePiece, gameBoard);
				NodePiece piece = node_object.GetComponent<NodePiece>();
				RectTransform rect = node_object.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(32 + (64 * column), -32 - (64 * line));
				piece.Initialize(type, new Coord(column, line), sprites[type - 1]);
				node.SetPiece(piece);
			}
		}
	}

	public void ResetPiece(NodePiece piece)
	{
		piece.ResetPosition();
		piecesToUpdate.Add(piece);
	}

	public void FlipPieces(Coord one, Coord two, bool isMain)
	{
		if (getTypeAtCoord(one) <= 0)
			return;
		
		Node nodeOne = getNodeAtCoord(one);
		NodePiece pieceOne = nodeOne.GetPiece();
		
		if (getTypeAtCoord(two) > 0)
		{
			Node nodeTwo = getNodeAtCoord(two);
			NodePiece pieceTwo = nodeTwo.GetPiece();

			nodeOne.SetPiece(pieceTwo);
			nodeTwo.SetPiece(pieceOne);

			if (isMain)
				piecesToFlip.Add(new FlippedPieces(pieceOne, pieceTwo));

			piecesToUpdate.Add(pieceOne);
			piecesToUpdate.Add(pieceTwo);
		}
		else
			ResetPiece(pieceOne);
	}


	void KillPiece(Coord coord)
	{
		List<KilledPiece> available = new List<KilledPiece>();
		for (int i = 0; i < piecesToKill.Count; i++)
		{
			if (!piecesToKill[i].falling)
				available.Add(piecesToKill[i]);
		}

		KilledPiece piece = null;
		if (available.Count > 0)
			piece = available[0];
		else
		{
			GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
			piece = kill.GetComponent<KilledPiece>();
			piecesToKill.Add(piece);
		}

		int type = getTypeAtCoord(coord) - 1;
		if (piece != null && type >= 0 && type < sprites.Length)
			piece.Initialize(sprites[type], getPositionFromCoord(coord));
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
		{
			AddConnectedGems(ref connected, list);
		}
		
		if (isMain)
		{
			for (int i = 0; i < connected.Count; i++)
				AddConnectedGems(ref connected, isConnected(connected[i], false));
		}

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
				{
					current_matches.Add(adjacent);
				}
			}

			if (current_matches.Count > 1)
			{	
				Debug.Log("Match count: " + current_matches.Count);
				line.AddRange(current_matches);
				Debug.Log("Line count: " + line.Count);
			}
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
		int type = (random.Next(1, sprites.Length+1));
		return type;
	}

	int getNewValueExcept(ref List<int> toRemove)
	{
		List<int> possible_values = new List<int>();
		for (int i = 0; i < sprites.Length; i++)
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

	Node getNodeAtCoord(Coord coord)
	{
		return board[coord.x, coord.y];
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

	public void GameTimerTimedOut()
	{
		foreach(Transform child in gameBoard.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
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
	NodePiece piece;

	
	public Node(int value, Coord coord)
	{		
		type = value;
		index = coord;
	}

	public void SetPiece(NodePiece new_piece)
	{
		piece = new_piece;
		if (piece == null)
		{
			type = 0;
			return;
		}
		else{
			type = piece.type;
			piece.SetIndex(index);
		}
	}

	public NodePiece GetPiece()
	{
		return piece;
	}
}

[System.Serializable]
public class FlippedPieces
{
	public NodePiece one;
	public NodePiece two;

	public FlippedPieces(NodePiece originPiece, NodePiece targetPiece)
	{
		one = originPiece;
		two = targetPiece;
	}

	public NodePiece getOtherPiece(NodePiece piece)
	{
		if (piece == one)
			return two;
		else if (piece == one)
			return one;
		else
			return null;
	}
}
