using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Requests.Buddylist;

/**
 * Script attached to the Controller object in the Game scene.
 */
public class GameSceneController : BaseSceneController
{
	public TicTacToeGameManager gameManager;

	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public LeavePanel leavePanel;
	public InputField messageInput;
	public Text chatTextArea;
	public ScrollRect chatScrollView;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private SmartFox sfs;
	private bool runTimer;
	private float timer = 20;
	private string lastSenderName;

	//----------------------------------------------------------
	// Unity callback methods
	//----------------------------------------------------------
	
	private void Start()
	{
		// Set a reference to the SmartFox client instance
		sfs = gm.GetSfsClient();

		// Hide modal panels
		HideModals();

		// Print system message
		PrintSystemMessage("Game joined as " + (sfs.MySelf.IsPlayer ? "player" : "spectator"));

		// Add event listeners
		AddSmartFoxListeners();

		// Initialize game manager
		// The SmartFox class instance is passed, so that the game manager can add its own listeners
		// and interact with the server-side Room Extension containing the game logic
		gameManager.Init(sfs);

		// If user is the first player in the Room, set a timeout
		// Having a timeout is usefult to suggest the user to leave the game if not yet started within some time
		// For example this could mean that the invited buddy refused the invitation, or the server couldn't locate other players to invite
		if (sfs.MySelf.IsPlayer && sfs.LastJoinedRoom.PlayerList.Count == 1)
			runTimer = true;
	}

	override protected void Update()
	{
		base.Update();

		// Check timeout
		if (runTimer)
		{
			timer -= Time.deltaTime;
			
			int timerInt = (int)Math.Round(timer, 0);

			if (timerInt == 0)
				StopTimeout(true);
		}
	}

	//----------------------------------------------------------
	// UI event listeners
	//----------------------------------------------------------
	#region
	/**
	 * On public chat message input edit end, if the Enter key was pressed, send the chat message.
	 */
	public void OnMessageInputEndEdit()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			SendMessage();
	}

	/**
	 * On Send button click, send the chat message.
	 */
	public void OnSendButtonClick()
	{
		SendMessage();
	}

	/**
	 * On Leave button click, go back to GuestLogin scene.
	 */
	public void OnLeaveButtonClick()
	{
		// Destroy game manager
		gameManager.Destroy();

		// Leave current game room
		sfs.Send(new LeaveRoomRequest());

		// Set user as "Available" in Buddy List system
		if (sfs.BuddyManager.MyOnlineState)
			sfs.Send(new SetBuddyVariablesRequest(new List<BuddyVariable> { new SFSBuddyVariable(ReservedBuddyVariables.BV_STATE, "Available") }));

		// Return to lobby scene
		SceneManager.LoadScene("Lobby");
	}

	/**
	 * Add Add Buddy button click, add opponent to buddy list.
	 */
	public void OnAddBuddyClick(string userName)
	{
		sfs.Send(new AddBuddyRequest(userName));
	}
	#endregion

	//----------------------------------------------------------
	// Helper methods
	//----------------------------------------------------------
	#region
	/**
	 * Add all SmartFoxServer-related event listeners required by the scene.
	 */
	private void AddSmartFoxListeners()
	{
		sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);

		sfs.AddEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyAdd);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnBuddyMessage);
	}

	/**
	 * Remove all SmartFoxServer-related event listeners added by the scene.
	 * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
	 */
	override protected void RemoveSmartFoxListeners()
	{
		sfs.RemoveEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.RemoveEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);

		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyAdd);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnBuddyMessage);
	}

	/**
	 * Hide all modal panels.
	 */
	override protected void HideModals()
	{
		leavePanel.Hide();
	}

	/**
	 * Disable timeout showing panel suggesting player to leave.
	 */
	private void StopTimeout(bool showPanel)
	{
		runTimer = false;

		if (showPanel)
			leavePanel.Show();
		else
			leavePanel.Hide();
	}

	/**
	 * Send a public chat message.
	 */
	private void SendMessage()
	{
		if (messageInput.text != "")
		{
			// Send public message to Room
			sfs.Send(new PublicMessageRequest(messageInput.text));

			// Reset message input
			messageInput.text = "";
			messageInput.ActivateInputField();
			messageInput.Select();
		}
	}

	/**
	 * Display a chat message.
	 */
	private void PrintChatMessage(string message, string senderName)
	{
		// Print sender name, unless they are the same of the last message
		if (senderName != lastSenderName)
			chatTextArea.text += "<b>" + (senderName == "" ? "Me" : senderName) + "</b>\n";

		// Print chat message
		chatTextArea.text += message + "\n";

		// Save reference to last message sender, to avoid repeating the name for subsequent messages from the same sender
		lastSenderName = senderName;

		// Scroll view to bottom
		ScrollChatToBottom();
	}

	/**
	 * Display a system message.
	 */
	private void PrintSystemMessage(string message)
	{
		// Print message
		chatTextArea.text += "<color=#ffffff><i>" + message + "</i></color>\n";

		// Scroll view to bottom
		ScrollChatToBottom();
	}

	/**
     * Scroll chat to bottom.
     */
	private void ScrollChatToBottom()
	{
		Canvas.ForceUpdateCanvases();
		chatScrollView.verticalNormalizedPosition = 0;
	}
	#endregion

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------
	#region
	private void OnPublicMessage(BaseEvent evt)
	{
		User sender = (User)evt.Params["sender"];
		string message = (string)evt.Params["message"];

		// Display chat message
		PrintChatMessage(message, sender != sfs.MySelf ? sender.Name : "");
	}

	private void OnUserEnterRoom(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];
		Room room = (Room)evt.Params["room"];

		// Display system message
		PrintSystemMessage("User " + user.Name + " joined this game as " + (user.IsPlayerInRoom(room) ? "player" : "spectator"));

		// Stop timeout
		if (user.IsPlayerInRoom(room))
			StopTimeout(false);
	}

	private void OnUserExitRoom(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];

		// Display system message
		if (user != sfs.MySelf)
			PrintSystemMessage("User " + user.Name + " left the game");
	}

	private void OnSpectatorToPlayer(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];

		// Display system message
		if (user == sfs.MySelf)
			PrintSystemMessage("Game joined as player");
		else
		{
			PrintSystemMessage("User " + user.Name + " is now a player");

			// Stop timeout
			if (user.IsPlayer)
				StopTimeout(false);
		}
	}

	private void OnBuddyAdd(BaseEvent evt)
	{
		// When a buddy is added, hide Add Buddy button in game interface
		gameManager.UpdatePlayerTags();
	}

	private void OnBuddyMessage(BaseEvent evt)
	{
		// Add message to queue in BuddyChatHistoryManager static class
		BuddyChatMessage chatMsg = BuddyChatHistoryManager.AddMessage(evt.Params);

		// Increase message counter
		// NOTE: there's no need to make sure the sender is not the current user,
		// as in this example the current user can't send buddy messages from the Game scene
		BuddyChatHistoryManager.IncreaseUnreadMessagesCount(chatMsg.buddyName);
	}
	#endregion
}
