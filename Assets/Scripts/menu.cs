using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menu : MonoBehaviour
{
	public enum MenuGUIStates
	{
		username = 0,
		searching = 1,
		joining = 2,
		create = 3,
		creating = 4,
		serverlist = 5,
		pauseMenu = 6,
		options = 7,
		ban = 8
	}
	
	private string versionName;
	
	public int ConnectPort;
	
	public int ListenPort;
	
	public string port;
	
	private string IPAddress;
	
	private int MasterPort;
	
	private int FacilitatorPort;
	
	public Vector3 ballPos;
	
	public Vector3 ballInitialSpawn;
	
	public bool ballSpawned;
	
	public HostData[] host;
	
	public Ping[] pingTimes;
	
	private bool connecting;
	
	public GameObject target;
	
	private MouseLook mouselook;
	
	public string username;
	
	private string password;
	
	private string ServerPassword;
	
	private string loginMessage;
	
	public bool gotUsernameFromWebsite;
	
	private bool userAuthenticated;
	
	private float loginStartTime;
	
	private float loginTimeoutDuration;
	
	private string joiningServerName;
	
	private bool allowAdminAccess;
	
	private float baseServerIndex;
	
	private float baseBanListIndex;
	
	private bool passcode;
	
	private bool joiningPasswordProtected;
	
	private int passcodeProgress;
	
	private int passcodeMaxProgress;
	
	private string loginURL;
	
	private bool loggedIn;
	
	private bool canLogIn;
	
	public List<string> Players;
	
	public List<string> PlayerIPs;
	
	public List<string> BannedPlayers;
	
	public MenuGUIStates currentMenu;
	
	private float timerStartLooking;
	
	private float timerLookingDuration;
	
	public GUISkin guiSkin;
	
	public Font fontEras;
	
	public menu()
	{
		versionName = "Kritz-CBD-00-05-06";
		ConnectPort = 25000;
		ListenPort = 25001;
		port = string.Empty;
		IPAddress = "127.0.0.1";
		MasterPort = 23466;
		FacilitatorPort = 50005;
		ballInitialSpawn = new Vector3(-31f, 12f, -54f);
		username = string.Empty;
		password = string.Empty;
		ServerPassword = string.Empty;
		loginMessage = string.Empty;
		loginTimeoutDuration = 30f;
		allowAdminAccess = true;
		passcodeMaxProgress = 6;
		loginURL = "127.0.0.1/unitylogin.php";
		Players = new List<string>();
		PlayerIPs = new List<string>();
		BannedPlayers = new List<string>();
		timerLookingDuration = 10f;
	}
	
	private void Awake()
	{
		MasterServer.ipAddress = IPAddress;
		MasterServer.port = MasterPort;
		Network.natFacilitatorIP = IPAddress;
		Network.natFacilitatorPort = FacilitatorPort;
		MasterServer.RequestHostList(versionName);
		username = PlayerPrefs.GetString("Username");
		fontEras = Resources.Load("Text/ERASBD") as Font;
		GetLogin();
		port = string.Empty + ListenPort;
	}
	
	[RPC]
	private void GetPlayerUsername(string ip, string newUsername)
	{
		MonoBehaviour.print(newUsername + " reported ip of " + ip);
		int index = PlayerIPs.IndexOf(ip);
		Players[index] = newUsername;
	}
	
	private void OnPlayerConnected(NetworkPlayer player)
	{
		if (BannedPlayers.IndexOf(player.ipAddress) != -1)
		{
			Network.CloseConnection(player, true);
			return;
		}
		PlayerIPs.Add(player.ipAddress);
		Players.Add("error: waiting for username");
	}
	
	private void OnServerInitialized()
	{
		StopAllCoroutines();
		if (Application.loadedLevel == 0)
		{
			Application.LoadLevel(1);
		}
		else
		{
			GameObject.Find("Spawn").GetComponent<SpawnPlayer>().Spawn();
		}
	}
	
	private void OnConnectedToServer()
	{
		StopAllCoroutines();
		Network.isMessageQueueRunning = false;
		Application.LoadLevel(1);
	}
	
	private void Update()
	{
		if (host == null || host.Length == 0)
		{
			host = MasterServer.PollHostList();
		}
		if (loginStartTime > 0f && loginStartTime + loginTimeoutDuration < Time.time)
		{
			loginMessage += "\n\nConnection to the master server is taking a while.\n The website may be under heavy load.\n";
			loginStartTime = 0f;
		}
	}
	
	private void GetLogin()
	{
		Application.ExternalCall("UnityShake");
	}
	
	private void GiveUsername(string webUsername)
	{
		username = webUsername;
		MonoBehaviour.print(webUsername);
		gotUsernameFromWebsite = true;
		PlayerPrefs.SetString("Username", username);
	}
	
	private void RefreshPings()
	{
		if (host != null && host.Length != 0)
		{
			StartCoroutine(PingBatch(host));
		}
	}
	
	private IEnumerator PingBatch(HostData[] hostsToPing)
	{
		pingTimes = new Ping[host.Length];
		int index = 0;
		for (int i = 0; i < hostsToPing.Length; i++)
		{
			string iP = hostsToPing[i].ip[0];
			StartCoroutine(PingHost(iP, index));
			index++;
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
		}
	}
	
	private IEnumerator PingHost(string IP, int index)
	{
		float timeStarted = Time.time;
		Ping p = new Ping(IP);
		while (!p.isDone && Time.time - timeStarted < 10f)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));
		}
		MonoBehaviour.print("Pinged " + IP + ": " + p.time);
		pingTimes[index] = p;
	}
	
	private IEnumerator Authenticate()
	{
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("unity", 1);
		wWWForm.AddField("username", username);
		wWWForm.AddField("password", password);
		WWW download = new WWW(loginURL, wWWForm);
		yield return download;
		if (!string.IsNullOrEmpty(download.error))
		{
			MonoBehaviour.print("Error downloading: " + download.error);
			yield break;
		}
		MonoBehaviour.print("Request success");
		if (download.text.Contains("success"))
		{
			MonoBehaviour.print("User authenticated");
			userAuthenticated = true;
			MasterServer.RequestHostList(versionName);
			RefreshPings();
			timerStartLooking = Time.time;
			currentMenu = MenuGUIStates.searching;
		}
		else
		{
			loginMessage = "Incorrect username or password";
		}
		loginStartTime = 0f;
	}
	
	private void OnGUI()
	{
		GUI.skin = guiSkin;
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.font = fontEras;
		gUIStyle.normal.textColor = Color.white;
		gUIStyle.fontSize = 64;
		gUIStyle.alignment = TextAnchor.MiddleCenter;
		Event current = Event.current;
		if (currentMenu == MenuGUIStates.username)
		{
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			gUIStyle.alignment = TextAnchor.MiddleCenter;
			GUI.Label(new Rect(Screen.width / 2 - 48, Screen.height / 20 - 3, 100f, 25f), "CITIZEN BURGER DISORDER by DarKer", gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 20, 100f, 25f), "CITIZEN BURGER DISORDER by DarKer", gUIStyle);
			gUIStyle.fontSize = 20;
			gUIStyle.normal.textColor = Color.black;
			GUI.Label(new Rect(Screen.width - 112, Screen.height - 32, 100f, 25f), "V" + versionName.Substring(versionName.IndexOf("CBD") + 4), gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width - 110, Screen.height - 30, 100f, 25f), "V" + versionName.Substring(versionName.IndexOf("CBD") + 4), gUIStyle);
			gUIStyle.fontSize = 40;
			if (gotUsernameFromWebsite || username.Equals("!"))
			{
				gUIStyle.alignment = TextAnchor.MiddleRight;
				GUI.Label(new Rect(Screen.width / 2 + 2 - 325, Screen.height / 20 + 120, 100f, 25f), "Password:", gUIStyle);
				gUIStyle.alignment = TextAnchor.MiddleCenter;
				gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
				GUI.Label(new Rect(Screen.width / 2 + 2 - 50, Screen.height / 20 + 75, 100f, 25f), username, gUIStyle);
				gUIStyle.normal.textColor = Color.white;
				GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 20 + 75, 100f, 25f), username, gUIStyle);
				if (GUI.Button(new Rect((float)Screen.width / 2f + 200f, Screen.height / 20 + 75, 100f, 43f), "Change"))
				{
					gotUsernameFromWebsite = false;
				}
			}
			else
			{
				gUIStyle.alignment = TextAnchor.MiddleRight;
				GUI.Label(new Rect(Screen.width / 2 + 2 - 325, Screen.height / 20 + 75, 100f, 25f), "Username:", gUIStyle);
				GUI.Label(new Rect(Screen.width / 2 + 2 - 325, Screen.height / 20 + 120, 100f, 25f), "Password:", gUIStyle);
				gUIStyle.alignment = TextAnchor.MiddleCenter;
				GUI.skin.textField.fontSize = 42;
				GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
				username = GUI.TextField(new Rect(Screen.width / 2 - 200, Screen.height / 20 + 70, 400f, 45f), username, 25);
			}
			GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
			password = GUI.PasswordField(new Rect(Screen.width / 2 - 200, Screen.height / 20 + 120, 400f, 42f), password, "*"[0]);
			if ((loginStartTime == 0f && current.keyCode == KeyCode.Return) || GUI.Button(new Rect((float)Screen.width / 2f + 200f, Screen.height / 20 + 119, 100f, 43f), "Login"))
			{
				loginMessage = "Logging in...";
				StartCoroutine(Authenticate());
				loginStartTime = Time.time;
			}
			gUIStyle.fontSize = 38;
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, Screen.height / 20 + 200, 100f, 25f), loginMessage, gUIStyle);
			gUIStyle.fontSize = 48;
		}
		else if (currentMenu == MenuGUIStates.searching)
		{
			if (PlayerPrefs.GetString("Username") != username)
			{
				PlayerPrefs.SetString("Username", username);
				MonoBehaviour.print("Updated username to " + username);
			}
			string text = "Lookin'...";
			currentMenu = MenuGUIStates.serverlist;
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, Screen.height / 3 - 3, 100f, 25f), text, gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 3, 100f, 25f), text, gUIStyle);
		}
		else if (currentMenu == MenuGUIStates.serverlist)
		{
			if (host == null)
			{
				host = MasterServer.PollHostList();
				RefreshPings();
				return;
			}
			string text2 = "Servers:";
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, 10f, 100f, 25f), text2, gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, 10f, 100f, 25f), text2, gUIStyle);
			gUIStyle.fontSize = 26;
			if (host.Length > 6)
			{
				baseServerIndex = GUI.VerticalScrollbar(new Rect(Screen.width - 150, 60f, 20f, (float)Screen.height / 1.2f), baseServerIndex, 1f, -1f, host.Length - 1);
			}
			else
			{
				baseServerIndex = -1f;
			}
			gUIStyle.alignment = TextAnchor.MiddleLeft;
			gUIStyle.fontSize = 14;
			GUI.Label(new Rect(Screen.width / 2 - 220, 60f, 250f, 25f), "Server name", gUIStyle);
			GUI.Label(new Rect(Screen.width / 2 + 100, 60f, 250f, 25f), "Players", gUIStyle);
			GUI.Label(new Rect(Screen.width / 2 + 200, 60f, 250f, 25f), "Ping", gUIStyle);
			gUIStyle.alignment = TextAnchor.MiddleCenter;
			gUIStyle.fontSize = 26;
			int num = 0;
			HostData[] array = host;
			foreach (HostData hostData in array)
			{
				bool flag = false;
				string gameName = hostData.gameName;
				string text3 = string.Empty + hostData.connectedPlayers;
				string empty = string.Empty;
				float num2 = -1f;
				if (pingTimes == null || pingTimes.Length <= num || pingTimes[num] == null || pingTimes[num].time <= 0)
				{
					empty = ((pingTimes == null || pingTimes.Length <= num || pingTimes[num] == null || pingTimes[num].time != -1) ? (empty + "...") : (empty + "???"));
				}
				else
				{
					empty = string.Empty + pingTimes[num].time + "ms";
					num2 = pingTimes[num].time;
				}
				if (joiningServerName == gameName)
				{
					flag = true;
				}
				gUIStyle.alignment = TextAnchor.MiddleLeft;
				if (!flag || (flag && ServerPassword.Length <= 0))
				{
					GUI.Label(new Rect(Screen.width / 2 - 220, (0f - baseServerIndex + (float)num) * 50f + 60f, 250f, 25f), gameName, gUIStyle);
					GUI.Label(new Rect(Screen.width / 2 + 100, (0f - baseServerIndex + (float)num) * 50f + 60f, 250f, 25f), text3, gUIStyle);
					if (num2 > 300f)
					{
						gUIStyle.normal.textColor = Color.Lerp(Color.red, Color.black, 0.5f);
					}
					else if (num2 > 200f)
					{
						gUIStyle.normal.textColor = Color.red;
					}
					else if (num2 > 100f)
					{
						gUIStyle.normal.textColor = Color.yellow;
					}
					else if (num2 > 1f)
					{
						gUIStyle.normal.textColor = Color.green;
					}
					else if (num2 < 0f)
					{
						gUIStyle.normal.textColor = Color.grey;
					}
					GUI.Label(new Rect(Screen.width / 2 + 200, (0f - baseServerIndex + (float)num) * 50f + 60f, 250f, 25f), empty, gUIStyle);
					gUIStyle.normal.textColor = Color.white;
					gUIStyle.alignment = TextAnchor.MiddleCenter;
				}
				if (!flag)
				{
					if (GUI.Button(new Rect(Screen.width / 2 - 355, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f), "Join"))
					{
						joiningServerName = gameName;
						if (hostData.comment.Length <= 1)
						{
							Network.Connect(hostData);
						}
					}
					if (username == "Kritz")
					{
						string empty2 = string.Empty;
						GUI.Label(text: (!(hostData.comment.Substring(0, 1) == "0")) ? "Allows admins" : "Locked to admins", position: new Rect(Screen.width / 2 + 400, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f));
					}
				}
				else if (hostData.comment.Length > 1)
				{
					if (!joiningPasswordProtected)
					{
						GUI.skin.textField.fontSize = 30;
						GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
						ServerPassword = GUI.TextField(new Rect(Screen.width / 2 - 225, (0f - baseServerIndex + (float)num) * 50f + 60f, 500f, 30f), ServerPassword, 25);
					}
					if ((ServerPassword.Length > 0 && ServerPassword == hostData.comment.Substring(1)) || (hostData.comment.Substring(0, 1).Equals("1") && username.Equals("Kritz")))
					{
						if (!joiningPasswordProtected)
						{
							if (GUI.Button(new Rect(Screen.width / 2 - 355, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f), "Alright!"))
							{
								joiningServerName = gameName;
								joiningPasswordProtected = true;
								Network.Connect(hostData);
							}
						}
						else
						{
							GUI.Label(new Rect(Screen.width / 2 - 355, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f), "Joinin'...");
						}
					}
					else if (ServerPassword.Length > 0 && GUI.Button(new Rect(Screen.width / 2 - 355, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f), "Alright!"))
					{
						ServerPassword = string.Empty;
						joiningPasswordProtected = false;
						flag = false;
						joiningServerName = string.Empty;
					}
				}
				else
				{
					GUI.Label(new Rect(Screen.width / 2 - 355, (0f - baseServerIndex + (float)num) * 50f + 62f, 60f, 25f), "Joinin'...");
				}
				if (hostData.comment.Length > 1)
				{
					if (!flag && !joiningPasswordProtected)
					{
						GUI.skin.label.fontSize = 12;
						GUI.Label(new Rect(Screen.width / 2 - 290, (0f - baseServerIndex + (float)num) * 50f + 62f, 70f, 25f), "(Protected)");
					}
					else if (!joiningPasswordProtected)
					{
						GUI.skin.label.fontSize = 14;
						GUI.skin.label.alignment = TextAnchor.MiddleLeft;
						GUI.Label(new Rect(Screen.width / 2 - 292, (0f - baseServerIndex + (float)num) * 50f + 62f, 150f, 25f), "Password:");
					}
				}
				num++;
			}
			if (GUI.Button(new Rect(Screen.width - 110, Screen.height - 60, 100f, 50f), "Create"))
			{
				currentMenu = MenuGUIStates.create;
			}
			if (GUI.Button(new Rect(10f, Screen.height - 60, 100f, 50f), "Refresh"))
			{
				MasterServer.RequestHostList(versionName);
				host = MasterServer.PollHostList();
				RefreshPings();
			}
		}
		else if (currentMenu == MenuGUIStates.create)
		{
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, Screen.height / 20 - 3, 100f, 25f), "Server Options", gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 20, 100f, 25f), "Server Options", gUIStyle);
			gUIStyle.fontSize = 42;
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, (float)Screen.height / 7f + 20f, 100f, 25f), "Server Password?", gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, (float)Screen.height / 7f + 22f, 100f, 25f), "Server Password?", gUIStyle);
			gUIStyle.fontSize = 25;
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, (float)Screen.height / 7f + 100f, 100f, 25f), "(blank for public server)", gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, (float)Screen.height / 7f + 103f, 100f, 25f), "(blank for public server)", gUIStyle);
			GUI.skin.textField.fontSize = 42;
			GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
			ServerPassword = GUI.TextField(new Rect(Screen.width / 2 - 200, Screen.height / 7 + 60, 400f, 45f), ServerPassword, 25);
			GUI.skin.toggle.fontSize = 16;
			string text5 = "  Administrator Access Disabled";
			if (allowAdminAccess)
			{
				text5 = "  Administrator Access Enabled";
			}
			if (ServerPassword.Length > 0)
			{
				allowAdminAccess = GUI.Toggle(new Rect(Screen.width / 2 - 100, Screen.height / 7 + 130, 300f, 30f), allowAdminAccess, text5);
			}
			else
			{
				allowAdminAccess = true;
			}
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 175, (float)Screen.height / 7f + 2f + 165f, 100f, 25f), "Server Port", gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 175, (float)Screen.height / 7f + 165f, 100f, 25f), "Server Port", gUIStyle);
			port = GUI.TextField(new Rect(Screen.width / 2, Screen.height / 7 + 160, 200f, 45f), string.Empty + port, 25);
			if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 7 + 240, 300f, 50f), "Create"))
			{
				try
				{
					ListenPort = int.Parse(port);
					currentMenu = MenuGUIStates.creating;
				}
				catch (Exception)
				{
					port = string.Empty + 25001;
				}
			}
		}
		else if (currentMenu == MenuGUIStates.creating)
		{
			string text6 = "Creatin'...";
			gUIStyle.normal.textColor = Color.Lerp(Color.black, Color.grey, 0.1f);
			GUI.Label(new Rect(Screen.width / 2 + 2 - 50, Screen.height / 3 - 3, 100f, 25f), text6, gUIStyle);
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 3, 100f, 25f), text6, gUIStyle);
			string empty3 = string.Empty;
			empty3 = ((!allowAdminAccess) ? (empty3 + "0") : (empty3 + "1"));
			if (ServerPassword.Length > 0)
			{
				empty3 += ServerPassword;
			}
			MonoBehaviour.print(empty3);
			bool useNat = !Network.HavePublicAddress();
			NetworkConnectionError serverError = Network.InitializeServer(10, ListenPort, useNat);
			if (serverError != NetworkConnectionError.NoError)
			{
				loginMessage = "Server create failed: " + serverError.ToString();
				Debug.LogError(loginMessage);
				currentMenu = MenuGUIStates.create;
				return;
			}
			MasterServer.RegisterHost(versionName, username + "'s Server", empty3);
			currentMenu = MenuGUIStates.pauseMenu;
		}
		else if (currentMenu == MenuGUIStates.pauseMenu)
		{
			if (GUI.Button(new Rect(Screen.width / 2 - 100, 225f, 200f, 60f), "Resume"))
			{
				Screen.lockCursor = true;
				GetComponent<MouseLook>().enabled = true;
				base.enabled = false;
			}
			if (GUI.Button(new Rect(Screen.width / 2 - 100, 295f, 200f, 60f), "Options"))
			{
				currentMenu = MenuGUIStates.options;
			}
			if (GUI.Button(new Rect(Screen.width / 2 - 100, 435f, 200f, 60f), "Quit to Main Menu"))
			{
				Network.Disconnect();
				MasterServer.UnregisterHost();
				host = MasterServer.PollHostList();
				RefreshPings();
				Screen.lockCursor = false;
				Application.LoadLevel(0);
			}
		}
		else if (currentMenu == MenuGUIStates.options)
		{
			if (mouselook == null)
			{
				mouselook = GetComponent<MouseLook>();
			}
			if (GUI.Button(new Rect(Screen.width / 2 - 100, 225f, 200f, 60f), "Back"))
			{
				currentMenu = MenuGUIStates.pauseMenu;
			}
			if (Network.isServer && GUI.Button(new Rect(Screen.width / 2 - 100, 300f, 200f, 60f), "Kick / Ban Users"))
			{
				currentMenu = MenuGUIStates.ban;
			}
			mouselook.inversion = GUI.Toggle(new Rect(Screen.width / 2 - 100, 400f, 200f, 30f), mouselook.inversion, "Look Inversion");
			GUI.skin.label.fontSize = 14;
			GUI.Label(new Rect(Screen.width / 2 - 100, 370f, 100f, 30f), "Sensitivity: " + mouselook.sensitivityX);
			mouselook.sensitivityX = (int)GUI.HorizontalSlider(new Rect(Screen.width / 2 - 5, 380f, 100f, 20f), mouselook.sensitivityX, 1f, 20f);
			mouselook.sensitivityY = mouselook.sensitivityX;
			PlayerPrefs.SetFloat("sensitivity", mouselook.sensitivityX);
		}
		else
		{
			if (currentMenu != MenuGUIStates.ban)
			{
				return;
			}
			baseBanListIndex = GUI.VerticalScrollbar(new Rect(Screen.width - 150, 60f, 20f, (float)Screen.height / 1.6f), baseBanListIndex, 1f, 0f, Network.connections.Length);
			for (int j = 0; j < Network.connections.Length; j++)
			{
				NetworkPlayer networkPlayer = Network.connections[j];
				if (j < Players.Count && Players[j] != null)
				{
					GUI.skin.label.fontSize = 16;
					GUI.skin.label.alignment = TextAnchor.UpperCenter;
					GUI.Label(new Rect(Screen.width / 2 - 400, 120f + 30f * (0f - baseBanListIndex + (float)j), 250f, 40f), Players[j] + " - " + networkPlayer.ipAddress);
					if (GUI.Button(new Rect(Screen.width / 2 - 125, 120f + 30f * (0f - baseBanListIndex + (float)j), 250f, 28f), "Kick " + Players[j]))
					{
						BannedPlayers.Add(PlayerIPs[j]);
						Players.RemoveAt(j);
						PlayerIPs.RemoveAt(j);
						Network.CloseConnection(networkPlayer, true);
						j = Network.connections.Length;
					}
				}
			}
			if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200f, 60f), "Back"))
			{
				currentMenu = MenuGUIStates.options;
			}
		}
	}
}
public static class Menu
{
	public static Food.FoodType[] Citizen = new Food.FoodType[4]
	{
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.lettuce,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[] Family = new Food.FoodType[7]
	{
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.bun,
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.lettuce,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[] Worker = new Food.FoodType[5]
	{
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[] President = new Food.FoodType[6]
	{
		Food.FoodType.cheese,
		Food.FoodType.patty,
		Food.FoodType.lettuce,
		Food.FoodType.tomato,
		Food.FoodType.tomato,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[] Mayor = new Food.FoodType[7]
	{
		Food.FoodType.patty,
		Food.FoodType.lettuce,
		Food.FoodType.tomato,
		Food.FoodType.tomato,
		Food.FoodType.bacon,
		Food.FoodType.bacon,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[] Boss = new Food.FoodType[6]
	{
		Food.FoodType.patty,
		Food.FoodType.cheese,
		Food.FoodType.patty,
		Food.FoodType.bacon,
		Food.FoodType.bacon,
		Food.FoodType.topBun
	};
	
	public static Food.FoodType[][] Items = new Food.FoodType[6][] { Citizen, Family, Worker, President, Mayor, Boss };
	
	public static string[] ItemNames = new string[6] { "Citizen", "Family", "Worker", "President", "Mayor", "Boss" };
	
	public static Material GetFoodMaterial(string food)
	{
		return Resources.Load("UI/Materials/" + food) as Material;
	}
	
	public static float ScoreFood(string foodItem, Food foodToCompare)
	{
		float num = 0f;
		float num2 = 0f;
		if ((bool)foodToCompare.transform.FindChild("burger-bottom"))
		{
			List<Food> list = new List<Food>(foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
			                                 .foodOnBurger);
			Food.FoodType[] array;
			switch (foodItem)
			{
			default:
				num2 = 4f;
				array = new Food.FoodType[Citizen.Length - 1];
				Array.Copy(Citizen, array, Citizen.Length - 1);
				break;
			case "Citizen":
				num2 = 4f;
				array = new Food.FoodType[Citizen.Length - 1];
				Array.Copy(Citizen, array, Citizen.Length - 1);
				break;
			case "Family":
				num2 = 8f;
				array = new Food.FoodType[Family.Length - 1];
				Array.Copy(Family, array, Family.Length - 1);
				break;
			case "Worker":
				num2 = 6f;
				array = new Food.FoodType[Worker.Length - 1];
				Array.Copy(Worker, array, Worker.Length - 1);
				break;
			case "President":
				num2 = 5f;
				array = new Food.FoodType[President.Length - 1];
				Array.Copy(President, array, President.Length - 1);
				break;
			case "Mayor":
				num2 = 5f;
				array = new Food.FoodType[Mayor.Length - 1];
				Array.Copy(Mayor, array, Mayor.Length - 1);
				break;
			case "Boss":
				num2 = 6f;
				array = new Food.FoodType[Boss.Length - 1];
				Array.Copy(Boss, array, Boss.Length - 1);
				break;
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				Food food = list[num3];
				float num4 = 0f;
				for (int i = 0; i < array.Length; i++)
				{
					if (food.type != array[i])
					{
						continue;
					}
					Debug.Log(string.Concat(food.type, "[", num3, "] matches GF[", i, "]"));
					num4 = 1f;
					array.SetValue(null, i);
					switch (food.type)
					{
					case Food.FoodType.bacon:
						num += 0.5f * num4;
						if (food.cooked < 0.8f)
						{
							num *= food.cooked;
						}
						num *= 1f - food.overcooked;
						break;
					case Food.FoodType.tomato:
						num += 0.5f * num4;
						num *= 1f - food.cooked;
						num *= 1f - food.overcooked;
						break;
					case Food.FoodType.patty:
						num += 2f * num4;
						if (food.cooked < 0.8f)
						{
							num *= food.cooked;
						}
						num *= 1f - food.overcooked;
						break;
					default:
						num += 1f * num4;
						num *= 1f - food.cooked;
						num *= 1f - food.overcooked;
						break;
					}
					list.Remove(food);
					break;
				}
			}
			foreach (Food item in list)
			{
				if (item.type != Food.FoodType.topBun)
				{
					num -= 1f;
				}
			}
		}
		num = Mathf.Round(num);
		Debug.Log("This food scores " + num + " out of " + num2);
		return num;
	}
	
	public static bool CompareAgainstFood(string foodItem, Food foodToCompare)
	{
		bool result = true;
		switch (foodItem)
		{
		case "Citizen":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger3 = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger3.Count == Citizen.Length)
				{
					for (int k = 0; k < foodOnBurger3.Count; k++)
					{
						if (foodOnBurger3[k].type != Citizen[k])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		case "Family":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger5 = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger5.Count == Family.Length)
				{
					for (int m = 0; m < foodOnBurger5.Count; m++)
					{
						if (foodOnBurger5[m].type != Family[m])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		case "Worker":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger4 = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger4.Count == Worker.Length)
				{
					for (int l = 0; l < foodOnBurger4.Count; l++)
					{
						if (foodOnBurger4[l].type != Worker[l])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		case "President":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger6 = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger6.Count == President.Length)
				{
					for (int n = 0; n < foodOnBurger6.Count; n++)
					{
						if (foodOnBurger6[n].type != President[n])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		case "Mayor":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger2 = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger2.Count == Mayor.Length)
				{
					for (int j = 0; j < foodOnBurger2.Count; j++)
					{
						if (foodOnBurger2[j].type != Mayor[j])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		case "Boss":
			if (foodToCompare.type == Food.FoodType.bun)
			{
				List<Food> foodOnBurger = foodToCompare.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.foodOnBurger;
				if (foodOnBurger.Count == Boss.Length)
				{
					for (int i = 0; i < foodOnBurger.Count; i++)
					{
						if (foodOnBurger[i].type != Boss[i])
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			break;
		}
		return result;
	}
}
