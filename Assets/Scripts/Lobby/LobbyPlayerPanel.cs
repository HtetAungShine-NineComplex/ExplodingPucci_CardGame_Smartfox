using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameTxt;

    public void SetName(string name)
    {
        _nameTxt.text = name;
    }
}
