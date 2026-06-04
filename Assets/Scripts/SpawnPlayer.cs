using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
	public Transform player;

	private bool disconnected;

	private string disconnectedMessage = string.Empty;

	private void OnLevelWasLoaded(int level)
	{
		if (level == 1)
		{
			MonoBehaviour.print("Level 1 was loaded");
			Spawn();
			Network.isMessageQueueRunning = true;
			if (Network.isServer)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Spawner");
				foreach (GameObject gameObject in array)
				{
					gameObject.GetComponent<SpawnNPC>().enabled = true;
				}
			}
		}
		Screen.lockCursor = true;
	}

	private void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if (Network.isServer)
		{
			disconnectedMessage = "Local server connection disconnected.";
			Debug.Log("Local server connection disconnected");
		}
		else if (info == NetworkDisconnection.LostConnection)
		{
			disconnectedMessage = "Lost connection to server.";
			Debug.Log("Lost connection to the server");
		}
		else
		{
			disconnectedMessage = "Disconnected from server.";
			Debug.Log("Successfully diconnected from the server");
		}
		disconnected = true;
		menu component = Camera.main.GetComponent<menu>();
		component.enabled = true;
		component.currentMenu = menu.MenuGUIStates.pauseMenu;
	}

	private void OnPlayerDisconnected(NetworkPlayer player)
	{
		Screen.lockCursor = false;
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	private void OnGUI()
	{
		if (disconnected)
		{
			GUI.skin.label.normal.textColor = Color.white;
			GUI.Label(new Rect(Screen.width / 2 - 200, 200f, 400f, 100f), disconnectedMessage);
		}
	}

	public void Spawn()
	{
		MonoBehaviour.print("Spawning!");
		Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Camera.main.transform.position = Vector3.zero;
		Transform transform = (Transform)Network.Instantiate(player, base.transform.position, base.transform.rotation, 0);
		if (PlayerPrefs.GetString("Username") == "Kritz")
		{
			base.networkView.RPC("SetPlayerTexture", RPCMode.AllBuffered, transform.networkView.viewID, "Kritz");
		}
		else
		{
			base.networkView.RPC("SetPlayerTexture", RPCMode.AllBuffered, transform.networkView.viewID, Random.Range(1, 8) + "staff");
		}
		menu component = Camera.main.GetComponent<menu>();
		MouseLook component2 = Camera.main.GetComponent<MouseLook>();
		FollowGameObject component3 = Camera.main.GetComponent<FollowGameObject>();
		foreach (Transform item in transform.transform)
		{
			foreach (Transform item2 in item.transform)
			{
				item2.renderer.enabled = false;
			}
			item.renderer.enabled = false;
		}
		component.enabled = false;
		component.currentMenu = menu.MenuGUIStates.pauseMenu;
		component2.enabled = true;
		component3.follow = transform.gameObject;
		component3.enabled = true;
		transform.networkView.RPC("SetUsername", RPCMode.AllBuffered, transform.GetChild(0).FindChild("Username").networkView.viewID, PlayerPrefs.GetString("Username"));
		component.networkView.RPC("GetPlayerUsername", RPCMode.Server, Network.player.externalIP, PlayerPrefs.GetString("Username"));
		if (PlayerPrefs.GetFloat("sensitivity") == 0f)
		{
			PlayerPrefs.SetFloat("sensitivity", 10f);
		}
		component2.sensitivityX = PlayerPrefs.GetFloat("sensitivity");
		component2.sensitivityY = component2.sensitivityX;
		FirstPersonControl component4 = transform.GetComponent<FirstPersonControl>();
		component4.camera = Camera.main;
		Screen.lockCursor = true;
	}

	[RPC]
	private void SetPlayerTexture(NetworkViewID playerID, string textureName)
	{
		Transform transform = NetworkView.Find(playerID).transform;
		transform.renderer.material = Resources.Load("Skins/Materials/" + textureName) as Material;
	}
}
