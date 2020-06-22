using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public int type;
	public Coord index;

	private Image img;
	private Vector2 _pos;
	public Vector2 Position
	{
		get 
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}
	private RectTransform _rect;
	public RectTransform Rect
	{
		get
		{
			return _rect;
		}
		set
		{
			_rect = value;
		}
	}

	private bool updating = false;

	public void Initialize(int new_type, Coord new_coord, Sprite piece)
	{
		img = GetComponent<Image>();
		_rect = GetComponent<RectTransform>();

		type = new_type;
		SetIndex(new_coord);
		img.sprite = piece;
	}

	public void SetIndex(Coord new_coord)
	{
		index = new_coord;
		ResetPosition();
		UpdateName();
	}

	public void ResetPosition()
	{
		_pos = new Vector2(32 + (64 * index.x), -32 - (64 * index.y));
	}

	public bool UpdatePiece()
	{
		if (Vector3.Distance(_rect.anchoredPosition, _pos) > 1)
		{
			MoveTo(_pos);
			updating = true;
		}
		else
		{
			_rect.anchoredPosition = _pos;
			updating = false;
		}

		return updating;
	}

	void UpdateName()
	{
		transform.name = "Node [" + index.x + ", " + index.y + "]";
	}

	public void Move(Vector2 direction)
	{
		_rect.anchoredPosition += direction * Time.deltaTime * 16f;
	}
	public void MoveTo(Vector2 new_pos)
	{
		_rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, new_pos, Time.deltaTime * 16f);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (updating)
			return;

		updating = true;
		MovePieces.instance.MovePiece(this);
		img.color = new Color32(103,87,255,255);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		MovePieces.instance.DropPiece();
		img.color = new Color32(255,255,255,255);
	}
}
