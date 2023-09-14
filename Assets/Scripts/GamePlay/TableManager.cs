using System.Collections;
using System.Collections.Generic;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.WebSocketSharp;
using TinAungKhant.UIManagement;
using UnityEngine;
using UnityEngine.UI;

public class TableManager : MonoBehaviour
{
    [SerializeField] private DeckHandler _deckHandler;
    [SerializeField] private ClientCardsManager _clientCardsManager;
    [SerializeField] private TableUIHandler _tableUIHandler;
    [SerializeField] private MatchInfoManager _infoManager;
    

    [SerializeField] private Button _drawCardBtn;

    private List<User> currentUsers;
    private List<User> unexplodePlayers;

    public void Init()
    {
        _infoManager.Init();
        _drawCardBtn.onClick.AddListener(DrawCard);
        _tableUIHandler.Init();
        SpawnPlayers();
        //GetCurrentTurnParticipant();
    }

    public void ListenSfsEvents()
    {
        //GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnRandomCardRes);
        GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        GetExistingCards();
    }

    public void RemoveSfsEvents()
    {
        //GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnRandomCardRes);
        GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

    }

    public void OnExtensionResponse(BaseEvent evt)
    {
        string cmdName = (string)evt.Params[ExtensionEventNames.CMD];
        ISFSObject responseData = (SFSObject)evt.Params[ExtensionEventNames.PARAMS];

        switch (cmdName)
        {
            case ExtensionEventNames.A_PLAYER_DRAW_CARD:
                OnAPlayerDrawCard(responseData);
                break;

            case ExtensionEventNames.SELF_DRAW_CARD:
                OnSelfDrawCard(responseData);
                break;

            case ExtensionEventNames.PARTICIPANT_CHANGED:
                OnParticipantTurnChanged(responseData);
                break;

            case ExtensionEventNames.CURRENT_TURN_PARTICIPANT:
                OnGetCurrentTurnParticipant(responseData);
                break;

            case ExtensionEventNames.A_PLAYER_GOT_EXPLODE_CARD:
                OnAPlayerDrawExplodeCard(responseData);
                break;

            case ExtensionEventNames.PLAYER_EXPLODED:
                OnAPlayerExplode(responseData);
                break;

            case ExtensionEventNames.GOT_EXPLODE_CARD:
                OnSelfDrawExplodeCard(responseData);
                break;

            case ExtensionEventNames.DEFUSE_SUCCESS:
                OnDefuseSuccess(responseData);
                break;

            case ExtensionEventNames.PUT_CARD_ON_DECK_SUCCESS:
                OnPutCardBackSuccess(responseData);
                break;

            case ExtensionEventNames.EXISTING_CARDS_RESPONSE:
                OnGetExistingCard(responseData);
                break;

            case ExtensionEventNames.GAME_OVER:
                OnGameOver(responseData);
                break;

            default:
                break;
        }
    }

    public void SpawnPlayers()
    {
        currentUsers = GlobalManager.Instance.GetSfsClient().LastJoinedRoom.UserList;
        unexplodePlayers = currentUsers;

        _tableUIHandler.SpawnPlayers(currentUsers);
    }

    public void DrawCard()
    {
        ExtensionRequest extensionRequest = new ExtensionRequest(ExtensionEventNames.DRAW_CARD, new SFSObject());
        GlobalManager.Instance.GetSfsClient().Send(extensionRequest);

        _drawCardBtn.interactable = false;
    }

    public void GetExistingCards()
    {
        ExtensionRequest extensionRequest = new ExtensionRequest(ExtensionEventNames.GET_EXISTING_CARDS, new SFSObject());
        GlobalManager.Instance.GetSfsClient().Send(extensionRequest);
    }

    public void GetCurrentTurnParticipant()
    {
        ExtensionRequest extensionRequest = new ExtensionRequest(ExtensionEventNames.GET_CURRENT_TURN_PARTICIPANT, new SFSObject());
        GlobalManager.Instance.GetSfsClient().Send(extensionRequest);
    }

    //this will invoke whenever a player drew a card. Drawer won't receive this event
    public void OnAPlayerDrawCard(ISFSObject responseData)
    {
        Debug.Log(responseData.GetUtfString(ExtensionEventNames.MESSAGE));
        int cardLeftOnDeck = responseData.GetInt(ExtensionEventNames.CARD_LEFT_ON_DECK);
        string drawerName = responseData.GetUtfString(ExtensionEventNames.DRAWER);

        _deckHandler.SetDeckAmount(cardLeftOnDeck);
        _tableUIHandler.OnPlayerDrawCard(drawerName);
    }

    //this will only invoke when you draw a card. Only the drawer can receive the event
    public void OnSelfDrawCard(ISFSObject responseData)
    {
        string drewCardName = responseData.GetUtfString(ExtensionEventNames.DREW_CARD);
        int cardTypeID = responseData.GetInt(ExtensionEventNames.CARD_TYPE_ID);
        int cardLeftOnDeck = responseData.GetInt(ExtensionEventNames.CARD_LEFT_ON_DECK);

        _deckHandler.SetDeckAmount(cardLeftOnDeck);
        _clientCardsManager.AddCard(drewCardName, cardTypeID);
    }

