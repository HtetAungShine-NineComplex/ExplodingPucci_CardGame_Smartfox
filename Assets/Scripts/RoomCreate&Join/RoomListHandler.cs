using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using TinAungKhant.UIManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THZ
{
    public class RoomListHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _roomListObj;
        [SerializeField] private GameObject _noRoomText;
        [SerializeField] private Transform _roomListRoot;

        private void Start()
        {
        }

        public void ListenSfsEvents()
        {
            GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdd);
            GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoined);
            GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreateError);

        }

        public void RemoveSfsEvents()
        {
            GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.ROOM_ADD, OnRoomAdd);
            GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoined);
            GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreateError);
        }

        public void RequestRoomList()
        {
            List<Room> rooms = GlobalManager.Instance.GetSfsClient().RoomManager.GetRoomList();

            Debug.Log("Rooms : " + rooms.Count);

            if (rooms.Count > 0)
            {
                _noRoomText.SetActive(false);

                foreach (Room room in rooms)
                {
                    GameObject roomListObj = Instantiate(_roomListObj, _roomListRoot);
                    RoomListButton roomListButton = roomListObj.GetComponent<RoomListButton>();
                    roomListButton.Init(room.Id, room.Name, room.MaxUsers);
                }
            }
            else
            {
                _noRoomText.SetActive(true);
            }
        }

        public void OnRoomAdd(BaseEvent evt)
        {
            Debug.Log("Added Room : " + evt.Params["room"]);
            _noRoomText.SetActive(false);

            Room room = (Room)evt.Params["room"];
            string roomName = room.Name;
            int maxUsers = room.MaxUsers;
            GameObject roomListObj = Instantiate(_roomListObj, _roomListRoot);
            RoomListButton roomListButton = roomListObj.GetComponent<RoomListButton>();
            roomListButton.Init(room.Id, roomName, maxUsers);
        }

        public void OnRoomJoined(BaseEvent evt)
        {
            Debug.Log("Joined Room : " + evt.Params["room"]);

            //UIManager.Instance.ShowUI(GLOBALCONST.UI_LOBBY);
            //UIManager.Instance.CloseUI(GLOBALCONST.UI_ROOM_MENU);
        }

        public void OnRoomCreateError(BaseEvent evt)
        {
            Debug.Log("Create Room error: " + (string)evt.Params["errorMessage"]);
        }
    }
}
