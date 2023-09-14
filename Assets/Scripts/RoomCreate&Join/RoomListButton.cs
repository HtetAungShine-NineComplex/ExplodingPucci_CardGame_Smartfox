using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListButton : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomNameTxt;
    [SerializeField] private TMP_Text _maxPlayerTxt;
    [SerializeField] private Button _btn;

    public void Init(int roomID, string roomName, int maxUsers)
    {
        _roomNameTxt.text = roomName;
        _maxPlayerTxt.text = "Max Players : " + maxUsers;

        _btn.onClick.AddListener(() => JoinRoom(roomID));
    }

    public void JoinRoom(int roomID)
    {
        GlobalManager.Instance.GetSfsClient().Send(new JoinRoomRequest(roomID));
    }
}
