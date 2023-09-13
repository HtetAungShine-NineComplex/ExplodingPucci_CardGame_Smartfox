using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Managers;
using Sfs2X.Requests;

/**
 * Script attached to the TicTacToe Game object in the Game scene.
 */
public class TicTacToeGameManager : MonoBehaviour
{
	public PlayerTag[] playerTags;
    public GameBoard board;
	public Text stateText;
	public Button restartButton;
    public Button joinButton;

	public UnityEvent<string> onAddBuddyClick;

	private SmartFox sfs;
	private IBuddyManager buddyMan;
	private State state;
	private int whoseTurn;
	private int lastWinner;

	private enum State
	{
		WAITING_FOR_PLAYERS = 0,
		RUNNING,
		GAME_WON,
		GAME_LOST,
		GAME_TIE,
		INTERRUPTED
	};

	//----------------------------------------------------------
	// Public methods
	//----------------------------------------------------------

	public void Init(SmartFox sfs)
	{
		this.sfs = sfs;
		this.buddyMan = sfs.BuddyManager;

		// Add SmartFoxServer-related event listeners required by this game
		sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
		sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);

		// Add listeners to "Add buddy" buttons in player tags
		// A custom event is fired so the main controller can send the appropriate request
		playerTags[1].addBuddyButton.onClick.AddListener(() => onAddBuddyClick.Invoke(playerTags[1].playerName.text));
		playerTags[2].addBuddyButton.onClick.AddListener(() => onAddBuddyClick.Invoke(playerTags[2].playerName.text));

		// Hide player tags
		playerTags[1].gameObject.SetActive(false);
		playerTags[2].gameObject.SetActive(false);

		// Display player tags
		UpdatePlayerTags();

		// Set initial state
		state = State.WAITING_FOR_PLAYERS;

		// Display game state
		UpdateGameState();

