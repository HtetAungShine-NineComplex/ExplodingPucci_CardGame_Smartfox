using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities;
using UnityEngine;

public class ResultPanelManager : MonoBehaviour
{
    [SerializeField] private Transform _winnerRoot;
    [SerializeField] private Transform _playersRoot;
    [SerializeField] private GameObject _resultPlayerPanel;

    private List<GameObject> _panelObjs = new List<GameObject>();

    public void ShowResult(string winnerName, int roomID)
    {
        List<User> players = GlobalManager.Instance.GetSfsClient().GetRoomById(roomID).PlayerList;

        foreach (var user in players)
        {
            if(user.Name == winnerName)
            {
                GameObject panelObj = Instantiate(_resultPlayerPanel, _winnerRoot);
                _panelObjs.Add(panelObj);
                PlayerPanel panel = panelObj.GetComponent<PlayerPanel>();
                panel.InitPlayer(user);
            }
            else
            {
                GameObject panelObj = Instantiate(_resultPlayerPanel, _playersRoot);
                _panelObjs.Add(panelObj);
                PlayerPanel panel = panelObj.GetComponent<PlayerPanel>();
                panel.InitPlayer(user);
            }    
        }
    }

    public void ClearPanels()
    {
        foreach (GameObject panelObj in _panelObjs)
        {
            Destroy(panelObj);
        }

        _panelObjs.Clear();
    }
}
