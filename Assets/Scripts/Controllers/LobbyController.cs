using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Invitation;
using Sfs2X.Entities.Match;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Requests.Buddylist;
using Sfs2X.Requests.Game;

/**
 * Script attached to the Controller object in the Lobby scene.
 */
public class LobbyController : BaseSceneController
{
	public static string BUDDYVAR_YEAR = SFSBuddyVariable.OFFLINE_PREFIX + "year";
	public static string BUDDYVAR_MOOD = "mood";

	public static string USERVAR_EXPERIENCE = "exp";
	public static string USERVAR_RANKING = "rank";

	public static string DEFAULT_ROOM = "The Lobby";
	public static string GAME_ROOMS_GROUP_NAME = "games";

	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public Text loggedInAsLabel;
	public Text userStatusLabel;
	public UserProfilePanel userProfilePanel;
	public StartGamePanel startGamePanel;
	public InvitationPanel invitationPanel;
	public WarningPanel warningPanel;

	public Transform gameListContent;
	public GameListItem gameListItemPrefab;

	public GameObject buddyListPanel;
	public Transform buddyListContent;
	public BuddyListItem buddyListItemPrefab;
	public InputField buddyNameInput;
	public BuddyChatPanel chatPanel;

	public Queue<InvitationWrapper> invitationQueue;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private SmartFox sfs;
	private Dictionary<int, GameListItem> gameListItems;
	private Dictionary<string, BuddyListItem> buddyListItems;

	// Comment EXTENSION_ID and EXTENSION_CLASS constants below and
	// uncomment the following to use the Java version of the Tic Tac Toe Extension
	private const string EXTENSION_ID = "TicTacToe";
	private const string EXTENSION_CLASS = "sfs2x.extensions.games.tictactoe.TicTacToeExtension";

	// Comment above EXTENSION_ID and EXTENSION_CLASS constants and
	// uncomment the following to use the JavaScript version of the Tic Tac Toe Extension
	//private const string EXTENSION_ID = "TicTacToe-JS";
	//private const string EXTENSION_CLASS = "TicTacToeExtension.js";

	//----------------------------------------------------------
	// Unity callback methods
	//----------------------------------------------------------
	
	private void Start()
	{
		// Set a reference to the SmartFox client instance
		sfs = gm.GetSfsClient();

		// Hide modal panels
		HideModals();

		// Display username in footer and user profile panel
		loggedInAsLabel.text = "Logged in as <b>" + sfs.MySelf.Name + "</b>";
		userProfilePanel.InitUserProfile(sfs.MySelf.Name);

		// Init or display player profile details
		InitPlayerProfile();

		// Add event listeners
		AddSmartFoxListeners();

		// Populate list of available games
		PopulateGamesList();

		// Initialize Buddy List system
		// We have to check if it was already initialized if this scene is reloaded after leaving the Game scene
		if (!sfs.BuddyManager.Inited)
			sfs.Send(new InitBuddyListRequest());
		else
			InitializeBuddyClient();

		// Join default Room
		// An initial Room where to join all users is needed to implement a public chat (not in this example)
		// or to automatically invite users to enter a private game when launched (see OnStartGameConfirm method below)
		// In this example we assume the Room already exists in the static SmartFoxServer configuration
		sfs.Send(new JoinRoomRequest(DEFAULT_ROOM));
	}

	//----------------------------------------------------------
	// UI event listeners
	//----------------------------------------------------------
	#region
	/**
	 * On Logout button click, disconnect from SmartFoxServer.
	 * This causes the SmartFox listeners added by this scene to be removed (see BaseSceneController.OnDestroy method)
	 * and the GuestLogin scene to be loaded (see SFSClientManager.OnConnectionLost method).
	 */
	public void OnLogoutButtonClick()
	{
		// Disconnect from SmartFoxServer
		sfs.Disconnect();
	}

	/**
	 * On Start game button click, show Start Game Panel prefab instance.
	 */
	public void OnStartGameButtonClick()
	{
		startGamePanel.Reset(sfs.BuddyManager.MyOnlineState, sfs.MySelf.GetVariable(USERVAR_EXPERIENCE).GetStringValue(), sfs.MySelf.GetVariable(USERVAR_RANKING).GetIntValue());
		startGamePanel.Show();
	}

