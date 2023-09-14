using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Util;
using TMPro;
using Sfs2X.Entities.Data;

/**
 * Script attached to the Controller object in the GuestLogin scene.
 */
public class LoginSceneController : BaseSceneController
{
	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Tooltip("IP address or domain name of the SmartFoxServer instance")]
	public string host = "127.0.0.1";

	[Tooltip("TCP listening port of the SmartFoxServer instance, used for TCP socket connection in all builds except WebGL")]
	public int tcpPort = 9933;

	[Tooltip("HTTP listening port of the SmartFoxServer instance, used for WebSocket (WS) connections in WebGL build")]
	public int httpPort = 8080;

	[Tooltip("Name of the SmartFoxServer Zone to join")]
	public string zone = "BasicExamples";

	[Tooltip("Display SmartFoxServer client debug messages")]
	public bool debug = false;

	[Header("SignUp Components")]
	[SerializeField] private Button signUpBtn;
	[SerializeField] private TMP_InputField signUp_username;
	[SerializeField] private TMP_InputField signUp_password;
	[SerializeField] private TMP_InputField signUp_email;


	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public Text errorText;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private SmartFox sfs;

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	private void Start()
	{
        Connect();
        //sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);
       
		signUpBtn.onClick.AddListener(signUp);
		// Reset buddy chat history
		BuddyChatHistoryManager.Init();

		// Show connection lost message, in case the disconnection occurred in another scene
		string connLostMsg = gm.ConnectionLostMessage;
		if (connLostMsg != null)
			errorText.text = connLostMsg;
	}
	private void signUp()
	{
		Connect();
        AddSmartFoxListeners();
        if (signUp_username.text != null && signUp_password.text != null)
		{
			ISFSObject reqObj = new SFSObject();
			reqObj.PutUtfString("username", signUp_username.text);
			reqObj.PutUtfString("password", signUp_password.text);
			reqObj.PutUtfString("email",signUp_email.text);

			GlobalManager.Instance.GetSfsClient().Send(new ExtensionRequest ("$SignUp.Submit",reqObj));
		}
		else
		{
			Debug.Log("Username or password required");
		}
	}

	private void OnSignUpResponse(BaseEvent evt)
	{
		string cmd = (string)evt.Params[ExtensionEventNames.CMD];
		ISFSObject resObj = (SFSObject)evt.Params[ExtensionEventNames.PARAMS];

		if(cmd== "$SignUp.Submit")
		{
			Debug.Log(resObj.GetDump());
			if ((bool)evt.Params.ContainsKey("errorMessage"))
			{
				Debug.Log("Error Sign Up" + resObj.GetUtfString("errorMessage"));
			}
			else
			{
				Debug.Log("Sign Up Success");
				//Show UI
			}
		}
	}
    //----------------------------------------------------------
    // UI event listeners
    //----------------------------------------------------------
    #region
    /**
	 * On username input edit end, if the Enter key was pressed, connect to SmartFoxServer.
	 */
    public void OnNameInputEndEdit()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			Connect();
	}

	/**
	 * On GuestLogin button click, connect to SmartFoxServer.
	 */
	public void OnLoginButtonClick()
	{
		Connect();
	}
	#endregion

	//----------------------------------------------------------
	// Helper methods
	//----------------------------------------------------------
	#region
	/**
	 * Enable/disable username input interaction.
	 */

	/**
	 * Connect to SmartFoxServer.
	 */
	private void Connect()
	{
		// Disable user interface

		// Clear any previour error message
		errorText.text = "";

		// Set connection parameters
		ConfigData cfg = new ConfigData();
		cfg.Host = host;
		cfg.Port = tcpPort;
		cfg.Zone = zone;
		cfg.Debug = debug;

#if UNITY_WEBGL
		cfg.Port = httpPort;
#endif

		// Initialize SmartFox client
		// The singleton class GlobalManager holds a reference to the SmartFox class instance,
		// so that it can be shared among all the scenes
#if !UNITY_WEBGL
		sfs = gm.CreateSfsClient();
#else
		sfs = gm.CreateSfsClient(UseWebSocket.WS_BIN);
#endif

		// Configure SmartFox internal logger
		sfs.Logger.EnableConsoleTrace = debug;

		// Add event listeners
		AddSmartFoxListeners();

		// Connect to SmartFoxServer
		sfs.Connect(cfg);
	}

	/**
	 * Add all SmartFoxServer-related event listeners required by the scene.
	 */
	private void AddSmartFoxListeners()
	{
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);
    }

	/**
	 * Remove all SmartFoxServer-related event listeners added by the scene.
	 * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
	 */
	override protected void RemoveSmartFoxListeners()
	{
		// NOTE
		// If this scene is stopped before a connection is established, the SmartFox client instance
        // could still be null, causing an error when trying to remove its listeners

		if (sfs != null)
		{
			sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
			sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
			sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
			sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
			sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);
        }
	}

	/**
	 * Hide all modal panels.
	 */
	override protected void HideModals()
	{
		// No modals used by this scene
	}
	#endregion

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------
	#region
	private void OnConnection(BaseEvent evt)
	{
		// Check if the conenction was established or not
		if ((bool)evt.Params["success"])
		{
			Debug.Log("SFS2X API version: " + sfs.Version);
			Debug.Log("Connection mode is: " + sfs.ConnectionMode);

			// GuestLogin
			sfs.Send(new LoginRequest(""));
			//signUp();
		}
		else
		{
			// Show error message
			errorText.text = "Connection failed; is the server running at all?";

		}
	}

	private void OnConnectionLost(BaseEvent evt)
	{
		// Remove SFS listeners
		RemoveSmartFoxListeners();

		// Show error message
		string reason = (string)evt.Params["reason"];
		
		if (reason != ClientDisconnectionReason.MANUAL)
			errorText.text = "Connection lost; reason is: " + reason;

	}

	private void OnLogin(BaseEvent evt)
	{
		// Load lobby scene
		//SceneManager.LoadScene("Lobby");
	}

	private void OnLoginError(BaseEvent evt)
	{
		// Disconnect
		// NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
		sfs.Disconnect();

		// Show error message
		errorText.text = "GuestLogin failed due to the following error:\n" + (string)evt.Params["errorMessage"];
	}
	#endregion
}
