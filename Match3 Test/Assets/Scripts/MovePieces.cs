using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePieces : MonoBehaviour
{
	public static MovePieces instance;
	Match3 game;
	
	NodePiece moving;
	Coord newIndex;
	Vector2 mouseStart;

	private void Awake() {
		instance = this;

	}
	// Start is called before the first frame update
	void Start()
	{
		game = GetComponent<Match3>();
	}

	// Update is called once per frame
	void Update()
	{
		if (moving)
		{
			Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
			Vector2 normalizedDir = dir.normalized;
			Vector2 absoluteDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

			newIndex = Coord.clone(moving.index);
			Coord toAdd = Coord.zero;
			if (dir.magnitude > 32) // If our mouse is 32 pixels away from the starting point of the mouse
			{
				if (absoluteDir.x > absoluteDir.y)
					if (normalizedDir.x > 0)
						toAdd = Coord.right;
					else
						toAdd = Coord.left;
				else
					if (normalizedDir.y > 0)
						toAdd = Coord.down;
					else
						toAdd = Coord.up;
			}
			
			newIndex.add(toAdd);

			Vector2 pos = game.getPositionFromCoord(moving.index);
			if (!newIndex.Equals(moving.index))
				pos += Coord.mult(new Coord(toAdd.x,-toAdd.y), 16).ToVector();
			
			moving.MoveTo(pos);

		}
	}

	public void MovePiece(NodePiece piece)
	{
		if (moving != null)
			return;

		moving = piece;
		mouseStart = Input.mousePosition;
	}

	public void DropPiece()
	{
		if (moving == null)
			return;
		
		if (!newIndex.Equals(moving.index))
			game.FlipPieces(moving.index, newIndex, true);
		else
			game.ResetPiece(moving);

		moving = null;
		
	}
}
