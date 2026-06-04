using UnityEngine;

public class SettingColor : MonoBehaviour
{
	private Color c;

	private void Start()
	{
	}

	[RPC]
	private void useColorNetwork(NetworkViewID id)
	{
		Transform transform = NetworkView.Find(id).transform;
		transform.renderer.material.SetColor("_Color", c);
	}

	[RPC]
	private void setColorNetwork(float r, float g, float b, NetworkViewID id)
	{
		c = new Color(r, g, b);
		Transform transform = NetworkView.Find(id).transform;
		transform.renderer.material.SetColor("_Color", c);
	}

	private void setColor(float r, float g, float b)
	{
		c = new Color(r, g, b);
	}
}
