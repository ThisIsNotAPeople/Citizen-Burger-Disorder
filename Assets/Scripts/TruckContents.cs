using System.Collections.Generic;
using UnityEngine;

public class TruckContents : MonoBehaviour
{
	private List<Collider> objectsInsideTruck = new List<Collider>();

	private void Start()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag.Contains("Physics"))
		{
			objectsInsideTruck.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (objectsInsideTruck.Contains(other))
		{
			objectsInsideTruck.Remove(other);
		}
	}

	public void DestroyBoxesInsideTruck()
	{
		Collider[] array = objectsInsideTruck.ToArray();
		foreach (Collider collider in array)
		{
			if (!(collider != null))
			{
				continue;
			}
			MonoBehaviour.print(collider.name);
			if (collider.tag.Contains("Physics"))
			{
				Debug.DrawLine(base.transform.position, collider.transform.position, Color.blue, 40f);
				if (collider.gameObject.GetComponent<PickupObject>().playerHolding == null)
				{
					collider.GetComponent<PickupObject>().DestroyObject();
				}
			}
		}
		objectsInsideTruck.Clear();
	}
}
