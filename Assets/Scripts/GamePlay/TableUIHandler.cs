using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities;
using UnityEngine;
using UnityEngine.UI;

public class TableUIHandler : MonoBehaviour
{
    [SerializeField] private Transform[] _playerSpawnPoints;
    [SerializeField] private GameObject _playerPanelPrefab;
    [SerializeField] private Button _drawBtn;
    [SerializeField] private ExplodePanelHandler _explodePanelHandler;
    [SerializeField] private GameObject _yourTurnTxt;
    [SerializeField] private GameObject _clientExplodeWarningPanel;
    [SerializeField] private GameObject _putExplodeCardPanel;

    private List<PlayerPanel> _playerPanels = new List<PlayerPanel>();

    public void Init()
    {
        _drawBtn.interactable = false;
    }

    public void SpawnPlayers(List<User> users)
    {
        int spawnIndex = 0;

        //for (int i = 0; i < users.Count; i++)
        //{
        //    if (!users[i].IsItMe)
        //    {
        //        GameObject panelObj = Instantiate(_playerPanelPrefab, _playerSpawnPoints[spawnIndex]);
        //        PlayerPanel playerPanel = panelObj.GetComponent<PlayerPanel>();
        //        playerPanel.InitPlayer(users[i]);
        //        _playerPanels.Add(playerPanel);
        //        spawnIndex++;
        //    }
        //}

        foreach (var user in users)
        {
            if (!user.IsItMe)
            {
                GameObject panelObj = Instantiate(_playerPanelPrefab, _playerSpawnPoints[spawnIndex]);
                PlayerPanel playerPanel = panelObj.GetComponent<PlayerPanel>();
                playerPanel.InitPlayer(user);
                _playerPanels.Add(playerPanel);
                spawnIndex++;
            }
        }
    }

    public void OnClientTurn()
    {
        _drawBtn.interactable = true;
        StartCoroutine(ShowYourTurnText());
    }

    IEnumerator ShowYourTurnText()
    {
        _yourTurnTxt.SetActive(false);
        _yourTurnTxt.SetActive(true);

        yield return new WaitForSeconds(1f);
        _yourTurnTxt.SetActive(false);
    }

    public void OnTurnChanged(string previousPlayer, string currentPlayer)
    {
        if(previousPlayer != null)
        {
            if (previousPlayer == GlobalManager.Instance.GetSfsClient().MySelf.Name)
            {
                OnClientTurnEnds();
            }
            else
            {
                GetPanelByName(previousPlayer).OnPlayerTurnEnd();
            }
        }

        if (GlobalManager.Instance.GetSfsClient().MySelf.Name == currentPlayer)
        {
            OnClientTurn();
        }
        else
        {
            GetPanelByName(currentPlayer).OnPlayerTurn();
        }
    }

    public void OnClientTurnEnds()
    {
        Debug.Log("On Client turn ends");
        _drawBtn.interactable = false;
    }

    public void OnPlayerDrawCard(string userName)
    {
        if (userName != GlobalManager.Instance.GetSfsClient().MySelf.Name)
        {
            GetPanelByName(userName).AddCard();
        }
    }

    public void OnPlayerGotExplodeCard(string userName)
    {
        if (userName != GlobalManager.Instance.GetSfsClient().MySelf.Name)
        {
            GetPanelByName(userName).ShowExplodeCard();
        }
    }

    public void OnPlayerDefuseSuccess(string userName)
    {
        if (userName != GlobalManager.Instance.GetSfsClient().MySelf.Name)
        {
            GetPanelByName(userName).HideExplodeCard();
        }
        else
        {
            HideExplodeWarningPanel();
            ShowPutExplodePanel();
        }
    }

    public void OnAPlayerExplode(List<User> users, string explodePlayerName)
    {
        _explodePanelHandler.ShowAnimation(users, explodePlayerName);

        if (explodePlayerName != GlobalManager.Instance.GetSfsClient().MySelf.Name)
        {
            GetPanelByName(explodePlayerName).OnExplode();
        }
        else
        {
            _clientExplodeWarningPanel.SetActive(false);
        }
    }

    public PlayerPanel GetPanelByName(string userName)
    {
        foreach (PlayerPanel panel in _playerPanels)
        {
            if(panel.CurrentUser.Name == userName)
            {
                return panel;
            }
        }

        return null;
    }

    public void ShowExplodeWarningPanel()
    {
        _clientExplodeWarningPanel.SetActive(true);
    }

    public void HideExplodeWarningPanel()
    {
        _clientExplodeWarningPanel.SetActive(false);
    }

    public void ShowPutExplodePanel()
    {
        if (!_putExplodeCardPanel.activeSelf)
        {
            _putExplodeCardPanel.SetActive(true);
        }
    }

    public void HidePutExplodePanel()
    {
        if(_putExplodeCardPanel.activeSelf)
        {
            _putExplodeCardPanel.SetActive(false);
        }
    }
}
