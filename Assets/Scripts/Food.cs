using UnityEngine;

public class Food : MonoBehaviour
{
	public enum FoodType
	{
		physics = 0,
		patty = 1,
		potato = 2,
		topBun = 3,
		bun = 4,
		lettuce = 5,
		cheese = 6,
		tomato = 7,
		bacon = 8,
		pineapple = 9,
		rat = 10,
		other = 11
	}

	public FoodType type;

	private BurgerStacking burgerStack;

	private bool ovenCooked;

	private bool grillCooked;

	public float cooked;

	private float cookedDelay;

	public float overcooked;

	public float cookTimeIdeal = 10f;

	public float cookTimeBurnDelay = 10f;

	public float cookTimeBurned = 10f;

	public float foodTemp = 20f;

	private float startFoodTemp;

	private float maxFoodTemp = 120f;

	private float minFoodTemp = -20f;

	public bool supportsTextureBlend;

	private Color highlightColor;

	private Color originalColor;

	public float textureToKeep = 0.5f;

	public float cookedRed = 0.2f;

	public float cookedGreen;

	public float cookedBlue;

	public float cookSpeedModifier = 1f;

	public bool inFood;

	public bool snapsToCentreInBurger = true;

	public float ignoreTriggerDelay;

	public bool foodBeenOnFloor;

