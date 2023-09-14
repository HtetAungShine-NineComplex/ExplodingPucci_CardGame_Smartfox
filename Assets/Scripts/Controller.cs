using UnityEngine;
using UnityEngine.UI;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Logging;
using Sfs2X.Requests;
using Sfs2X.Util;
using Sfs2X.Entities.Data;
using Sfs2X.Bitswarm;
using TMPro;
using TinAungKhant.UIManagement;

/**
 * Script attached to the Controller object in the scene.
 */
public class Controller : MonoBehaviour
{
	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------
	[SerializeField] private Button guest_Btn;
	[Header("Login Components")]
	[SerializeField] private TMP_InputField loginName_Input;
	[SerializeField] private TMP_InputField loginPassword_Input;
	[SerializeField] private Button login_Btn;
	[SerializeField] private Button createAnAcc_btn;
	[SerializeField] private Text errorText; 

	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Space(20)]
	[Header("SFS2X connection settings")]

	[Tooltip("IP address or domain name of the SmartFoxServer instance; if encryption is enabled, a domain name must be entered")]
	public string host = "127.0.0.1";

	[Tooltip("TCP listening port of the SmartFoxServer instance, used for TCP socket connection in all builds except WebGL")]
	public int tcpPort = 9933;

	[Tooltip("HTTP listening port of the SmartFoxServer instance, used for WebSocket (WS) connections in WebGL build")]
	public int httpPort = 8080;

	[Tooltip("HTTPS listening port of the SmartFoxServer instance, used for WebSocket Secure (WSS) connections in WebGL build and connection encryption in all other builds")]
	public int httpsPort = 8443;

	[Tooltip("Use SmartFoxServer's HTTP tunneling (BlueBox) if TCP socket connection can't be established; not available in WebGL builds")]
	public bool useHttpTunnel = false;

	[Tooltip("Enable SmartFoxServer protocol encryption; 'host' must be a domain name and an SSL certificate must have been deployed")]
	public bool encrypt = false;

	[Tooltip("Name of the SmartFoxServer Zone to join")]
	public string zone = "BasicExamples";

	[Tooltip("Display SmartFoxServer client debug messages")]
	public bool debug = false;

	[Tooltip("Client-side SmartFoxServer logging level")]
	public LogLevel logLevel = LogLevel.INFO;

	[Header("GuestLogin UI")]
	[SerializeField] private LoginUI loginUI;

	[Header("Register Components")]

	[SerializeField] private TMP_InputField regName_Input;
	[SerializeField] private TMP_InputField regEmail_Input;
	[SerializeField] private TMP_InputField regPassword_Input;
	[SerializeField] private Button register_Btn;
	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private SmartFox sfs;

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------
	#region
	private void Start()
	{
		
		// Make sure the application runs in background
		Application.runInBackground = true;

		register_Btn.onClick.AddListener(() => OnRegisterButtonClick());
		guest_Btn.onClick.AddListener(() => OnGuestLoginClick());
		createAnAcc_btn.onClick.AddListener(() => OnGuestLoginClick());

        Connect();
    }

	private void Update()
	{
		// Process the SmartFox events queue
		if (sfs != null)
			sfs.ProcessEvents();
	}

	private void OnApplicationQuit()
	{
		// Disconnect from SmartFoxServer if a connection is active
		// This is required because an active socket connection during the application quit process can cause a crash on some platforms
		if (sfs != null && sfs.IsConnected)
			sfs.Disconnect();
	}
	#endregion
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
	public void OnGuestLoginClick()
	{
		//Connect();
		GuestLogin();
	}

	public void OnRegisterButtonClick()
	{
       
        //Connect();
        sfs.Send(new LogoutRequest());
        sfs.Send(new LoginRequest("", "", "CardGame"));
        Register();
	}
	/**
	 * On Logout button click, disconnect from SmartFoxServer.
	 */
	public void OnLogoutButtonClick()
	{
		sfs.Disconnect();
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
		Debug.Log("Attempting connection to SFS2X...");

		// Clear any previour error message
		errorText.text = "";

		// Initialize SmartFox client
#if !UNITY_WEBGL
		sfs = new SmartFox();
#else
		sfs = new SmartFox(encrypt ? UseWebSocket.WSS_BIN : UseWebSocket.WS_BIN);
#endif

		// Add event listeners
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener(SFSEvent.CRYPTO_INIT, OnCryptoInit);
		sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);

		// Configure internal SFS2X logger
		sfs.Logger.EnableConsoleTrace = true;
		sfs.Logger.LoggingLevel = logLevel;

		// Set connection parameters
		ConfigData cfg = new ConfigData();
		cfg.Host = host;
		cfg.Port = tcpPort;
		cfg.Zone = zone;
		cfg.HttpPort = httpPort;
		cfg.HttpsPort = httpsPort;
		cfg.BlueBox.IsActive = useHttpTunnel;
		cfg.BlueBox.UseHttps = encrypt;
		cfg.Debug = debug;

