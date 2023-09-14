using System;

using UnityEngine;
using UnityEngine.UI;

using Sfs2X.Entities;
using Sfs2X.Entities.Variables;

/**
 * Script attached to Buddy List Item prefab.
 */
public class BuddyListItem : MonoBehaviour
{
	public Image statusIcon;
	public Text mainLabel;
	public Text moodLabel;

	public Button chatButton;
	public Text chatButtonText;
	public Button inviteButton;
	public Button blockButton;
	public Button removeButton;
	public Button addButton;

	public Sprite buttonIconBlock;
	public Sprite buttonIconUnblock;

	public Sprite statusIconAvailable;
	public Sprite statusIconAway;
	public Sprite statusIconOccupied;
	public Sprite statusIconOffline;
	public Sprite statusIconBlocked;

	public bool isBlocked;

	/**
	 * Initialize the prefab instance.
	 */
	public void Init(Buddy buddy)
	{
		SetState(buddy);
	}

	/**
	 * Update prefab instance based on the state of the corresponding buddy.
	 */
	public void SetState(Buddy buddy)
	{
		// Nickname
		mainLabel.text = "<b>" + ((buddy.NickName != null && buddy.NickName != "") ? buddy.NickName : buddy.Name) + "</b>";

		// Age
		DateTime now = DateTime.Now;
		BuddyVariable year = buddy.GetVariable(LobbyController.BUDDYVAR_YEAR);
		mainLabel.text += (year != null && !year.IsNull()) ? " <size=12>(" + (now.Year - year.GetIntValue()) + " yo)</size>" : "";

		// Mood
		BuddyVariable mood = buddy.GetVariable(LobbyController.BUDDYVAR_MOOD);

		if (mood != null && !mood.IsNull() && mood.GetStringValue() != "")
		{
			moodLabel.text = mood.GetStringValue();
			moodLabel.gameObject.SetActive(true);
		}
		else
			moodLabel.gameObject.SetActive(false);

		// Save blocked state
		// (see LobbyController.UpdateBuddyListItem method)
		isBlocked = buddy.IsBlocked;

		// If buddy is not blocked and is temporary, show add button and hide remove button
		bool showAddButton = !buddy.IsBlocked && buddy.IsTemp;
		addButton.gameObject.SetActive(showAddButton);
		removeButton.gameObject.SetActive(!showAddButton);

		// Status icon and buttons
		if (buddy.IsBlocked)
		{
			statusIcon.sprite = statusIconBlocked;
			blockButton.transform.GetComponentInChildren<Image>().sprite = buttonIconUnblock;
			EnableInteraction(false);
			SetChatMsgCounter(0);
		}
		else
		{
			blockButton.transform.GetComponentInChildren<Image>().sprite = buttonIconBlock;

			if (!buddy.IsOnline)
			{
				statusIcon.sprite = statusIconOffline;
				EnableInteraction(false);
				SetChatMsgCounter(0);
			}
			else
			{
				string state = buddy.State;

				if (state == "Available")
					statusIcon.sprite = statusIconAvailable;
				else if (state == "Away")
					statusIcon.sprite = statusIconAway;
				else if (state == "Occupied")
					statusIcon.sprite = statusIconOccupied;

				EnableInteraction(true);
			}
		}
	}

	/**
	 * Show unread chat messages counter.
	 */
	public void SetChatMsgCounter(int value)
	{
		if (value > 0)
			chatButtonText.text = value.ToString();
		else
			chatButtonText.text = "";
	}

	/**
	 * Enable/disable interaction with buttons.
	 */
	private void EnableInteraction(bool enable)
	{
		chatButton.interactable = enable;
		inviteButton.interactable = enable;
	}
}
