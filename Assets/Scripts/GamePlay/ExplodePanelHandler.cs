using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities;
using UnityEngine;

public class ExplodePanelHandler : MonoBehaviour
{
    [SerializeField] private GameObject playerPanelPrefab;
    private List<GameObject> _panels = new List<GameObject>();

    public void ShowAnimation(List<User> users, string explodedPlayerName)
    {
        foreach (var panel in _panels)
        {
            Destroy(panel);
        }
        _panels = new List<GameObject>();

        this.gameObject.SetActive(true);
        StartCoroutine(ExplodeAnimation(users, explodedPlayerName));
    }

    IEnumerator ExplodeAnimation(List<User> users, string explodedPlayerName)
    {
        PlayerPanel explodedPanel = new PlayerPanel();

        foreach (var user in users)
        {
            PlayerPanel panel = new PlayerPanel();

            GameObject panelObj = Instantiate(playerPanelPrefab, transform);
            _panels.Add(panelObj);
            panel = panelObj.GetComponent<PlayerPanel>();
            panel.InitPlayer(user);

            if (user.Name == explodedPlayerName)
            {
                explodedPanel = panel;
            }
        }

        explodedPanel.ShowExplodeAnimation();

        yield return new WaitForSeconds(2.6f);

        this.gameObject.SetActive(false);
    }
}