#if UNITY_WEBGL
		cfg.Port = encrypt ? httpsPort : httpPort;
#endif

		// Connect to SmartFoxServer
		sfs.Connect(cfg);
	}

	private void GuestLogin()
	{
		Debug.Log("Performing login...");

        // Guest GuestLogin
        sfs.Send(new LogoutRequest());
        sfs.Send(new LoginRequest("", "", "CardGame"));
	}

	private void Register()
	{
		Debug.Log("Performing register...");

        //Register
        if (regName_Input.text != string.Empty && regPassword_Input.text != string.Empty)
        {
            ISFSObject requestObj = new SFSObject();
            requestObj.PutUtfString("username", regName_Input.text);
            requestObj.PutUtfString("password", regPassword_Input.text);
            requestObj.PutUtfString("email", regEmail_Input.text);

            sfs.Send(new ExtensionRequest("$SignUp.Submit", requestObj));
        }
        else
        {
            Debug.Log("Username and Password required");
        }
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
			Debug.Log("Connection established successfully");
			Debug.Log("SFS2X API version: " + sfs.Version);
			Debug.Log("Connection mode is: " + sfs.ConnectionMode);

#if !UNITY_WEBGL
			if (encrypt)
			{
				Debug.Log("Initializing encryption...");

				// Initialize encryption
				sfs.InitCrypto();
			}
			else
			{
				// Attempt login

				//GuestLogin();
				/*sfs.Send(new LogoutRequest());
				sfs.Send(new LoginRequest("", "", "CardGame"));*/
				//Register();
			}
#else
			// Attempt login
			GuestLogin();
			Register();
#endif
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
		sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.RemoveEventListener(SFSEvent.CRYPTO_INIT, OnCryptoInit);
		sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnSignUpResponse);

		sfs = null;


		// Show error message
		string reason = (string)evt.Params["reason"];

		Debug.Log("Connection to SmartFoxServer lost; reason is: " + reason);

		if (reason != ClientDisconnectionReason.MANUAL)
		{
			// Show error message
			string connLostMsg = "An unexpected disconnection occurred; ";

			if (reason == ClientDisconnectionReason.IDLE)
				connLostMsg += "you have been idle for too much time";
			else if (reason == ClientDisconnectionReason.KICK)
				connLostMsg += "you have been kicked";
			else if (reason == ClientDisconnectionReason.BAN)
				connLostMsg += "you have been banned";
			else
				connLostMsg += "reason is unknown.";

			errorText.text = connLostMsg;
		}
	}

	private void OnCryptoInit(BaseEvent evt)
	{
		if ((bool)evt.Params["success"])
		{
			Debug.Log("Encryption initialized successfully");

            // Attempt login
           /* GuestLogin();
            sfs.Send(new LogoutRequest());
            sfs.Send(new LoginRequest("", "", "CardGame"));
            Register();*/
		}
		else
		{
			Debug.Log("Encryption initialization failed: " + (string)evt.Params["errorMessage"]);

			// Disconnect
			// NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
			sfs.Disconnect();

			// Show error message
			errorText.text = "Encryption initialization failed";
		}
	}

	private void OnLogin(BaseEvent evt)
	{
		Debug.Log("GuestLogin successful");
		UIManager.Instance.CloseUI(GLOBALCONST.LOGIN_UI);
		UIManager.Instance.ShowUI(GLOBALCONST.LOBBY_UI);
	}
    private void OnSignUpResponse(BaseEvent evt)
    {
        string cmd = (string)evt.Params[ExtensionEventNames.CMD];
        ISFSObject responseObj = (SFSObject)evt.Params[ExtensionEventNames.PARAMS];

        if (cmd == "$SignUp.Submit")
        {

            Debug.Log(responseObj.GetDump());
            if ((bool)evt.Params.ContainsKey("errorMessage"))
            {
                Debug.Log("Error Sign Up : " + responseObj.GetUtfString("errorMessage"));
            }
            else
            {
                Debug.Log("Sign Up Success!");
            }

        }
    }
    private void OnLoginError(BaseEvent evt)
	{
		Debug.Log("GuestLogin failed");

		// Disconnect
		// NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
		sfs.Disconnect();

		// Show error message
		errorText.text = "GuestLogin failed due to the following error:\n" + (string)evt.Params["errorMessage"];
	}
	#endregion
}
