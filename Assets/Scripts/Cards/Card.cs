using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardModel))]
public class Card : MonoBehaviour
{
    public string cardName { get; private set; }
    public CardType cardType { get; private set; }

    private CardModel cardModel;
    private ClientCardsManager _clientCardsManager;

    [SerializeField] private Button _cardBtn;
    [SerializeField] private Button _useBtn;
    [SerializeField] private Button _cancelBtn;
    [SerializeField] private GameObject _btnPanel;

    public void InitCard(string name, int cardTypeID, ClientCardsManager clientCardsManager)
    {
        cardModel = GetComponent<CardModel>();
        cardName = name;
        cardModel.ShowFace(name);

        cardType = (CardType)Enum.GetValues(typeof(CardType)).GetValue(cardTypeID);
        Debug.Log(cardType);
        _clientCardsManager = clientCardsManager;

        _cardBtn.onClick.AddListener(() => { _btnPanel.SetActive(true); });
        _useBtn.onClick.AddListener(UseCard);
        _cancelBtn.onClick.AddListener(() => { _btnPanel.SetActive(false); });
    }

    private void UseCard()
    {
        switch (cardType)
        {
            case CardType.DEFUSE:
                _clientCardsManager.Defuse(this);
                break;

            default:
                break;
        }
    }

    public void DestroyCard()
    {
        Destroy(this.gameObject);
    }
}
