using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
	public List<Food> foodOnPlate = new List<Food>();

	private void OnTriggerEnter(Collider other)
	{
		if (!other.name.Contains("bun-bottom") || other.transform.GetChild(0).FindChild("triggerBunStack").GetComponent<BurgerStacking>()
			.foodOnBurger.Count <= 0 || other.transform.GetChild(0).FindChild("triggerBunStack").GetComponent<BurgerStacking>()
			.foodCount(Food.FoodType.topBun) <= 0 || foodOnPlate.Count != 0 || !other.rigidbody || !other.GetComponent<Food>() || other.GetComponent<PickupObject>().beingHeld || !(other.transform.parent != base.transform.parent) || !base.transform.parent.rigidbody.useGravity)
		{
			return;
		}
		MonoBehaviour.print("yeaaaah!");
		Food component = other.GetComponent<Food>();
		if (!foodOnPlate.Contains(component))
		{
			if (Network.isServer)
			{
				other.networkView.RPC("AddFoodToPlate", RPCMode.AllBuffered, other.networkView.viewID, base.transform.parent.networkView.viewID);
			}
			if (Network.isServer)
			{
				other.networkView.RPC("SetObjectPosition", RPCMode.All, other.transform.position, other.transform.rotation, other.networkView.viewID);
				other.networkView.RPC("SetParent", RPCMode.All, base.transform.parent.networkView.viewID, "plate");
				other.networkView.RPC("DestroyRigidbody", RPCMode.All, other.networkView.viewID);
				other.networkView.RPC("SetActive", RPCMode.All, other.networkView.viewID, false);
				other.networkView.RPC("SetObservedToTransform", RPCMode.All, other.networkView.viewID);
			}
			base.transform.parent.renderer.material.SetFloat("_Blend", 0.2f);
			if (component.type == Food.FoodType.bun)
			{
				other.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>()
					.enabled = false;
			}
		}
	}

	public int foodCount(Food.FoodType type)
	{
		int num = 0;
		foreach (Food item in foodOnPlate)
		{
			if (item.type == type)
			{
				num++;
			}
		}
		return num;
	}

	private void Update()
	{
	}
}
