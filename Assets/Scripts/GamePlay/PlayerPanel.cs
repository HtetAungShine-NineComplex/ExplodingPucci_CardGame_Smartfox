using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities;
using TMPro;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameTxt;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private GameObject _explodeCard;
    [SerializeField] private Transform _cardRoot;
    [SerializeField] private GameObject _turnAnimationObj;
    [SerializeField] private GameObject _explodeTxtObj;
    [SerializeField] private CanvasGroup _canvasGroup;

    public User CurrentUser { get; private set; }

    private List<GameObject> _playerCards = new List<GameObject>();

    public void InitPlayer(User user)
    {
        _canvasGroup.alpha = 1f;
        _turnAnimationObj.SetActive(false);
        _playerNameTxt.text = user.Name;
        CurrentUser = user;
    }

    public void AddCard()
    {
        GameObject cardObj = Instantiate(_cardPrefab, _cardRoot);
        _playerCards.Add(cardObj);
    }

    public void RemoveCard()
    {
        _playerCards.RemoveAt(0);
    }

    public void ShowExplodeCard()
    {
        _explodeCard.SetActive(true);
    }

    public void HideExplodeCard()
    {
        _explodeCard.SetActive(false);
    }

    public void OnPlayerTurn()
    {
        _turnAnimationObj.SetActive(true);
    }

    public void OnPlayerTurnEnd()
    {
        _turnAnimationObj.SetActive(false);
    }

    public void OnExplode()
    {
        _explodeCard.SetActive(false);
        _canvasGroup.alpha = 0.5f;
    }

    //to call only from explode panel
    public void ShowExplodeAnimation()
    {
        _explodeTxtObj.SetActive(true);
    }
}
