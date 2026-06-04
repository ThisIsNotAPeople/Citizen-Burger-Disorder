using System.Collections.Generic;
using UnityEngine;

public class BurgerStacking : MonoBehaviour
{
	public List<Food> foodOnBurger = new List<Food>();

	private LayerMask layerMask;

	private Vector3 originalPosition;

	private bool lateUpdateRequired;

	private int checkCount;

	private int maxChecks = 4;

	private bool complete;

	private void Start()
	{
		layerMask = 1 << LayerMask.NameToLayer("Food");
		originalPosition = base.transform.localPosition;
	}

	public void Reset()
	{
		base.collider.enabled = true;
		base.transform.localPosition = originalPosition;
		foodOnBurger.Clear();
	}

	private void LateUpdate()
	{
		if (Time.frameCount % 30 == 0 && checkCount < maxChecks)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.position + base.transform.up * 2f, Vector3.down, out hitInfo, 6f, layerMask) && hitInfo.transform != base.transform && hitInfo.transform.IsChildOf(base.transform.root))
			{
				Vector3 position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
				if (position.y > base.transform.position.y + 0.1f)
				{
					base.transform.position = position;
				}
			}
			checkCount++;
		}
		RaycastHit hitInfo2;
		if (lateUpdateRequired && foodOnBurger.Count > 0 && Physics.Raycast(base.transform.position + base.transform.up * 2f, Vector3.down, out hitInfo2, 6f, layerMask) && hitInfo2.transform.IsChildOf(base.transform.root))
		{
			Vector3 position2 = new Vector3(base.transform.position.x, hitInfo2.point.y, base.transform.position.z);
			if (position2.y > base.transform.position.y)
			{
				base.transform.position = position2;
			}
		}
		lateUpdateRequired = false;
	}

	private void OnTriggerStay(Collider other)
	{
		if (!base.collider.enabled || !other.collider.enabled || !(other.GetComponent<Food>() != null) || (other.name.Contains("bun-bottom") && (!other.name.Contains("bun-bottom") || foodOnBurger.Count <= 0)) || (other.name.Contains("bun-top") && (!other.name.Contains("bun-top") || foodOnBurger.Count <= 0)) || !(other.tag == "PhysicsFood") || !other.rigidbody || !(other.GetComponent<Food>() != null) || base.transform.parent.parent.GetComponent<PickupObject>().beingHeld || !other.rigidbody.useGravity || !(other.GetComponent<Food>().ignoreTriggerDelay <= 0f))
		{
			return;
		}
		if (Network.peerType == NetworkPeerType.Server)
		{
			Food component = other.GetComponent<Food>();
			if (other.collider != null && base.transform.parent.parent.rigidbody != null && other.GetComponent<PickupObject>().lastPlayerHolding != null)
			{
				base.transform.parent.parent.networkView.RPC("AddFoodToBurger", RPCMode.AllBuffered, other.networkView.viewID);
				other.transform.rotation = Quaternion.Euler(Mathf.Clamp(other.transform.rotation.eulerAngles.x, -2.5f, 2.5f), other.transform.rotation.eulerAngles.y, Mathf.Clamp(other.transform.rotation.eulerAngles.z, -2.5f, 2.5f));
				if (component.snapsToCentreInBurger)
				{
					other.transform.position = base.transform.position + other.transform.up * (other.transform.collider.bounds.size.y / 3f);
				}
				else
				{
					other.transform.position = new Vector3(Mathf.Clamp(other.transform.position.x, base.transform.position.x - base.transform.localScale.x / 3f, base.transform.position.x + base.transform.localScale.x / 3f), base.transform.position.y, Mathf.Clamp(other.transform.position.z, base.transform.position.z - base.transform.localScale.z / 3f, base.transform.position.z + base.transform.localScale.z / 3f));
				}
				if (other.name.Contains("bun-bottom"))
				{
					base.transform.parent.parent.networkView.RPC("MoveFoodFromBurger", RPCMode.AllBuffered, other.networkView.viewID);
				}
				if (other.name.Contains("rat"))
				{
					other.GetComponent<Rat>().enabled = false;
				}
				component.inFood = true;
				other.networkView.RPC("SetObjectPosition", RPCMode.All, other.transform.position, other.transform.rotation, other.networkView.viewID);
				other.networkView.RPC("SetParent", RPCMode.All, base.transform.parent.parent.networkView.viewID, "burger");
				other.networkView.RPC("DestroyRigidbody", RPCMode.All, other.networkView.viewID);
				other.networkView.RPC("SetActive", RPCMode.All, other.networkView.viewID, false);
				other.networkView.RPC("SetObservedToTransform", RPCMode.All, other.networkView.viewID);
				if (base.collider.enabled)
				{
					lateUpdateRequired = true;
				}
				checkCount = 0;
			}
			if (foodCount(Food.FoodType.topBun) > 0)
			{
				base.collider.enabled = false;
				base.enabled = false;
			}
		}
		else
		{
			Food component2 = other.GetComponent<Food>();
			if (component2.snapsToCentreInBurger)
			{
				other.transform.position = base.transform.position + other.transform.up * (other.transform.collider.bounds.size.y / 3f);
			}
			else
			{
				other.transform.position = new Vector3(Mathf.Clamp(other.transform.position.x, base.transform.position.x - base.transform.localScale.x / 3f, base.transform.position.x + base.transform.localScale.x / 3f), base.transform.position.y, Mathf.Clamp(other.transform.position.z, base.transform.position.z - base.transform.localScale.z / 3f, base.transform.position.z + base.transform.localScale.z / 3f));
			}
		}
	}

	public int foodCount(Food.FoodType type)
	{
		int num = 0;
		foreach (Food item in foodOnBurger)
		{
			if (item.type == type)
			{
				num++;
			}
		}
		return num;
	}
}