	/**
	 * On Quick join button click, send request to join a random game Room.
	 */
	public void OnQuickJoinButtonClick()
	{
		// Quick join a game in the "games" group among those matching the current user's player profile
		sfs.Send(new QuickJoinGameRequest(null, new List<string>() { GAME_ROOMS_GROUP_NAME }, sfs.LastJoinedRoom));
	}

	/**
	 * On:
	 * - Start game button click on the Start game panel, or
	 * - Invite buddy button click on a Buddy List Item prefab instance,
	 * create and join a new game Room.
	 */
	public void OnStartGameConfirm(bool isPublic, string buddyName)
	{
		// Configure Room
		string roomName = sfs.MySelf.Name + "'s game";

		SFSGameSettings settings = new SFSGameSettings(roomName);
		settings.GroupId = GAME_ROOMS_GROUP_NAME;
		settings.MaxUsers = 2;
		settings.MaxSpectators = 10;
		settings.MinPlayersToStartGame = 2;
		settings.IsPublic = isPublic;
		settings.LeaveLastJoinedRoom = true;
		settings.NotifyGameStarted = false;
		settings.Extension = new RoomExtension(EXTENSION_ID, EXTENSION_CLASS);

		// Additional settings specific to private games
		if (!isPublic) // This check is actually redundant: if the game is public, the invitation-related settings are ignored
		{
			// Invite a buddy
			if (buddyName != null)
			{
				settings.InvitedPlayers = new List<object>();
				settings.InvitedPlayers.Add(sfs.BuddyManager.GetBuddyByName(buddyName));
			}

			// Search the "default" group, which in this example contains the static default Room only
			settings.SearchableRooms = new List<string>() { "default" };

			// Additional invitation parameters
			ISFSObject invParams = new SFSObject();
			invParams.PutUtfString("room", roomName);
			invParams.PutUtfString("message", startGamePanel.GetInvitationMessage());

			settings.InvitationParams = invParams;
		}

		// Define players match expression to locate the users to invite
		var matchExp = new MatchExpression(USERVAR_EXPERIENCE, StringMatch.EQUALS, sfs.MySelf.GetVariable(USERVAR_EXPERIENCE).GetStringValue());
		matchExp.And(USERVAR_RANKING, NumberMatch.GREATER_OR_EQUAL_THAN, sfs.MySelf.GetVariable(USERVAR_RANKING).GetIntValue());

		settings.PlayerMatchExpression = matchExp;

		// Request Room creation to server
		sfs.Send(new CreateSFSGameRequest(settings));
	}

	/**
	 * On Play game button click in Game List Item prefab instance, join an existing game Room as a player.
	 */
	public void OnGameItemPlayClick(int roomId)
	{
		// Join game Room as player
		sfs.Send(new Sfs2X.Requests.JoinRoomRequest(roomId, null, sfs.LastJoinedRoom.Id));
	}

	/**
	 * On Watch game button click in Game List Item prefab instance, join an existing game Room as a spectator.
	 */
	public void OnGameItemWatchClick(int roomId)
	{
		// Join game Room as spectator
		sfs.Send(new Sfs2X.Requests.JoinRoomRequest(roomId, null, sfs.LastJoinedRoom.Id, true));
	}

	/**
	 * On User icon click, show User Profile Panel prefab instance.
	 */
	public void OnUserIconClick()
	{
		userProfilePanel.Show();
	}

