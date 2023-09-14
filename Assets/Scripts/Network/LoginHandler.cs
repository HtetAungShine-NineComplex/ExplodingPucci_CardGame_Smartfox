using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using UnityEngine.EventSystems;
using Sfs2X.Util;
using UnityEngine.SceneManagement;
using TinAungKhant.UIManagement;

namespace THZ
{
    public class LoginHandler : MonoBehaviour
    {
        private GlobalManager _sfsManager;

        [SerializeField] private TMP_InputField _userNameIF;
        [SerializeField] private TMP_InputField _passwordIF;
        [SerializeField] private Button _loginBtn;
        [SerializeField] private Button _toSignUpBtn;

        [SerializeField] private string _host;
        [SerializeField] private int _tcpPort;
        [SerializeField] private string _zone;

        private SmartFox _sfs;

        // Start is called before the first frame update
        void Start()
        {
            _toSignUpBtn.onClick.AddListener(ToSignUp);
        }

        public void Connect()
        {
            _sfsManager = GlobalManager.Instance;
            _sfs = _sfsManager.CreateSfsClient();
            _loginBtn.interactable = false;


            ConfigData config = new ConfigData();
            config.Host = _host;
            config.Port = _tcpPort;
            config.Zone = _zone;

            _sfs.Connect(config);
        }

        public void ListenSfsEvents()
        {
            Debug.Log("Listen events");
            _sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            _sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
            _sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);

        }

        public void RemoveSfsEvents()
        {
            _sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
            _sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
            _sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);

        }

        public void Login()
        {
            if (_userNameIF.text != string.Empty && _passwordIF.text != string.Empty)
            {
                _sfs.Send(new LoginRequest(_userNameIF.text, _passwordIF.text, _zone));
                Debug.Log("Login");
            }
            else
            {
                //_sfs.Send(new LoginRequest(_userNameIF.text, _zone));
            }
        }

        private void OnConnection(BaseEvent evt)
        {
            // Check if the conenction was established or not
            if ((bool)evt.Params["success"])
            {
                Debug.Log("SFS2X API version: " + _sfs.Version);
                Debug.Log("Connection mode is: " + _sfs.ConnectionMode);
                _loginBtn.interactable = true;
                _loginBtn.onClick.AddListener(() => Login());

                // Login
                //_sfs.Send(new LoginRequest(nameInput.text));
            }
            else
            {

            }
        }

        private void OnLoginError(BaseEvent evt)
        {
            Debug.LogError("Login error: " + (string)evt.Params["errorMessage"]);
        }

        private void OnLogin(BaseEvent evt)
        {
            Debug.Log($"Logged In as {_sfs.MySelf.Name}");

            //UIManager.Instance.CloseUI(GLOBALCONST.UI_LOGIN);
            //UIManager.Instance.ShowUI(GLOBALCONST.UI_ROOM_MENU);
        }

        private void ToSignUp()
        {
            _sfs.Send(new LogoutRequest());
            _sfs.Send(new LoginRequest("", "", _zone));

            //UIManager.Instance.ShowUI(GLOBALCONST.UI_SIGNUP);
            //UIManager.Instance.CloseUI(GLOBALCONST.UI_LOGIN);
        }
    }
}
