using UnityEngine;

public class Grill : MonoBehaviour
{
	public AudioClip sfxMeatCooking;

	private void OnTriggerEnter(Collider other)
	{
		if (sfxMeatCooking != null && other.GetComponent<Food>() != null)
		{
			AudioSource.PlayClipAtPoint(sfxMeatCooking, other.transform.position);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)other.GetComponent<Food>())
		{
			Food component = other.GetComponent<Food>();
			component.cook();
			if (component.cookSpeedModifier != 1f)
			{
				component.cookSpeedModifier = 1f;
			}
		}
	}
}
