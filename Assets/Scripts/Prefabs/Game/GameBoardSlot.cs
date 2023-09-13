using UnityEngine;
using UnityEngine.Events;

public class GameBoardSlot : MonoBehaviour
{
    public int row;
    public int col;

	public SpriteRenderer spriteRenderer;
	public Sprite[] markSprites;

	public UnityEvent<int, int> onSlotClick;

	public enum Mark
	{
		EMPTY = 0,
		CROSS,
		RING
	};

	private Mark mark = Mark.EMPTY;

	private void OnMouseDown()
	{
		if (mark == Mark.EMPTY)
			onSlotClick.Invoke(row, col);
	}

	public void SetMark(Mark mark)
	{
		spriteRenderer.sprite = markSprites[((int)mark)];
		this.mark = mark;
	}
}
