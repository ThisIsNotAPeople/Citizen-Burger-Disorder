using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
	private GraphScript graph;

	public List<NavigationScript> pathToFollow = new List<NavigationScript>();

	private static List<Transform> avoidNodes = new List<Transform>();

	private float maxSpeed = 500f;

	public GameObject exitNode;

	private Vector3 targetLocation = Vector3.zero;

	public PickupObject stolenFood;

	private Transform targetFood;

	private int pathfindLayerMask = -32513;

	private PickupObject myPickupObject;

	private bool wasBeingHeld;

	private float timeStuck;

	private float stuckDuration = 2f;

	private float stuckCheckTime;

	private float stuckCheckWait = 2f;

	private bool defeated;

	private GameObject foodInSight;

	private void Start()
	{
		if (Network.isServer)
		{
			base.networkView.RPC("SetSpeeds", RPCMode.AllBuffered, Random.Range(300f, maxSpeed), Random.Range(300f, maxSpeed));
			graph = GameObject.Find("!RatGraph").GetComponent<GraphScript>();
			myPickupObject = GetComponent<PickupObject>();
			FindExit();
		}
	}

	private void Update()
	{
		if (!Network.isServer)
		{
			return;
		}
		if (!base.rigidbody)
		{
			base.enabled = false;
		}
		if (myPickupObject.beingHeld)
		{
			wasBeingHeld = true;
			targetLocation = Vector3.zero;
		}
		else if (wasBeingHeld)
		{
			wasBeingHeld = false;
			base.networkView.RPC("GiveUp", RPCMode.All, base.networkView.viewID);
			CreatePath(exitNode.transform.position);
		}
		foodInSight = CanSeeFood();
		if ((base.transform.position - targetLocation).magnitude < 3f && !foodInSight)
		{
			targetLocation = Vector3.zero;
		}
		if (pathToFollow == null || pathToFollow.Count == 0)
		{
			if (targetLocation != Vector3.zero)
			{
				CreatePath(targetLocation);
			}
			else
			{
				CreatePath(exitNode.transform.position);
			}
		}
		if ((exitNode.transform.position - base.transform.position).magnitude < 15f)
		{
			if (stolenFood != null)
			{
				base.networkView.RPC("SetStolenFood", RPCMode.Others, base.networkView.viewID, stolenFood.networkView.viewID, false);
				stolenFood.DestroyObject();
			}
			FloorTrigger.currentRats--;
			Network.RemoveRPCs(base.gameObject.networkView.viewID);
			Network.Destroy(base.gameObject);
		}
		if (stolenFood == null && targetFood == null && (bool)foodInSight && !defeated)
		{
			targetFood = foodInSight.transform;
		}
		if ((bool)targetFood && stolenFood == null && !defeated)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero += Seek(targetFood.transform.position);
			zero += Avoid();
			zero2 = zero;
			zero2.y = 0f;
			if (base.networkView.isMine)
			{
				if (zero != Vector3.zero)
				{
					base.transform.rotation = Quaternion.LookRotation(zero2 + base.transform.forward);
				}
				base.rigidbody.AddForce(zero.normalized * maxSpeed * 10f * Time.deltaTime);
			}
			if (defeated || !targetFood || !((base.transform.position - targetFood.position).magnitude < 4f))
			{
				return;
			}
			Food component = targetFood.GetComponent<Food>();
			if ((bool)component.beingHeldByRat && Random.value > 0.5f)
			{
				base.networkView.RPC("GiveUp", RPCMode.All, component.beingHeldByRat.networkView.viewID);
			}
			if ((bool)targetFood && (bool)component)
			{
				component.beingHeldByRat = this;
				base.networkView.RPC("SetStolenFood", RPCMode.All, base.networkView.viewID, targetFood.networkView.viewID, true);
				if (FloorTrigger.foodDropPosition.Contains(targetFood.gameObject))
				{
					FloorTrigger.foodDropPosition.Remove(targetFood.gameObject);
				}
				targetLocation = Vector3.zero;
				CreatePath(exitNode.transform.position);
				targetFood = null;
			}
		}
		else if (pathToFollow != null && pathToFollow.Count > 0)
		{
			Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.z);
			Vector2 vector2 = new Vector2(pathToFollow[0].transform.position.x, pathToFollow[0].transform.position.z);
			float magnitude = (vector - vector2).magnitude;
			if (magnitude < 4f)
			{
				pathToFollow.RemoveAt(0);
			}
			Vector3 zero3 = Vector3.zero;
			Vector3 zero4 = Vector3.zero;
			if (pathToFollow.Count > 0)
			{
				zero3 += Seek(pathToFollow[0].transform.position);
				zero3 += Avoid();
				zero4 = zero3;
				zero4.y = 0f;
			}
			else
			{
				zero3 += Seek(targetLocation);
				zero3 += Avoid();
				zero4 = zero3;
				zero4.y = 0f;
			}
			if (zero3 != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(zero4 + base.transform.forward);
			}
			base.rigidbody.AddForce(zero3.normalized * maxSpeed * 10f * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if ((bool)stolenFood)
		{
			if (Network.isServer && stolenFood.beingHeld)
			{
				base.networkView.RPC("GiveUp", RPCMode.All, base.networkView.viewID);
			}
			stolenFood.transform.position = base.transform.position + base.transform.forward;
		}
	}

	[RPC]
	private void SetStolenFood(NetworkViewID ratID, NetworkViewID foodID, bool ratIsHolding)
	{
		Rat component = NetworkView.Find(ratID).GetComponent<Rat>();
		PickupObject component2 = NetworkView.Find(foodID).GetComponent<PickupObject>();
		if (ratIsHolding)
		{
			component.stolenFood = component2;
			component.stolenFood.rigidbody.useGravity = false;
			component.stolenFood.transform.rigidbody.collider.enabled = false;
			component.stolenFood.GetComponent<Food>().beingHeldByRat = component;
		}
		else
		{
			component.stolenFood.transform.rigidbody.useGravity = true;
			component.stolenFood.transform.rigidbody.collider.enabled = true;
			component.stolenFood.GetComponent<Food>().beingHeldByRat = null;
			component.stolenFood = null;
		}
	}

	[RPC]
	private void GiveUp(NetworkViewID ratToGiveUp)
	{
		Rat component = NetworkView.Find(ratToGiveUp).GetComponent<Rat>();
		if ((bool)component.stolenFood)
		{
			MonoBehaviour.print("Stolen food!");
			base.networkView.RPC("SetStolenFood", RPCMode.All, ratToGiveUp, component.stolenFood.networkView.viewID, false);
		}
		component.targetFood = null;
		component.targetLocation = Vector3.zero;
		component.defeated = true;
	}

	[RPC]
	private void SetSpeeds(float baseSpeed, float maxSpeed)
	{
		this.maxSpeed = maxSpeed;
	}

	private void LateUpdate()
	{
		if (!Network.isServer)
		{
			return;
		}
		if (stuckCheckTime == 0f || timeStuck > 0f)
		{
			if (timeStuck == 0f && !Physics.Raycast(base.transform.position + base.transform.up * 0.5f, base.transform.forward, 1f, pathfindLayerMask))
			{
				timeStuck = Time.time;
				stuckCheckTime = timeStuck;
			}
			if (timeStuck > 0f && Physics.Raycast(base.transform.position + base.transform.up * 0.5f, base.transform.forward, 1f, pathfindLayerMask))
			{
				timeStuck = 0f;
				stuckCheckTime = Time.time;
			}
		}
		if (stuckCheckTime > 0f && Time.time > stuckCheckTime + stuckCheckWait)
		{
			stuckCheckTime = 0f;
		}
		if (timeStuck > 0f && Time.time > timeStuck + stuckDuration)
		{
			if (targetLocation != Vector3.zero)
			{
				CreatePath(targetLocation);
			}
			else
			{
				CreatePath(exitNode.transform.position);
			}
			timeStuck = 0f;
			stuckCheckTime = Time.time;
		}
	}

	[RPC]
	private void SyncFoodInSight(NetworkViewID foodID)
	{
		GameObject gameObject = NetworkView.Find(foodID).gameObject;
		foodInSight = gameObject;
	}

	private GameObject CanSeeFood()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 10f);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if ((bool)collider.GetComponent<Food>() && !collider.name.Contains("rat"))
			{
				Food component = collider.GetComponent<Food>();
				RaycastHit hitInfo;
				if (component.foodBeenOnFloor && !component.inFood && component.GetBurgerStack() == null && component.type != Food.FoodType.bun && Physics.Linecast(base.transform.position, collider.transform.position, out hitInfo, pathfindLayerMask) && hitInfo.collider == collider && Mathf.Abs(base.transform.position.y - collider.transform.position.y) < 10f)
				{
					return collider.gameObject;
				}
			}
		}
		return null;
	}

	private Vector3 Avoid()
	{
		Vector3 zero = Vector3.zero;
		if (avoidNodes == null || avoidNodes.Count == 0)
		{
			foreach (Transform item in GameObject.Find("!AvoidNodes").transform)
			{
				avoidNodes.Add(item);
			}
		}
		foreach (Transform avoidNode in avoidNodes)
		{
			if ((base.transform.position - avoidNode.position).magnitude < 3f)
			{
				zero += (base.transform.position - avoidNode.position).normalized * maxSpeed * 0.5f * ((3f - (base.transform.position - avoidNode.position).magnitude) / 3f);
			}
		}
		return zero;
	}

	private Vector3 Seek(Vector3 Target)
	{
		Vector3 result = Vector3.zero;
		if ((bool)base.rigidbody)
		{
			result = (Target - base.transform.position).normalized * maxSpeed - base.rigidbody.velocity;
		}
		return result;
	}

	public void SetTargetFood(Vector3 targetPos)
	{
		targetLocation = targetPos;
	}

	public void CreatePath(Vector3 targetPos)
	{
		int num = FindClosestVisibleNode(base.transform);
		int goal = FindClosestNode(targetPos);
		if (num == -1)
		{
			MonoBehaviour.print("Can't find closest");
			base.transform.LookAt(graph.nodes[FindClosestNode(base.transform.position)].transform.position);
			num = FindClosestVisibleNode(base.transform);
		}
		pathToFollow = graph.FindPath(num, goal);
	}

	private int FindClosestNode(Vector3 pos)
	{
		float num = 9999f;
		int num2 = -1;
		foreach (NavigationScript node in graph.nodes)
		{
			if ((node.transform.position - pos).magnitude < num)
			{
				num = (node.transform.position - pos).magnitude;
				num2 = graph.nodes[graph.nodes.IndexOf(node)].index;
			}
		}
		if (num2 == -1)
		{
			MonoBehaviour.print("Error: no node found");
		}
		return num2;
	}

	public void FindExit(bool excludeNearestExit = false)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Despawner");
		List<GameObject> list = new List<GameObject>();
		float num = 99999f;
		int index = 0;
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(array[i]);
			if ((base.transform.position - array[i].transform.position).magnitude < num)
			{
				num = (base.transform.position - array[i].transform.position).magnitude;
				index = i;
			}
		}
		if (excludeNearestExit && list.Count > 1)
		{
			list.RemoveAt(index);
		}
		exitNode = list[Random.Range(0, list.Count)];
	}

	private int FindClosestVisibleNode(Transform t)
	{
		float num = 9999f;
		int num2 = -1;
		foreach (NavigationScript node in graph.nodes)
		{
			if (!Physics.Linecast(t.position, node.transform.position, pathfindLayerMask) && (node.transform.position - t.position).magnitude < num && (node.transform.position - base.transform.position).magnitude > 0.5f)
			{
				num = (node.transform.position - t.position).magnitude;
				num2 = graph.nodes[graph.nodes.IndexOf(node)].index;
			}
		}
		if (num2 == -1)
		{
			MonoBehaviour.print("Error: no visible node found");
		}
		return num2;
	}
}