		// Tell Room Extension that user is ready
		sfs.Send(new ExtensionRequest("ready", new SFSObject(), sfs.LastJoinedRoom));
	}

    public void Destroy()
    {
        // Remove SmartFoxServer-related event listeners added by this game
        sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
		sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.RemoveEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);

		sfs = null;
	}

	/**
	 * On Restart button click, start new game.
	 */
	public void OnRestartButtonClick()
	{
		// Send request to Extension
		sfs.Send(new ExtensionRequest("restart", new SFSObject(), sfs.LastJoinedRoom));
	}

	/**
	 * On Join button click, turn current user from spectator to player.
	 */
	public void OnJoinButtonClick()
	{
		// Send request to server
		sfs.Send(new SpectatorToPlayerRequest());
	}

	/**
	 * Listen to custom click event dispatched by GameBoard instance.
	 */
	public void OnBoardSlotClick(int row, int col)
	{
		// Only player in turn is allowed to interact with the board when the game is running
		if (state == State.RUNNING && sfs.MySelf.PlayerId == whoseTurn)
		{
			ISFSObject move = new SFSObject();
			move.PutInt("r", row);
			move.PutInt("c", col);

			// Send move to Extension
			sfs.Send(new ExtensionRequest("move", move, sfs.LastJoinedRoom));
		}
	}

	//----------------------------------------------------------
	// Private methods
	//----------------------------------------------------------

	/**
	 * Display current game state.
	 */
	private void UpdateGameState()
	{
		// Hide buttons
		restartButton.gameObject.SetActive(false);
		joinButton.gameObject.SetActive(false);

		if (state == State.WAITING_FOR_PLAYERS)
		{
			if (sfs.MySelf.IsPlayer)
				stateText.text = "Waiting for your opponent";
			else
			{
				if (sfs.LastJoinedRoom.PlayerList.Count < 2)
				{
					stateText.text = "Waiting for players; do you want to join?";

					// Show Join button
					joinButton.gameObject.SetActive(true);
				}
			}
		}

		else if (state == State.INTERRUPTED)
		{
			if (sfs.MySelf.IsPlayer)
			{
				stateText.text = "Your opponent left the game";

				// If the current Room is private, no other player will ever join it
				// If spectators could be invited in a private game, we should also check that no one is in the Room
				if (sfs.LastJoinedRoom.IsPasswordProtected)
					stateText.text += "; you should leave too";
				else
					stateText.text += "; waiting for a new player";
			}
			else
			{
				stateText.text = "Game interrupted due to missing player/s; do you want to join?";

				// Show Join button
				joinButton.gameObject.SetActive(true);
			}
		}

		else if (state == State.RUNNING)
		{
			if (sfs.MySelf.IsPlayer)
				stateText.text = (sfs.MySelf.PlayerId == whoseTurn ? "It's your turn" : "It's your opponent's turn");
			else
				stateText.text = string.Format("It's player {0} turn", whoseTurn);
		}

		else
		{
			stateText.text = "GAME OVER";

			if (sfs.MySelf.IsPlayer)
			{
				switch (state)
				{
					case State.GAME_WON:
						stateText.text += "\nYou are the winner!";
						break;

					case State.GAME_LOST:
						stateText.text += "\nYou've lost!";
						break;

					case State.GAME_TIE:
						stateText.text += "\nIt's a tie!";
						break;
				}

				// Show restart button
				restartButton.gameObject.SetActive(true);
			}
			else
			{
				switch (state)
				{
					case State.GAME_TIE:
						stateText.text += "\nIt's a tie!";
						break;

					default:
						stateText.text += string.Format("\nPlayer {0} is the winner!", lastWinner);
						break;
				}
			}
		}
	}

	/**
	 * Start the game.
	 */
	private void StartGame(int whoseTurn, int p1Wins, int p2Wins)
	{
		this.whoseTurn = whoseTurn;

		// Reset game board
		board.Reset();

		// Set game state
		state = State.RUNNING;

		// Display game state
		UpdateGameState();

		// Display player wins
		playerTags[1].Wins = p1Wins;
		playerTags[2].Wins = p2Wins;

		// Enable game board
		if (sfs.MySelf.IsPlayer)
			board.IsEnabled = true;
	}

	/**
	 * Stop the game.
	 */
	private void StopGame()
	{
		// Set game state
		state = State.INTERRUPTED;

		// Display game state
		UpdateGameState();

		// Disable game board
		board.IsEnabled = false;
	}

	/**
	 * Spectator receives board update. If match isn't started yet,
	 * a message is displayed and he can click the join button
	 */
	private void SetSpectatorBoard(int whoseTurn, bool gameRunning, ISFSArray boardData, int p1Wins, int p2Wins, int lastWinner)
	{
		this.whoseTurn = whoseTurn;

		// Set game state
		if (gameRunning)
			state = State.RUNNING;

		// Display game state
		UpdateGameState();

		// Display player wins
		playerTags[1].Wins = p1Wins;
		playerTags[2].Wins = p2Wins;

		// Update board
		// NOTE: the board data is a SFSArray (the board rows) containing three other SFSArrays (the columns)
		// As SFSArrays are 0-based, we have to convert to board coordinates
		for (int i = 0; i < 3; i++)
		{
			int[] values = boardData.GetIntArray(i);

			for (int j = 0; j < 3; j++)
			{
				int row = i + 1;
				int col = j + 1;

				board.SetMark(row, col, values[j]);
			}
		}

		if (lastWinner > -1)
			DeclareWinner(lastWinner);
	}

	/**
	 * On move received, add mark to game board.
	 */
	private void ExecuteMove(int playerId, int row, int col)
	{
		// Set new turn
		whoseTurn = (playerId == 1) ? 2 : 1;

		// Display game state
		UpdateGameState();

		// Show mark on game board
		board.SetMark(row, col, playerId);
	}

	/**
	 * Declare game winner.
	 */
	private void DeclareWinner(int winnerId)
	{
		lastWinner = winnerId;

		// Set game state
		if (winnerId > 0)
		{
			// Current user is the winner
			if (sfs.MySelf.PlayerId == winnerId)
				state = State.GAME_WON;

			// Opponent is the winner
			else
				state = State.GAME_LOST;

			// Update player wins
			playerTags[winnerId].Wins++;
		}
		else
			state = State.GAME_TIE;

		// Display game state
		UpdateGameState();

		// Disable game board
		board.IsEnabled = false;
	}

	/**
	 * Display current players tags.
	 */
	public void UpdatePlayerTags()
	{
		foreach (User user in sfs.LastJoinedRoom.PlayerList)
		{
			int i = user.PlayerId;

			playerTags[i].gameObject.SetActive(true);
			playerTags[i].label.text = "Player " + i;
			playerTags[i].playerName.text = user.Name;
			playerTags[i].Wins = playerTags[i].Wins;
			playerTags[i].addBuddyButton.gameObject.SetActive(user != sfs.MySelf && (buddyMan.GetBuddyByName(user.Name) == null || buddyMan.GetBuddyByName(user.Name).IsTemp));
		}
	}

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------

	private void OnUserEnterRoom(BaseEvent evt)
	{
		UpdatePlayerTags();
	}

	private void OnUserExitRoom(BaseEvent evt)
	{
		User user = (User)evt.Params["user"];
		
		if (user != sfs.MySelf && user.IsPlayer)
		{
			// Hide player panel
			playerTags[user.PlayerId].gameObject.SetActive(false);
		}
	}

	private void OnSpectatorToPlayer(BaseEvent evt)
	{
		UpdateGameState();
		UpdatePlayerTags();
	}

	private void OnExtensionResponse(BaseEvent evt)
	{
		string cmd = (string)evt.Params["cmd"];
		ISFSObject data = (SFSObject)evt.Params["params"];
		
		switch (cmd)
		{
			case "start":
				StartGame(data.GetInt("t"), data.GetInt("p1w"), data.GetInt("p2w"));
				break;

			case "state":
				SetSpectatorBoard(data.GetInt("t"), data.GetBool("run"), data.GetSFSArray("board"), data.GetInt("p1w"), data.GetInt("p2w"), data.ContainsKey("w") ? data.GetInt("w") : -1);
				break;

			case "stop":
				StopGame();
				break;

			case "move":
				ExecuteMove(data.GetInt("t"), data.GetInt("r"), data.GetInt("c"));
				break;

			case "over":
				DeclareWinner(data.GetInt("w"));
				break;
		}
	}
}