	/**
	 * On buddy name input edit end, if the Enter key was pressed, send request to add buddy to user's Buddy List.
	 */
	public void OnBuddyNameInputEndEdit()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			OnAddBuddyButtonClick();
	}

	/**
	 * On Add buddy button click, send request to add buddy to user's Buddy List.
	 */
	public void OnAddBuddyButtonClick()
	{
		if (buddyNameInput.text != "")
		{
			// Request buddy adding to buddy list
			sfs.Send(new AddBuddyRequest(buddyNameInput.text));
			buddyNameInput.text = "";
		}
	}

	/**
	 * On Add buddy button click in Buddy List Item prefab instance, send request to add temporary buddy to user's Buddy List.
	 */
	public void OnAddBuddyButtonClick(string buddyName)
	{
		sfs.Send(new AddBuddyRequest(buddyName));
	}

	/**
	 * On Block buddy button click in Buddy List Item prefab instance, send request to block/unblock buddy in user's Buddy List.
	 */
	public void OnBlockBuddyButtonClick(string buddyName)
	{
		bool isBlocked = sfs.BuddyManager.GetBuddyByName(buddyName).IsBlocked;

		// Request buddy block/unblock
		sfs.Send(new BlockBuddyRequest(buddyName, !isBlocked));
	}

	/**
	 * On Remove buddy button click in Buddy List Item prefab instance, send request to remove buddy from user's Buddy List.
	 */
	public void OnRemoveBuddyButtonClick(string buddyName)
	{
		// Request buddy removal from buddy list
		sfs.Send(new RemoveBuddyRequest(buddyName));
	}

	/**
	 * On Chat button click in Buddy List Item prefab instance, initialize and show chat panel.
	 */
	public void OnChatBuddyButtonClick(string buddyName)
	{
		// Skip if chat is already active with the same buddy
		if (chatPanel.BuddyName != buddyName)
		{
			// Get chat history (if any) and reset new messages counter
			List<BuddyChatMessage> buddyChatMessages = BuddyChatHistoryManager.GetBuddyChatMessages(buddyName);
			BuddyChatHistoryManager.ResetUnreadMessagesCount(buddyName);

			// Initialize and show panel
			chatPanel.Init(sfs.BuddyManager.GetBuddyByName(buddyName), buddyChatMessages);

			// Move buddy list item to top of the list and reset new messages counter
			buddyListItems.TryGetValue(buddyName, out BuddyListItem buddyListItem);

			if (buddyListItem != null)
			{
				buddyListItem.gameObject.transform.SetAsFirstSibling();
				buddyListItem.SetChatMsgCounter(0);
			}
		}
	}

	/**
	 * On Invite button click in Buddy List Item prefab instance, start new game and invite buddy.
	 */
	public void OnInviteBuddyButtonClick(string buddyName)
	{
		OnStartGameConfirm(false, buddyName);
	}

	/**
	 * On custom event fired by Buddy Chat Panel prefab instance, send message to buddy.
	 */
	public void OnBuddyMessageSubmit(string buddyName, string message)
	{
		// Add a custom parameter containing the recipient name
		ISFSObject _params = new SFSObject();
		_params.PutUtfString("recipient", buddyName);

		// Retrieve buddy
		Buddy buddy = sfs.BuddyManager.GetBuddyByName(buddyName);

		// Send message to buddy
		sfs.Send(new BuddyMessageRequest(message, buddy, _params));
	}

	/**
	 * On custom event fired by User Profile Panel prefab instance, toggle user online state in Buddy List system.
	 */
	public void OnOnlineToggleChange(bool isChecked)
	{
		// Send requesto to toggle online/offline state
		sfs.Send(new GoOnlineRequest(isChecked));
	}

	/**
	 * On custom event fired by User Profile Panel prefab instance, set user's Buddy Variables.
	 */
	public void OnBuddyDetailChange(string varName, object value)
	{
		List<BuddyVariable> buddyVars = new List<BuddyVariable>();
		buddyVars.Add(new SFSBuddyVariable(varName, value));

		// Set Buddy Variables
		sfs.Send(new SetBuddyVariablesRequest(buddyVars));
	}

	/**
	 * On custom event fired by User Profile Panel prefab instance, set user's User Variables.
	 */
	public void OnPlayerDetailChange(string varName, object value)
	{
		List<UserVariable> userVars = new List<UserVariable>();
		userVars.Add(new SFSUserVariable(varName, value));
		
		// Set User Variables
		sfs.Send(new SetUserVariablesRequest(userVars));
	}

	/**
	 * On custom event fired by Invitation Panel prefab instance, send reply to invitation.
	 */
	public void OnInvitationReplyClick(Invitation invitation, bool accept)
	{
		// Accept/refuse invitation
		sfs.Send(new InvitationReplyRequest(invitation, (accept ? InvitationReply.ACCEPT : InvitationReply.REFUSE)));

		// If invitation was accepted, refuse all remaining invitations in the queue
		if (accept)
		{
			// Refuse other invitations
			foreach (InvitationWrapper iw in invitationQueue)
				sfs.Send(new InvitationReplyRequest(iw.invitation, InvitationReply.REFUSE));

			// Reset queue
			invitationQueue.Clear();
		}

		// If invitation was refused, process next invitation in the queue (if any)
		else
			ProcessInvitations();
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
		sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
		sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
		sfs.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
		sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);

		sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate);

		sfs.AddEventListener(SFSBuddyEvent.BUDDY_LIST_INIT, OnBuddyListInit);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_ERROR, OnBuddyError);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_ONLINE_STATE_UPDATE, OnBuddyOnlineStateUpdate);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_VARIABLES_UPDATE, OnBuddyVariablesUpdate);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_BLOCK, OnBuddyBlock);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_REMOVE, OnBuddyRemove);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyAdd);
		sfs.AddEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnBuddyMessage);

		sfs.AddEventListener(SFSEvent.INVITATION, OnInvitation);
	}

	/**
	 * Remove all SmartFoxServer-related event listeners added by the scene.
	 * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
	 */
	override protected void RemoveSmartFoxListeners()
	{
		sfs.RemoveEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
		sfs.RemoveEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
		sfs.RemoveEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
		sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
		sfs.RemoveEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
		sfs.RemoveEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);

		sfs.RemoveEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate);

		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_LIST_INIT, OnBuddyListInit);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_ERROR, OnBuddyError);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_ONLINE_STATE_UPDATE, OnBuddyOnlineStateUpdate);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_VARIABLES_UPDATE, OnBuddyVariablesUpdate);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_BLOCK, OnBuddyBlock);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_REMOVE, OnBuddyRemove);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyAdd);
		sfs.RemoveEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnBuddyMessage);

		sfs.RemoveEventListener(SFSEvent.INVITATION, OnInvitation);
	}

	/**
	 * Hide all modal panels.
	 */
	override protected void HideModals()
	{
		userProfilePanel.Hide();
		startGamePanel.Hide();
		invitationPanel.Hide();
		warningPanel.Hide();
	}

	/**
	 * Initialize player profile.
	 * 
	 * IMPORTANT NOTE
	 * The Experience and Ranking details are custom parameters used in this example to show how to filter players through SmartFoxServer's matchmaking system
	 * when sending invitations to play a game. Of course they (or any other parameter with a similar purpose) shouldn't be set manually, but they should be
	 * determined by the game logic, saved in a database and set in User Variables by a server-side Extension upon user login.
	 * In this example, for sake of simplicity, such details are set to default values when the Lobby scene is loaded and they are lost upon user logout.
	 */
	private void InitPlayerProfile()
	{
		// Check if player details are set in User Variables
		if (sfs.MySelf.GetVariable(USERVAR_EXPERIENCE) == null || sfs.MySelf.GetVariable(USERVAR_RANKING) == null)
		{
			// If not, set player profile default values; this in turn causes a UserVariablesUpdate event to be fired,
			// upon which we will display the player details in the user profile panel
			SFSUserVariable expVar = new SFSUserVariable(USERVAR_EXPERIENCE, "Novice");
			SFSUserVariable rankVar = new SFSUserVariable(USERVAR_RANKING, 3);

			sfs.Send(new SetUserVariablesRequest(new List<UserVariable>() { expVar, rankVar }));
		}
		else
		{
			// If yes, display player details in user profile panel
			userProfilePanel.InitPlayerProfile(sfs.MySelf);
		}
	}

	/**
	 * Display list of existing games.
	 */
	private void PopulateGamesList()
	{
		// Initialize list
		if (gameListItems == null)
			gameListItems = new Dictionary<int, GameListItem>();

		// For the game list we use a scrollable area containing a separate prefab for each Game Room
		// The prefab contains clickable buttons to join the game
		List<Room> rooms = sfs.RoomManager.GetRoomList();

		// Display game list items
		foreach (Room room in rooms)
			AddGameListItem(room);
	}

	/**
	 * Create Game List Item prefab instance and add to games list.
	 */
	private void AddGameListItem(Room room)
	{
		// Show only game rooms
		// Also password protected Rooms are skipped, to make this example simpler
		// (protection would require an interface element to input the password)
		if (!room.IsGame || room.IsHidden || room.IsPasswordProtected)
			return;

		// Create game list item
		GameListItem gameListItem = Instantiate(gameListItemPrefab);
		gameListItems.Add(room.Id, gameListItem);

		// Init game list item
		gameListItem.Init(room);

		// Add listener to play and watch buttons
		gameListItem.playButton.onClick.AddListener(() => OnGameItemPlayClick(room.Id));
		gameListItem.watchButton.onClick.AddListener(() => OnGameItemWatchClick(room.Id));

		// Add game list item to container
		gameListItem.gameObject.transform.SetParent(gameListContent, false);
	}

	/**
	 * Show current user state in Buddy List system.
	 */
	private void DisplayUserStateAsBuddy()
	{
		if (sfs.BuddyManager.MyOnlineState)
			userStatusLabel.text = sfs.BuddyManager.MyState;
		else
			userStatusLabel.text = "Offline";
	}

	/**
	 * Initialize buddy-related entities.
	 */
	private void InitializeBuddyClient()
	{
		// Init buddy-related data structures
		buddyListItems = new Dictionary<string, BuddyListItem>();

		// For the buddy list we use a scrollable area containing a separate prefab for each buddy
		// The prefab contains clickable buttons to invite a buddy to play, chat with them, block or remove them
		List<Buddy> buddies = sfs.BuddyManager.BuddyList;

		// Display buddy list items
		// All blocked buddies are displayed at the bottom of the list
		foreach (Buddy buddy in buddies)
			AddBuddyListItem(buddy, !buddy.IsBlocked);

		// Set current user details in buddy system
		userProfilePanel.InitBuddyProfile(sfs.BuddyManager);

		// Display user state in buddy list system
		DisplayUserStateAsBuddy();

		// Show/hide buddy list
		buddyListPanel.SetActive(sfs.BuddyManager.MyOnlineState);
	}

	/**
	 * Create Buddy List Item prefab instance and add to buddy list.
	 */
	private void AddBuddyListItem(Buddy buddy, bool toTop = false)
	{
		string buddyName = buddy.Name;

		// Check if buddy list item already exist
		// This could happen if a temporary buddy is added permanently
		if (buddyListItems.ContainsKey(buddyName))
		{
			BuddyListItem buddyListItem = buddyListItems[buddyName];
			buddyListItem.SetState(buddy);
		}
		else
		{
			// Create buddy list item
			BuddyListItem buddyListItem = Instantiate(buddyListItemPrefab);
			buddyListItems.Add(buddyName, buddyListItem);

			// Init buddy list item
			buddyListItem.Init(buddy);

			// Set unread messages counter
			// (buddy could have sent messages while user was in the game scene)
			buddyListItem.SetChatMsgCounter(BuddyChatHistoryManager.GetUnreadMessagesCount(buddy.Name));

			// Add listeners to buttons
			buddyListItem.removeButton.onClick.AddListener(() => OnRemoveBuddyButtonClick(buddyName));
			buddyListItem.addButton.onClick.AddListener(() => OnAddBuddyButtonClick(buddyName));
			buddyListItem.blockButton.onClick.AddListener(() => OnBlockBuddyButtonClick(buddyName));
			buddyListItem.chatButton.onClick.AddListener(() => OnChatBuddyButtonClick(buddyName));
			buddyListItem.inviteButton.onClick.AddListener(() => OnInviteBuddyButtonClick(buddyName));

			// Add buddy list item to container
			buddyListItem.gameObject.transform.SetParent(buddyListContent, false);

			if (toTop)
				buddyListItem.gameObject.transform.SetAsFirstSibling();

			// Add buddy to invite list in Start game panel
			startGamePanel.UpdateInviteList(buddy);
		}
	}

	/**
	 * Update Buddy List Item prefab instance when buddy state changes.
	 */
	private void UpdateBuddyListItem(Buddy buddy)
	{
		// Get reference to buddy list item corresponding to Buddy
		buddyListItems.TryGetValue(buddy.Name, out BuddyListItem buddyListItem);

		if (buddyListItem != null)
		{
			// Check if the update was cause by a block/unblock request
			if (buddy.IsBlocked != buddyListItem.isBlocked)
			{
				// Move buddy to the bottom of the list
				if (buddy.IsBlocked)
					buddyListItem.gameObject.transform.SetAsLastSibling();
				else
					buddyListItem.gameObject.transform.SetAsFirstSibling();
			}

			// Update buddy list item
			buddyListItem.SetState(buddy);

			// Update buddy chat panel
			if (chatPanel.BuddyName == buddy.Name)
				chatPanel.SetState(buddy);

			// Update buddy in invite list in Start game panel
			startGamePanel.UpdateInviteList(buddy);
		}
	}

	/**
	 * Process the invitation in queue, displaying the invitation accept/refuse panel.
	 */
	private void ProcessInvitations()
	{
		// If the invitation panel is visible, then the user is already dealing with an invitation
		// Otherwise we can go on with the processing
		if (!invitationPanel.IsVisible)
		{
			while (invitationQueue.Count > 0)
			{
				// Get next invitation in queue
				InvitationWrapper iw = invitationQueue.Dequeue();

				// Evaluate remaining time for replying
				DateTime now = DateTime.Now;
				TimeSpan ts = now - iw.date;

				// Update expiration time
				iw.expiresInSeconds -= (int)Math.Floor(ts.TotalSeconds);

				// Display invitation only if expiration will occur in 3 seconds or more, otherwise discard it
				if (iw.expiresInSeconds >= 3)
				{
					invitationPanel.Show(iw);
					break;
				}
			}
		}
	}
	#endregion

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------
	#region
	private void OnRoomAdded(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];

		// Display game list item
		AddGameListItem(room);
	}

	public void OnRoomRemoved(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];

		// Get reference to game list item corresponding to Room
		gameListItems.TryGetValue(room.Id, out GameListItem gameListItem);

		// Remove game list item
		if (gameListItem != null)
		{
			// Remove listeners
			gameListItem.playButton.onClick.RemoveAllListeners();
			gameListItem.watchButton.onClick.RemoveAllListeners();

			// Remove game list item from dictionary
			gameListItems.Remove(room.Id);

			// Destroy game object
			GameObject.Destroy(gameListItem.gameObject);
		}
	}

	public void OnUserCountChanged(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];

		// Get reference to game list item corresponding to Room
		gameListItems.TryGetValue(room.Id, out GameListItem gameListItem);

		// Update game list item
		if (gameListItem != null)
			gameListItem.SetState(room);
	}

	private void OnRoomJoin(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];

		// If a game Room was joined, go to the Game scene, otherwise ignore this event
		if (room.IsGame)
		{
			// Set user as "Away" in Buddy List system
			if (sfs.BuddyManager.MyOnlineState)
				sfs.Send(new SetBuddyVariablesRequest(new List<BuddyVariable> { new SFSBuddyVariable(ReservedBuddyVariables.BV_STATE, "Away") }));

			// Load game scene
			SceneManager.LoadScene("Game");
		}
	}

	private void OnRoomJoinError(BaseEvent evt)
	{
		// Show Warning Panel prefab instance
		warningPanel.Show("Room join failed: " + (string)evt.Params["errorMessage"]);
	}

	private void OnRoomCreationError(BaseEvent evt)
	{
		// Show Warning Panel prefab instance
		warningPanel.Show("Room creation failed: " + (string)evt.Params["errorMessage"]);
	}

	private void OnBuddyListInit(BaseEvent evt)
	{
		// Initialize buddy-related entities
		InitializeBuddyClient();
	}

	private void OnBuddyError(BaseEvent evt)
	{
		// Show Warning Panel prefab instance
		warningPanel.Show("Buddy list system error: " + (string)evt.Params["errorMessage"]);
	}

	public void OnBuddyOnlineStateUpdate(BaseEvent evt)
	{
		Buddy buddy = (Buddy)evt.Params["buddy"];
		bool isItMe = (bool)evt.Params["isItMe"];

		// As this event is fired in case of online state update for both the current user and their buddies,
		// we have to check who the event refers to

		if (!isItMe)
			UpdateBuddyListItem(buddy);
		else
		{
			DisplayUserStateAsBuddy();

			// Show/hide buddy list
			buddyListPanel.SetActive(sfs.BuddyManager.MyOnlineState);

			if (!sfs.BuddyManager.MyOnlineState)
			{
				// Hide chat if current user went offline
				chatPanel.Hide();
			}
			else
			{
				// Update all buddy items if current user came online
				foreach (Buddy b in sfs.BuddyManager.BuddyList)
					UpdateBuddyListItem(b);
			}
		}
	}

	public void OnBuddyVariablesUpdate(BaseEvent evt)
	{
		Buddy buddy = (Buddy)evt.Params["buddy"];
		bool isItMe = (bool)evt.Params["isItMe"];

		// As this event is fired in case of Buddy Variables update for both the current user and their buddies,
		// we have to check who the event refers to

		if (!isItMe)
			UpdateBuddyListItem(buddy);
		else
			DisplayUserStateAsBuddy();
	}

	public void OnBuddyBlock(BaseEvent evt)
	{
		Buddy buddy = (Buddy)evt.Params["buddy"];

		UpdateBuddyListItem(buddy);
	}

	public void OnBuddyRemove(BaseEvent evt)
	{
		Buddy buddy = (Buddy)evt.Params["buddy"];

		// Get reference to buddy list item corresponding to Buddy
		buddyListItems.TryGetValue(buddy.Name, out BuddyListItem buddyListItem);

		// Remove buddy list item
		if (buddyListItem != null)
		{
			// Remove listeners
			buddyListItem.chatButton.onClick.RemoveAllListeners();
			buddyListItem.inviteButton.onClick.RemoveAllListeners();
			buddyListItem.blockButton.onClick.RemoveAllListeners();
			buddyListItem.removeButton.onClick.RemoveAllListeners();

			// Remove buddy list item from dictionary
			buddyListItems.Remove(buddy.Name);

			// Destroy game object
			GameObject.Destroy(buddyListItem.gameObject);

			// Block chat interaction
			if (chatPanel.BuddyName == buddy.Name)
				chatPanel.Hide();

			// Remove buddy from invite list in Start game panel
			startGamePanel.UpdateInviteList(buddy, true);
		}
	}

	public void OnBuddyAdd(BaseEvent evt)
	{
		Buddy buddy = (Buddy)evt.Params["buddy"];

		// Add buddy list item at the top of the list
		AddBuddyListItem(buddy, true);
	}

	public void OnBuddyMessage(BaseEvent evt)
	{
		// Add message to queue
		BuddyChatMessage chatMsg = BuddyChatHistoryManager.AddMessage(evt.Params);

		// Show message or increase message counter
		if (chatPanel.BuddyName == chatMsg.buddyName)
		{
			// Display message in chat panel
			chatPanel.PrintChatMessage(chatMsg);
		}
		else
		{
			// Increase unread messages count
			// NOTE: there's no need to make sure the sender is not the current user,
			// as in this example the current user can't send buddy messages in other ways than
			// the chat panel (which would make the above if statement true)
			int unreadMsgCnt = BuddyChatHistoryManager.IncreaseUnreadMessagesCount(chatMsg.buddyName);

			// Update buddy list item
			BuddyListItem buddyListItem = buddyListItems[chatMsg.buddyName];
			buddyListItem.gameObject.transform.SetAsFirstSibling();
			buddyListItem.SetChatMsgCounter(unreadMsgCnt);
		}
	}

	public void OnUserVariablesUpdate(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];

		// Display player details in user profile panel
		if (user.IsItMe)
			userProfilePanel.InitPlayerProfile(user);
	}

	public void OnInvitation(BaseEvent evt)
	{
		Invitation invitation = (Invitation)evt.Params["invitation"];

		// Add invitation wrapper to queue
		// We use a quue because a user could receive multiple invitations within the time it takes to accept or refuse one
		if (invitationQueue == null)
			invitationQueue = new Queue<InvitationWrapper>();

		invitationQueue.Enqueue(new InvitationWrapper(invitation));

		// Trigger invitation processing
		ProcessInvitations();
	}
	#endregion
}
