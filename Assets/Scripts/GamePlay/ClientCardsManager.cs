using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using UnityEngine;

public class ClientCardsManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerCardPrefab;
    [SerializeField] private GameObject _playerDrawAnimObj;
    [SerializeField] private Transform _rootContent;

    private List<Card> _playerCards = new List<Card>();

    public void AddCard(string cardName, int cardTypeID)
    {
        StartCoroutine(ShowPlayerDrawAnim());
        GameObject cardObj = Instantiate(_playerCardPrefab, _rootContent);
        Card card = cardObj.GetComponent<Card>();
        card.InitCard(cardName, cardTypeID, this);
        _playerCards.Add(card);
    }

    public void RemoveCard(Card card)
    {
        _playerCards.Remove(card);
        card.DestroyCard();
    }

    public void Defuse(Card card)
    {
        RemoveCard(card);

        ISFSObject requestObj = new SFSObject();
        requestObj.PutUtfString(ExtensionEventNames.USED_CARD, card.cardName);
        requestObj.PutInt(ExtensionEventNames.CARD_TYPE_ID, (int)card.cardType);

        Debug.Log((int)card.cardType);

        ExtensionRequest extensionRequest = new ExtensionRequest(ExtensionEventNames.DEFUSE, requestObj);
        GlobalManager.Instance.GetSfsClient().Send(extensionRequest);
    }

    IEnumerator ShowPlayerDrawAnim()
    {
        _playerDrawAnimObj.SetActive(true);

        yield return new WaitForSeconds(1f);

        _playerDrawAnimObj.SetActive(false);
    }
}
