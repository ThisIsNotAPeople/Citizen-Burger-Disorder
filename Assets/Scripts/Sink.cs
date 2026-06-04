using UnityEngine;

public class Sink : MonoBehaviour
{
	private GameObject bubbles;

	private float speed = 0.1f;

	private void OnTriggerEnter(Collider other)
	{
		bubbles = Resources.Load("Prefabs/Dishwashing/Bubbles") as GameObject;
		if (bubbles != null)
		{
			Object.Instantiate(bubbles, other.transform.position, other.transform.rotation);
		}
		else
		{
			Debug.LogWarning("bubbles not spawned!");
		}
		
		if (Network.isServer)
		{
			if (other.name.Contains("rat"))
			{
				Rat rat = other.GetComponent<Rat>();
				
				if (rat != null)
				{
					rat.networkView.RPC("GiveUp", RPCMode.All, base.networkView.viewID);
					rat.enabled = false;
				}
			}
			
			Flamable flamable = other.GetComponent<Flamable>();
			
			if (flamable != null)
			{
				MonoBehaviour.print("Yo");
				flamable.FireBurnOut();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag.Equals("Physics") && other.renderer.material.name.Contains("plate"))
		{
			float @float = other.renderer.material.GetFloat("_Blend");
			if (@float > 0f)
			{
				other.renderer.material.SetFloat("_Blend", Mathf.Max(0f, @float - Time.deltaTime * speed));
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.renderer != null && other.renderer.material.name.Contains("StaffMenuTex"))
		{
			other.GetComponent<DrawTexture>().NewTex();
		}
	}

	private void Start()
	{
		bubbles = Resources.Load("Prefabs/Dishwashing/Bubbles") as GameObject;
	}

	private void Update()
	{
	}
}