	public Rat beingHeldByRat;

	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncAllFood", player, base.networkView.viewID, cooked, cookedDelay, overcooked, cookedRed, cookedBlue, cookedGreen, foodTemp, foodBeenOnFloor);
	}

	public void CallSyncFood()
	{
		if (Network.isServer)
		{
			base.networkView.RPC("SyncAllFood", RPCMode.Others, base.networkView.viewID, cooked, cookedDelay, overcooked, cookedRed, cookedBlue, cookedGreen, foodTemp, foodBeenOnFloor);
		}
	}

	[RPC]
	private void SyncAllFood(NetworkViewID objectID, float nCooked, float nCookedDelay, float nOvercooked, float nRed, float nBlue, float nGreen, float nFoodTemp, bool nFoodBeenOnFloor)
	{
		//Discarded unreachable code: IL_001d
		Food component;
		try
		{
			component = NetworkView.Find(objectID).GetComponent<Food>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		component.cooked = nCooked;
		component.cookedDelay = nCookedDelay;
		component.overcooked = nOvercooked;
		component.foodTemp = nFoodTemp;
		component.foodBeenOnFloor = nFoodBeenOnFloor;
		component.cookedRed = nRed;
		component.cookedBlue = nBlue;
		component.cookedGreen = nGreen;
		component.UpdateMaterial(true);
	}

	private void Awake()
	{
		if (base.renderer != null && base.renderer.material != null)
		{
			originalColor = base.renderer.material.color;
			base.renderer.material.color = Color.Lerp(originalColor, new Color(cookedRed, cookedGreen, cookedBlue), cooked * cookSpeedModifier * (1f - textureToKeep));
		}
		burgerStack = GetBurgerStack();
		startFoodTemp = foodTemp;
	}

	public BurgerStacking GetBurgerStack()
	{
		if (burgerStack == null)
		{
			if (type == FoodType.bun && (bool)base.transform.FindChild("burger-bottom").GetChild(0))
			{
				burgerStack = base.transform.FindChild("burger-bottom").FindChild("triggerBunStack").GetComponent<BurgerStacking>();
			}
			else if (type != FoodType.bun && inFood)
			{
				MonoBehaviour.print("Set burger stacking for " + base.transform.name);
				burgerStack = base.transform.parent.FindChild("triggerBunStack").GetComponent<BurgerStacking>();
			}
		}
		return burgerStack;
	}

	public void Highlight(Color highlightColor)
	{
		if (base.renderer != null && base.renderer.material != null)
		{
			base.renderer.material.color = highlightColor;
		}
	}

	public void cook()
	{
		foodTemp += Time.deltaTime;
		UpdateMaterial();
	}

	private void UpdateMaterial(bool instant = false)
	{
		if (!base.renderer)
		{
			return;
		}
		if (!instant)
		{
			if (cooked < 1f)
			{
				if (!supportsTextureBlend)
				{
					base.renderer.material.color = Color.Lerp(originalColor, new Color(cookedRed, cookedGreen, cookedBlue), cooked);
				}
				else
				{
					base.renderer.material.SetFloat("_Blend", cooked);
				}
				cooked += Time.deltaTime / cookTimeIdeal;
			}
			else if (cookedDelay < 1f)
			{
				cookedDelay += Time.deltaTime / cookTimeBurnDelay;
			}
			else if (overcooked < 1f)
			{
				base.renderer.material.color = Color.Lerp(base.renderer.material.color, new Color(0.005f, 0f, 0f), overcooked * cookSpeedModifier / 80f);
				overcooked += Time.deltaTime / cookTimeBurned;
			}
		}
		else
		{
			base.renderer.material.color = Color.Lerp(originalColor, new Color(cookedRed, cookedGreen, cookedBlue), cooked);
			base.renderer.material.color = Color.Lerp(base.renderer.material.color, new Color(0.005f, 0f, 0f), overcooked * cookSpeedModifier / 80f);
		}
	}

	[RPC]
	private void MoveFoodFromBurger(NetworkViewID otherBurgerTransformID)
	{
		Transform transform = NetworkView.Find(otherBurgerTransformID).transform;
		Transform transform2 = transform.transform.FindChild("burger-bottom").FindChild("triggerBunStack");
		burgerStack.foodOnBurger.AddRange(transform2.GetComponent<BurgerStacking>().foodOnBurger);
		burgerStack.transform.position = transform2.position;
		transform2.collider.enabled = false;
		transform2.GetComponent<BurgerStacking>().enabled = false;
		transform2.GetComponent<BurgerStacking>().foodOnBurger.Clear();
	}

	[RPC]
	private void AddFoodToBurger(NetworkViewID foodTransformID)
	{
		Transform transform = NetworkView.Find(foodTransformID).transform;
		Food component = transform.GetComponent<Food>();
		burgerStack.foodOnBurger.Add(component);
	}

	[RPC]
	private void AddFoodToPlate(NetworkViewID foodTransformID, NetworkViewID plateTransformID)
	{
		Transform transform = NetworkView.Find(foodTransformID).transform;
		Plate component = NetworkView.Find(plateTransformID).transform.FindChild("triggerPlate").GetComponent<Plate>();
		component.foodOnPlate.Add(transform.GetComponent<Food>());
	}

	[RPC]
	private void SetObservedToTransform(NetworkViewID id)
	{
		Transform transform = NetworkView.Find(id).transform;
		transform.GetComponent<NetworkView>().observed = transform.transform;
	}

	[RPC]
	private void SetObservedToNetObj(NetworkViewID id)
	{
		Transform transform = NetworkView.Find(id).transform;
		if ((bool)GetComponent<PickupObject>())
		{
			transform.GetComponent<PickupObject>().netObject.states = new NetworkObject.State[20];
			transform.GetComponent<NetworkView>().observed = transform.GetComponent<NetworkObject>();
		}
		else
		{
			transform.GetComponent<NetworkView>().observed = transform.GetComponent<NetworkObject>();
		}
	}

	[RPC]
	private void SetCollider(bool state)
	{
		base.collider.enabled = state;
	}

	[RPC]
	private void DestroyRigidbody(NetworkViewID tID)
	{
		Transform transform = NetworkView.Find(tID).transform;
		Object.Destroy(transform.gameObject.rigidbody);
	}

	[RPC]
	private void SetParent(NetworkViewID parentID, string type = "")
	{
		Transform parent = ((!(type == "burger")) ? NetworkView.Find(parentID).transform : NetworkView.Find(parentID).transform.GetChild(0));
		base.transform.parent = parent;
	}

	[RPC]
	private void SetActive(NetworkViewID tID, bool active)
	{
		Transform transform = NetworkView.Find(tID).transform;
		transform.GetComponent<Food>().enabled = active;
		transform.GetComponent<NetworkObject>().enabled = active;
		if ((bool)transform.GetComponent<PickupObject>())
		{
			transform.GetComponent<PickupObject>().enabled = active;
		}
	}

	public bool getGrillCooked()
	{
		return grillCooked;
	}

	public bool getOvenCooked()
	{
		return ovenCooked;
	}

	public float getCooked()
	{
		return cooked;
	}

	public float getOvercooked()
	{
		return overcooked;
	}

	public void setCookingSpeedModifier(float f)
	{
		cookSpeedModifier = f;
	}

	public void setOvenCooked(bool b)
	{
		ovenCooked = b;
	}

	public void setGrillCooked(bool b)
	{
		grillCooked = b;
	}

	private void Update()
	{
		if (ignoreTriggerDelay > 0f)
		{
			ignoreTriggerDelay = Mathf.Max(0f, ignoreTriggerDelay - Time.deltaTime);
		}
		foodTemp = Mathf.Lerp(foodTemp, startFoodTemp, 0.01f * Time.deltaTime);
	}

	[RPC]
	public void BurgerExplosion(float force, NetworkViewID targetID)
	{
		Transform transform = NetworkView.Find(targetID).transform;
		Food component = transform.GetComponent<Food>();
		MonoBehaviour.print("Getting burger stack for " + transform.name + " with " + burgerStack.foodOnBurger.Count + " items");
		int count = burgerStack.foodOnBurger.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			MonoBehaviour.print(burgerStack.foodOnBurger[num].transform.name + " is exploding " + num);
			Transform transform2 = burgerStack.foodOnBurger[num].transform;
			if (transform2.tag.Contains("Food"))
			{
				if (base.networkView.isMine)
				{
					base.networkView.RPC("SetActive", RPCMode.All, transform2.networkView.viewID, true);
					base.networkView.RPC("SetObservedToNetObj", RPCMode.All, transform2.networkView.viewID);
					if (transform2.GetComponent<Food>().type == FoodType.bun && num > 0)
					{
						base.networkView.RPC("BurgerExplosion", RPCMode.All, force, transform2.networkView.viewID);
					}
				}
				if (transform2.name.Contains("rat"))
				{
					transform2.GetComponent<Rat>().enabled = true;
				}
				transform2.parent = null;
				transform2.GetComponent<Food>().ignoreTriggerDelay = 1f;
				if (!transform2.rigidbody)
				{
					transform2.gameObject.AddComponent<Rigidbody>();
					if (base.networkView.isMine)
					{
						transform2.rigidbody.AddExplosionForce((float)(num / Mathf.Max(count - 1, 1)) * force, transform2.position, component.burgerStack.foodOnBurger.Count);
					}
				}
			}
		}
		component.burgerStack.foodOnBurger.Clear();
		burgerStack.enabled = true;
		burgerStack.Reset();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (type == FoodType.bun && Network.isServer && (bool)base.rigidbody && burgerStack.foodOnBurger.Count > 0 && !(collision.relativeVelocity.magnitude > 7f))
		{
		}
	}
}
