using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sfs2X.Requests;
using UnityEngine.XR;
using Sfs2X.Entities.Data;
using Sfs2X.Core;
using TinAungKhant.UIManagement;

namespace THZ
{
    public class SignUpHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _userNameIF;
        [SerializeField] private TMP_InputField _passwordIF;
        [SerializeField] private TMP_InputField _emailIF;
        [SerializeField] private Button _signUpBtn;

        [SerializeField] private string _host;
        [SerializeField] private int _tcpPort;
        [SerializeField] private string _zone;

        public void Init()
        {
            _signUpBtn.onClick.AddListener(SignUp);

        }

        public void ListenSFSEvent()
        {
            GlobalManager.Instance.GetSfsClient().AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);
        }

        public void RemoveSFSEvent()
        {
            GlobalManager.Instance.GetSfsClient().RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);
        }

        private void SignUp()
        {
            if (_userNameIF.text != string.Empty && _passwordIF.text != string.Empty)
            {
                ISFSObject requestObj = new SFSObject();
                requestObj.PutUtfString("username", _userNameIF.text);
                requestObj.PutUtfString("password", _passwordIF.text);
                requestObj.PutUtfString("email", _emailIF.text);

                GlobalManager.Instance.GetSfsClient().Send(new ExtensionRequest("$SignUp.Submit", requestObj));
            }
            else
            {
                Debug.Log("Username and Password required");
            }
        }

        private void OnSignUpResponse(BaseEvent evt)
        {
            string cmd = (string)evt.Params[ExtensionEventNames.CMD];
            ISFSObject responseObj = (SFSObject)evt.Params[ExtensionEventNames.PARAMS];

            if (cmd == "$SignUp.Submit")
            {

                Debug.Log(responseObj.GetDump());
                if (evt.Params.ContainsKey("errorMessage"))
                {
                    Debug.Log("Error Sign Up : " + responseObj.GetUtfString("errorMessage"));
                }
                else
                {
                    Debug.Log("Sign Up Success!");

                    //UIManager.Instance.ShowUI(GLOBALCONST.UI_LOGIN);
                    //UIManager.Instance.CloseUI(GLOBALCONST.UI_SIGNUP);
                }

            }
        }
    }
}
