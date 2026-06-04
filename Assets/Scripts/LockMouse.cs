using UnityEngine;

public class LockMouse : MonoBehaviour
{
	private menu m;

	private MouseLook ml;

	private void Start()
	{
		Screen.lockCursor = false;
		m = Camera.main.GetComponent<menu>();
		ml = Camera.main.GetComponent<MouseLook>();
	}

	private void Update()
	{
		if (m.enabled)
		{
			Screen.lockCursor = false;
		}
		else
		{
			Screen.lockCursor = true;
		}
		if (Network.peerType != 0 && Input.GetKeyDown(KeyCode.Escape))
		{
			m.enabled = !Screen.lockCursor;
			ml.enabled = false;
		}
		if (Input.GetKeyDown (KeyCode.F1) || Input.GetKeyDown (KeyCode.Escape))
		{
			MonoBehaviour.print("opened main menu");
			m.enabled = true;
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			Application.LoadLevel(0);
		}
	}
}
