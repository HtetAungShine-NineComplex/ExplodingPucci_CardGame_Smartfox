using UnityEngine;
using UnityEngine.UI;

public class PlayerTag : MonoBehaviour
{
	public Text label;
	public Text playerName;
	public Text winsValue;
	public Button addBuddyButton;

	private int wins;

	public int Wins
	{
		get => wins;

		set {
			wins = value;

			// Update wins label
			winsValue.text = "Wins: " + wins;

			// Show/hide wins label
			winsValue.gameObject.SetActive(wins > 0);
		}
	}
}
