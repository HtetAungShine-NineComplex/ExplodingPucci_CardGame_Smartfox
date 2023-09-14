using System.Collections;
using System.Collections.Generic;
using Sfs2X.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCreator : MonoBehaviour
{
    [SerializeField] private int _maxUsers;
    [SerializeField] private bool _isGame;

    [SerializeField] private Button _createBtn;
    [SerializeField] private TMP_InputField _roomNameIF;

    public void ListenCreateRoomClick()
    {
        _createBtn.onClick.AddListener(() => CreateRoom(_roomNameIF.text, _maxUsers, _isGame));
    }

    public void CreateRoom(string roomName, int maxUsers, bool isGame)
    {
        if(roomName == string.Empty)
        {
            roomName = GlobalManager.Instance.GetSfsClient().MySelf.Name + "'s Room";
        }

        RoomSettings settings = new RoomSettings(roomName);
        settings.MaxUsers = (short)maxUsers;
        settings.IsGame = isGame;

        GlobalManager.Instance.GetSfsClient().Send(new CreateRoomRequest(settings, true));
    }
}
