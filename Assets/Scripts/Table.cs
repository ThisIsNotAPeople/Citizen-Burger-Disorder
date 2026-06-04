using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
	public List<string> foodOrder = new List<string>();

	public NPC npcAtTable;

	public TableNodes myTableNode;

	public Transform prefSpeech;

	public Transform reward;

	private Transform speechBubble;

	public int tableNumber;

	private Computer mainComputer;

	private void OnPlayerConnected(NetworkPlayer player)
	{
		if (foodOrder.Count > 0)
		{
			base.networkView.RPC("GetSyncFoodOrder", RPCMode.Others, base.networkView.viewID, foodOrder[0]);
		}
	}

	private void Update()
	{
		if (speechBubble != null && (speechBubble.position - base.transform.position + base.transform.up * 4f).magnitude > 0.01f)
		{
			speechBubble.position = Vector3.Lerp(speechBubble.position, base.transform.position + base.transform.up * 2f, 3f * Time.deltaTime);
		}
		if (foodOrder != null && foodOrder.Count > 0)
		{
			if (speechBubble == null && (Camera.main.transform.position - base.transform.position).magnitude < 15f)
			{
				CreateSpeechBubble();
			}
			else if (speechBubble != null && (Camera.main.transform.position - base.transform.position).magnitude >= 15f)
			{
				DestroySpeechBubble();
			}
		}
	}

	[RPC]
	public void ClearOrder()
	{
		MonoBehaviour.print("Clearing food order");
		if (speechBubble != null && base.networkView.isMine)
		{
			DestroySpeechBubble();
			if (mainComputer == null)
			{
				mainComputer = GameObject.Find("!Monitor").GetComponent<Computer>();
			}
			mainComputer.ClearFoodFromTable(tableNumber - 1, foodOrder[0]);
		}
		foodOrder.Clear();
	}

	private void CreateSpeechBubble(string order = null)
	{
		if (order == null)
		{
			order = foodOrder[0].ToLower();
		}
		Transform transform = (Transform)Object.Instantiate(prefSpeech, base.transform.position + base.transform.up, base.transform.rotation);
		speechBubble = transform;
		speechBubble.GetChild(0).transform.renderer.material = Resources.Load("UI/Materials/" + order) as Material;
	}

	[RPC]
	private void CreateSpeechBubble(NetworkViewID targetID, string order = null)
	{
		Transform transform = NetworkView.Find(targetID).transform;
		Table component = transform.GetComponent<Table>();
		if (order == null)
		{
			order = component.foodOrder[0].ToLower();
		}
		Transform transform2 = (Transform)Object.Instantiate(prefSpeech, transform.transform.position + transform.transform.up, transform.transform.rotation);
		speechBubble = transform2;
		speechBubble.GetChild(0).transform.renderer.material = Resources.Load("UI/Materials/" + order) as Material;
	}

	private void DestroySpeechBubble()
	{
		if (speechBubble != null)
		{
			Object.Destroy(speechBubble.gameObject);
		}
	}

	[RPC]
	private void DestroySpeechBubble(NetworkViewID targetID)
	{
		Transform transform = NetworkView.Find(targetID).transform;
		Table component = transform.GetComponent<Table>();
		if (component.speechBubble != null)
		{
			Object.Destroy(component.speechBubble.gameObject);
		}
	}

	[RPC]
	private void GetSyncFoodOrder(NetworkViewID targetID, string newFoodOrder)
	{
		Table component = NetworkView.Find(targetID).transform.GetComponent<Table>();
		MonoBehaviour.print("Adding: " + newFoodOrder);
		if (component.foodOrder == null)
		{
			component.foodOrder = new List<string>();
		}
		component.foodOrder.Add(newFoodOrder);
	}

	[RPC]
	private void SetNPCAtTable(NetworkViewID tableID, NetworkViewID npcID)
	{
		Table component = NetworkView.Find(tableID).transform.GetComponent<Table>();
		NPC component2 = NetworkView.Find(npcID).transform.GetComponent<NPC>();
		component.npcAtTable = component2;
	}

	[RPC]
	private void RemoveNPCAtTable(NetworkViewID tableID)
	{
		Table component = NetworkView.Find(tableID).transform.GetComponent<Table>();
		component.npcAtTable = null;
	}

	[RPC]
	public void GenerateFoodOrder()
	{
		string text = string.Empty;
		int num = 1;
		foodOrder.Clear();
		for (int i = 0; i < num; i++)
		{
			int num2 = Random.Range(0, Menu.Items.Length);
			string empty = string.Empty;
			foodOrder.Add(Menu.ItemNames[num2]);
			if (mainComputer == null)
			{
				mainComputer = GameObject.Find("!Monitor").GetComponent<Computer>();
			}
			mainComputer.AddFoodToTable(tableNumber - 1, Menu.ItemNames[num2]);
			empty = ((i >= num - 1) ? ".\n" : ",\n");
			text = text + Menu.ItemNames[num2] + empty;
		}
		Transform transform = (Transform)Object.Instantiate(prefSpeech, base.transform.position + base.transform.up, base.transform.rotation);
		speechBubble = transform;
		MonoBehaviour.print(foodOrder[0].ToLower());
		speechBubble.GetChild(0).transform.renderer.material = Resources.Load("UI/Materials/" + foodOrder[0].ToLower()) as Material;
		base.networkView.RPC("GetSyncFoodOrder", RPCMode.Others, base.networkView.viewID, foodOrder[0]);
	}

	private bool matchPlateToOrder(List<Food> foodOnPlate)
	{
		return Menu.CompareAgainstFood(foodOrder[0], foodOnPlate[0]);
	}

	private void SpawnMoney(int maxMoney = 3)
	{
		int num = Random.Range(1, maxMoney + 1);
		for (int i = 0; i <= num; i++)
		{
			Transform transform = Network.Instantiate(reward, base.transform.position + base.transform.up * 1.5f, base.transform.rotation, 0) as Transform;
			transform.name = "Tip";
			transform.rigidbody.AddForce(npcAtTable.transform.up * 700f + npcAtTable.transform.forward * 400f + Random.insideUnitSphere * 2f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!Network.isServer || foodOrder == null || foodOrder.Count <= 0 || !other.transform.FindChild("triggerPlate") || !other.transform.rigidbody || !other.transform.rigidbody.useGravity)
		{
			return;
		}
		Plate component = other.transform.FindChild("triggerPlate").GetComponent<Plate>();
		if (component.foodOnPlate.Count <= 0)
		{
			return;
		}
		if (matchPlateToOrder(component.foodOnPlate))
		{
			StartCoroutine(NetworkSend.Send(component.transform.parent.GetComponent<PickupObject>().lastPlayerHolding.username, "OrdersCompleted", "1"));
		}
		float num = Menu.ScoreFood(foodOrder[0], component.foodOnPlate[0]);
		num = Mathf.Round(num * 1.3f);
		num *= 0.5f;
		if (Random.value > 0.8f)
		{
			num += 1f;
		}
		SpawnMoney(Mathf.RoundToInt(num));
		if (speechBubble != null)
		{
			DestroySpeechBubble();
		}
		npcAtTable.setWants(NPC.wants.toLeave);
		npcAtTable.inside = false;
		npcAtTable.FindExit(false);
		npcAtTable.networkView.RPC("FollowNoPlayer", RPCMode.All);
		myTableNode.GetComponent<TableNodes>().networkView.RPC("SetOccupied", RPCMode.AllBuffered, false);
		other.renderer.material.SetFloat("_Blend", 1f);
		component.foodOnPlate.Clear();
		base.networkView.RPC("ClearOrder", RPCMode.All);
		npcAtTable = null;
		BurgerStacking[] componentsInChildren = other.transform.GetComponentsInChildren<BurgerStacking>();
		BurgerStacking[] array = componentsInChildren;
		foreach (BurgerStacking burgerStacking in array)
		{
			List<Food> foodOnBurger = burgerStacking.foodOnBurger;
			foreach (Food item in foodOnBurger)
			{
				item.GetComponent<PickupObject>().DestroyObject();
			}
		}
		foreach (Transform item2 in other.transform)
		{
			if (item2.tag.Equals("PhysicsFood"))
			{
				MonoBehaviour.print(item2.gameObject.name);
				item2.gameObject.GetComponent<PickupObject>().DestroyObject();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
	}
}
