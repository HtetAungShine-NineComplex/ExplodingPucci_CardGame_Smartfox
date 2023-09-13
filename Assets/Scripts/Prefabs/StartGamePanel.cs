using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Sfs2X.Entities;

/**
 * Script attached to Start Game Panel prefab.
 */
public class StartGamePanel : BasePanel
{
	public static string DEFAULT_INVITATION_MESSAGE = "Do you want to join my game?";

	public Text expText;
	public Text rankText;
	public Toggle publicToggle;
	public CanvasGroup invitationGroup;
	public InputField invitationMessageInput;

	public Toggle inviteListTogglePrefab;
	public ToggleGroup inviteListToggleGroup;
	public GameObject inviteList;
	public Transform inviteListContent;
	public GameObject offlineWarning;

	public UnityEvent<bool, string> onStartGameConfirm;

	private Dictionary<string, Toggle> inviteListToggles;

	/**
     * On start, set default invitation message.
     */
	public void Start()
	{
		invitationMessageInput.text = DEFAULT_INVITATION_MESSAGE;
	}

	/**
	 * Reset panel.
	 */
	public void Reset(bool isUserOnline, string exp, int rank)
	{
		publicToggle.isOn = true;
		invitationGroup.interactable = false;

		if (inviteListToggles != null)
			foreach (Toggle t in inviteListToggles.Values)
				t.isOn = false;

		expText.text = "Experience = " + exp;
		rankText.text = "Ranking >= " + rank.ToString();

		// Show/hide buddy list
		inviteList.SetActive(isUserOnline);
		offlineWarning.SetActive(!isUserOnline);
	}

	/**
	 * Add/update/remove toggles in invite list.
	 */
	public void UpdateInviteList(Buddy buddy, bool doRemove = false)
	{
		// Init collection
		if (inviteListToggles == null)
			inviteListToggles = new Dictionary<string, Toggle>();

		// Get reference to invite list toggle corresponding to Buddy
		inviteListToggles.TryGetValue(buddy.Name, out Toggle inviteListToggle);

		if (inviteListToggle != null)
		{
			if (doRemove)
			{
				// Remove invite list item from dictionary
				inviteListToggles.Remove(buddy.Name);

				// Destroy game object
				GameObject.Destroy(inviteListToggle.gameObject);
			}
			else
			{
				// Update existing item
				SetInviteToggleState(inviteListToggle, buddy);
			}
		}
		else
		{
			// Create invite list item
			inviteListToggle = Instantiate(inviteListTogglePrefab);
			inviteListToggle.name = buddy.Name;
			inviteListToggle.group = inviteListToggleGroup;
			inviteListToggle.isOn = false;

			inviteListToggles.Add(buddy.Name, inviteListToggle);

			// Init invite list toggle
			SetInviteToggleState(inviteListToggle, buddy);

			// Add invite list toggle to container
			inviteListToggle.gameObject.transform.SetParent(inviteListContent, false);
		}
	}

	/**
	 * Enable/disable interaction on invitation section of the panel.
	 */
	public void OnPublicToggleChange(bool isChecked)
	{
		invitationGroup.interactable = !isChecked;
	}

	/**
	 * Dispatch a custom event to create and join a game Room.
	 */
	public void OnStartGameConfirmClick()
	{
		// Get invited buddy
		string invitedBuddyName = null;

		foreach (Toggle toggle in inviteListToggleGroup.ActiveToggles())
		{
			if (toggle.isOn)
			{
				invitedBuddyName = toggle.name;
				break;
			}
		}

		// Dispatch event
		onStartGameConfirm.Invoke(publicToggle.isOn, invitedBuddyName);

		// Hide panel
		Hide();
	}

	/**
	 * Return the invitation massage.
	 */
	public string GetInvitationMessage()
	{
		return invitationMessageInput.text;
	}

	private void SetInviteToggleState(Toggle toggle, Buddy buddy)
	{
		// Nickname
		toggle.GetComponentInChildren<Text>().text = (buddy.NickName != null && buddy.NickName != "") ? buddy.NickName : buddy.Name;

		// Status
		toggle.interactable = !buddy.IsBlocked && buddy.IsOnline && (buddy.State == "Available" || buddy.State == "Occupied");

		if (!toggle.interactable)
			toggle.isOn = false;
	}
}