    //this will invoke whenever a player drew an explode card. Drawer won't receive this event
    public void OnAPlayerDrawExplodeCard(ISFSObject responseData)
    {
        Debug.Log(responseData.GetUtfString(ExtensionEventNames.MESSAGE));
        int cardLeftOnDeck = responseData.GetInt(ExtensionEventNames.CARD_LEFT_ON_DECK);
        string drawerName = responseData.GetUtfString(ExtensionEventNames.DRAWER);
        string drewCardName = responseData.GetUtfString(ExtensionEventNames.DREW_CARD);
        int cardTypeID = responseData.GetInt(ExtensionEventNames.CARD_TYPE_ID);

        _deckHandler.SetDeckAmount(cardLeftOnDeck);
        _tableUIHandler.OnPlayerGotExplodeCard(drawerName);
    }

    //this will only invoke when you draw an explode card. Only the drawer can receive the event
    public void OnSelfDrawExplodeCard(ISFSObject responseData)
    {
        string drewCardName = responseData.GetUtfString(ExtensionEventNames.DREW_CARD);
        int cardTypeID = responseData.GetInt(ExtensionEventNames.CARD_TYPE_ID);
        int cardLeftOnDeck = responseData.GetInt(ExtensionEventNames.CARD_LEFT_ON_DECK);

        _deckHandler.SetDeckAmount(cardLeftOnDeck);
        _tableUIHandler.ShowExplodeWarningPanel();
    }

    //this will invoke when a player exploded
    public void OnAPlayerExplode(ISFSObject responseData)
    {
        string decisionMaker = responseData.GetUtfString(ExtensionEventNames.DECISION_MAKER);
        Debug.Log(responseData.GetUtfString(ExtensionEventNames.DECISION_MAKER) + " is exploded..");

        _tableUIHandler.OnAPlayerExplode(unexplodePlayers, decisionMaker);

        unexplodePlayers.Remove(unexplodePlayers.Find(user => user.Name == decisionMaker));
    }

    public void OnDefuseSuccess(ISFSObject responseData)
    {
        string message = responseData.GetUtfString(ExtensionEventNames.MESSAGE);
        string defusePlayer = responseData.GetUtfString(ExtensionEventNames.DEFUSE_PLAYER);

        _infoManager.UpdateMessage(message);


        _tableUIHandler.OnPlayerDefuseSuccess(defusePlayer);
    }

    public void OnPutCardBackSuccess(ISFSObject responseData)
    {
        int cardLeftOnDeck = responseData.GetInt(ExtensionEventNames.CARD_LEFT_ON_DECK);

        _deckHandler.SetDeckAmount(cardLeftOnDeck);
        _tableUIHandler.HidePutExplodePanel();
    }

    //this will invoke whenever turn changed
    public void OnParticipantTurnChanged(ISFSObject responseData)
    {

        string previousPlayerName = string.Empty;
        string currentPlayerName = responseData.GetUtfString(ExtensionEventNames.CURRENT_TURN_USER_NAME);

        if (responseData.ContainsKey(ExtensionEventNames.PREVIOUS_TURN_USER_NAME))
        {
            previousPlayerName = responseData.GetUtfString(ExtensionEventNames.PREVIOUS_TURN_USER_NAME);

            _tableUIHandler.OnTurnChanged(previousPlayerName, currentPlayerName);
        }
        else
        {
            _tableUIHandler.OnTurnChanged(null, currentPlayerName);
        }

        _infoManager.ShowPlayerTurn(currentPlayerName);
        Debug.Log("Current turn is : " + responseData.GetUtfString(ExtensionEventNames.CURRENT_TURN_USER_NAME));
    }

    //this will invoke after we request the current turn participant
    public void OnGetCurrentTurnParticipant(ISFSObject responseData)
    {
        Debug.Log("Current turn is : " + responseData.GetUtfString(ExtensionEventNames.CURRENT_TURN_USER_NAME));
    }

    //this will invoke when client request existing cards
    public void OnGetExistingCard(ISFSObject responseData)
    {
        ISFSArray existingCardsArray = responseData.GetSFSArray(ExtensionEventNames.EXISTING_CARDS);

        for (int i = 0; i < existingCardsArray.Size(); i++)
        {
            ISFSObject cardObject = existingCardsArray.GetSFSObject(i);
            string cardName = cardObject.GetUtfString(ExtensionEventNames.CARD_NAME);
            int cardTypeID = cardObject.GetInt(ExtensionEventNames.CARD_TYPE_ID);

            Debug.Log("Received existing card: " + cardName);
            _clientCardsManager.AddCard(cardName, cardTypeID);
        }
    }

    public void OnGameOver(ISFSObject responseData)
    {
        string winnerName = responseData.GetUtfString(ExtensionEventNames.WINNER);
        int roomID = responseData.GetInt(ExtensionEventNames.ROOM_ID);

        ResultData resultData = new ResultData(winnerName, roomID);

        UIManager.Instance.ShowUI(GLOBALCONST.UI_RESULT_PANEL, resultData);
    }

    public List<User> getCurrentUsers()
    {
        return currentUsers;
    }
}
