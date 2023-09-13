using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class GameBoard : MonoBehaviour
{
	public UnityEvent<int, int> onSlotClick;

	private GameBoardSlot[,] slots;
	private bool isEnabled;

	void Start()
	{
		InitBoard();
	}

	public void Reset()
	{
		foreach (GameBoardSlot slot in slots)
			if (slot) // Ignore null slots (0-indexes)
				slot.SetMark(GameBoardSlot.Mark.EMPTY);
	}

	public bool IsEnabled
	{
		get => isEnabled;

		set {
			isEnabled = value;
		}
	}

	/**
	 * Listen to custom click event dispatched by GameBoardSlot instances.
	 */
	public void OnSlotClick(int row, int col)
	{
		if (isEnabled)
		{
			// Re-dispatch event
			onSlotClick.Invoke(row, col);
		}
	}

	public void SetMark(int r, int c, int value)
	{
		slots[r, c].SetMark((GameBoardSlot.Mark)value);
	}

	/**
	 * Initialize data structure containing references to GameBoardSlot instances.
	 */
	private void InitBoard()
	{
		// We use a two-dimensional array with 4x4 entries, so we can have 1-based indexes for the board 
		slots = new GameBoardSlot[4, 4];

		for (int r = 1; r <= 3; r++)
		{
			for (int c = 1; c <= 3; c++)
			{
				string slotObjName = String.Format("Slot {0}-{1}", r, c);
				Transform slotTr = this.transform.Find(slotObjName);

				slots[r, c] = slotTr.gameObject.GetComponent<GameBoardSlot>();
			}
		}
	}
}
