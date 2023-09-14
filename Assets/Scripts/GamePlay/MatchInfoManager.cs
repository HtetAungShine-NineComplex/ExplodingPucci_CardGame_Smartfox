using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchInfoManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _infoTxt;

    public void Init()
    {
        _infoTxt.text = string.Empty;
    }

    public void ShowPlayerTurn(string playerName)
    {
        _infoTxt.text = playerName + "'s turn.";
    }

    public void UpdateMessage(string message)
    {
        _infoTxt.text = message;
    }
}
