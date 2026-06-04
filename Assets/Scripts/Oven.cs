using UnityEngine;

public class Oven : MonoBehaviour
{
	public AudioClip sfxOvenCooking;

	private void OnTriggerEnter(Collider other)
	{
		if (sfxOvenCooking != null)
		{
			AudioSource.PlayClipAtPoint(sfxOvenCooking, base.transform.position);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)other.GetComponent<Food>())
		{
			Food component = other.GetComponent<Food>();
			component.cook();
			if (component.cookSpeedModifier != 0.5f)
			{
				component.cookSpeedModifier = 0.5f;
			}
		}
	}
}
