using UnityEngine;

public class InsideTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag.Equals("NPC"))
		{
			other.GetComponent<NPC>().inside = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag.Equals("NPC"))
		{
			other.GetComponent<NPC>().inside = false;
		}
	}
}
