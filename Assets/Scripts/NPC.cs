using System.Collections.Generic;using UnityEngine;

public class NPC : MonoBehaviour{public enum wants{toExitScene = 0,toEnter = 1,toFindSeat = 2,toGetToSeat = 3,toGetMenu = 4,toLeave = 5,toEat = 6,toIdle = 7}
	
	public Transform playerToFollow;
	
	public Transform nodeToFollow;
	
	public Transform targetToFollow;
	
	private GraphScript graph;
	
	public List<NavigationScript> pathToFollow = new List<NavigationScript>();
	
	private List<Transform> avoidNodes = new List<Transform>();
	
	public bool isFollowingPlayer;
	
	public bool isFollowingTableNode;
	
	private float speed = 60f;
	
	private float maxSpeed = 120f;
	
	private float unsafeDistance = 7f;
	
	private Vector3 randomDistanceFromNodeBias;
	
	private float followWeight = 1f;
	
	private float avoidNodesWeight = 0.6f;
	
	private float avoidNPCsWeight = 0.75f;
	
	public bool inside;
	
	public GameObject exitNode;
	
	public List<GameObject> exitListz = new List<GameObject>();
	
	public TableGraph tableGraph;
	
	public List<NPC> myNPCGroup = new List<NPC>();
	
	public NPC myNPCGroupLeader;
	
	public TableNodes targetSeat;
	
	public wants currentlyWants;
	
	public wants previouslyWanted;
	
	public float idleTimeMin = 5f;
	
	public float idleTimeRand;
	
	public float idleStartTime;
	
	public float waitUntilNextIdle = 5f;
	
	private float linecastCheckDelay = 5f;
	
	private float lastLinecastCheck;
	
	private int timesStuckPathfinding;
	
	public int desiredGroupSize = 2;
	
	private float menuWaitBaseDuration = 120f;
	
	private float menuWaitStartTime;
	
	private float foodWaitBaseDuration = 400f;
	
	public float foodWaitStartTime;
	
	private int layerMask = -28929;
	
	private int pathfindLayerMask = -53505;
	
	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncPosition", RPCMode.Others, base.transform.position, base.transform.rotation);
		if ((bool)base.rigidbody)
		{
			base.networkView.RPC("SyncRigidbody", RPCMode.Others, base.rigidbody.velocity, base.rigidbody.angularVelocity);
		}
	}
	
	[RPC]
	private void SyncPosition(Vector3 pos, Quaternion rot)
	{
		base.transform.position = pos;
		base.transform.rotation = rot;
	}
	
	[RPC]
	private void SyncRigidbody(Vector3 vel, Vector3 aVel)
	{
		if (!base.rigidbody)
		{
			base.gameObject.AddComponent<Rigidbody>();
		}
		base.rigidbody.velocity = vel;
		base.rigidbody.angularVelocity = aVel;
	}
	
	[RPC]
	private void SyncWants(int currentlyWantsID, int previouslyWantsID)
	{
		currentlyWants = (wants)currentlyWantsID;
	}
	
	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 value = base.transform.position;
			Quaternion value2 = base.transform.rotation;
			stream.Serialize(ref value);
			stream.Serialize(ref value2);
		}
		else
		{
			Vector3 value3 = Vector3.zero;
			Quaternion value4 = Quaternion.Euler(0f, 0f, 0f);
			stream.Serialize(ref value3);
			stream.Serialize(ref value4);
			base.transform.position = Vector3.Lerp(base.transform.position, value3, 0.2f);
			base.transform.rotation = value4;
		}
	}
	
	private void Start()
	{
		graph = GameObject.Find("!NavigationGraph").GetComponent<GraphScript>();
		tableGraph = GameObject.Find("!TableNodes").GetComponent<TableGraph>();
		foreach (Transform item in GameObject.Find("!AvoidNodes").transform)
		{
			avoidNodes.Add(item);
		}
		randomDistanceFromNodeBias = Random.insideUnitSphere * 5f;
		randomDistanceFromNodeBias.y = 0f;
		speed *= Random.Range(0.9f, 1.5f);
		FindExit(true);
	}
	
	private void Update()
	{
		if (!Network.isServer)
		{
			return;
		}
		if (currentlyWants == wants.toEnter)
		{
			if (pathToFollow == null || pathToFollow.Count == 0)
			{
				CreatePath(graph.enterance.transform.position);
				return;
			}
			if ((pathToFollow[0].transform.position + randomDistanceFromNodeBias - base.transform.position).magnitude < 10f)
			{
				if (Enterance.npcsWaiting < 6)
				{
					pathToFollow.RemoveAt(0);
				}
				else
				{
					setWants(wants.toExitScene);
					pathToFollow = new List<NavigationScript>();
					FindExit(false);
					CreatePath(exitNode.transform.position);
				}
				if (Random.value > 0.5f)
				{
					randomDistanceFromNodeBias = Random.insideUnitSphere * 5f;
					randomDistanceFromNodeBias.y = 0f;
				}
			}
			if (pathToFollow.Count == 0)
			{
				Enterance.npcsWaiting++;
				setWants(wants.toFindSeat);
				return;
			}
			RaycastHit hitInfo;
			if (Time.time > lastLinecastCheck + linecastCheckDelay && Physics.Linecast(base.transform.position, pathToFollow[0].transform.position, out hitInfo, pathfindLayerMask) && hitInfo.distance < 3f)
			{
				if (timesStuckPathfinding > 4)
				{
					base.gameObject.layer = 16;
				}
				randomDistanceFromNodeBias = Random.insideUnitSphere * 0.2f;
				randomDistanceFromNodeBias.y = 0f;
				CreatePath(graph.enterance.transform.position);
				timesStuckPathfinding++;
				lastLinecastCheck = Time.time;
				return;
			}
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero += Seek(pathToFollow[0].transform.position + randomDistanceFromNodeBias) * followWeight;
			zero2 = zero;
			zero2.y = 0f;
			zero += SeparateAvoidNodes() * avoidNodesWeight;
			zero.y = 0f;
			if (zero != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(zero2 + base.transform.forward);
			}
			base.rigidbody.AddForce(zero.normalized * maxSpeed * 10f * Time.deltaTime);
		}
		else if (currentlyWants == wants.toExitScene)
		{
			if (pathToFollow == null || pathToFollow.Count == 0)
			{
				CreatePath(exitNode.transform.position);
				return;
			}
			if ((pathToFollow[0].transform.position + randomDistanceFromNodeBias - base.transform.position).magnitude < 10f)
			{
				pathToFollow.RemoveAt(0);
				if (Random.value > 0.5f)
				{
					randomDistanceFromNodeBias = Random.insideUnitSphere * 5f;
					randomDistanceFromNodeBias.y = 0f;
				}
			}
			if (pathToFollow.Count == 0)
			{
				SpawnNPC.currentNPCs--;
				Network.RemoveRPCs(base.networkView.viewID);
				Network.Destroy(base.networkView.viewID);
				return;
			}
			RaycastHit hitInfo2;
			if (Time.time > lastLinecastCheck + linecastCheckDelay && Physics.Linecast(base.transform.position, pathToFollow[0].transform.position, out hitInfo2, layerMask) && hitInfo2.distance < 3f)
			{
				if (timesStuckPathfinding > 4)
				{
					base.gameObject.layer = 15;
				}
				randomDistanceFromNodeBias = Random.insideUnitSphere * 0.2f;
				randomDistanceFromNodeBias.y = 0f;
				CreatePath(exitNode.transform.position);
				timesStuckPathfinding++;
				lastLinecastCheck = Time.time;
				return;
			}
			Vector3 zero3 = Vector3.zero;
			Vector3 zero4 = Vector3.zero;
			zero3 += Seek(pathToFollow[0].transform.position + randomDistanceFromNodeBias) * followWeight;
			zero4 = zero3;
			zero4.y = 0f;
			zero3 += SeparateAvoidNodes() * avoidNodesWeight;
			zero3.y = 0f;
			if (zero3 != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(zero4 + base.transform.forward);
			}
			base.rigidbody.AddForce(zero3.normalized * maxSpeed * 10f * Time.deltaTime);
		}
		else if (currentlyWants == wants.toIdle)
		{
			base.transform.rotation = Quaternion.LookRotation(base.transform.forward);
			Vector3 zero5 = Vector3.zero;
			base.rigidbody.AddForce(zero5.normalized * maxSpeed * 10f * Time.deltaTime);
			if (Time.time > idleTimeMin + idleTimeRand + idleStartTime)
			{
				currentlyWants = previouslyWanted;
				if (currentlyWants == wants.toExitScene)
				{
					FindExit(false);
					CreatePath(exitNode.transform.position);
				}
			}
		}
		else if (currentlyWants == wants.toFindSeat)
		{
			List<NPC> list = new List<NPC>();
			int num = 0;
			int num2 = 0;
			list.Add(this);
			if (desiredGroupSize > 1)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("NPC");
				foreach (GameObject gameObject in array)
				{
					if (gameObject != base.gameObject && (base.transform.position - gameObject.transform.position).magnitude < 40f)
					{
						NPC component = gameObject.GetComponent<NPC>();
						if (num2 < desiredGroupSize - 1 && (component.currentlyWants == wants.toFindSeat || component.currentlyWants == wants.toIdle) && component.desiredGroupSize == desiredGroupSize)
						{
							list.Add(component);
							num2++;
						}
					}
				}
			}
			if (list.Count > 1 && list.Count == desiredGroupSize)
			{
				myNPCGroup = list;
				if (tableGraph == null)
				{
					tableGraph = GameObject.Find("!TableNodes").GetComponent<TableGraph>();
				}
				num = TableGraph.FindUnoccupiedTableForGroup(myNPCGroup.Count);
				MonoBehaviour.print("Goal table: " + num);
				if (num >= 0)
				{
					foreach (NPC item in list)
					{
						if (item != this)
						{
							item.myNPCGroupLeader = this;
							item.setWants(wants.toGetToSeat);
						}
						foreach (TableNodes tableNode in TableGraph.tables[num - 1].tableNodes)
						{
							if (!tableNode.occupied)
							{
								item.targetSeat = tableNode;
								tableNode.networkView.RPC("SetOccupied", RPCMode.AllBuffered, true);
								item.setWants(wants.toGetToSeat);
								item.pathToFollow = graph.FindPath(FindClosestNode(item.transform.position), FindClosestVisibleNode(item.targetSeat.transform));
								MonoBehaviour.print("Found a seat for id[" + list.IndexOf(item) + "] @ " + num + " : " + TableGraph.tables[num - 1].tableNodes.IndexOf(tableNode));
								Enterance.npcsWaiting--;
								break;
							}
						}
						if (targetSeat == null)
						{
							MonoBehaviour.print("Error: no free seat found?");
						}
					}
					return;
				}
				if (Network.isServer && desiredGroupSize == 4 && Random.value > 0.8f)
				{
					base.networkView.RPC("setGroupSize", RPCMode.All, base.networkView.viewID, 2);
				}
				myNPCGroup = new List<NPC>();
				myNPCGroupLeader = null;
				setWants(wants.toIdle);
				idleStartTime = Time.time;
				idleTimeRand = Random.Range(20, 40);
			}
			else if (desiredGroupSize == 1)
			{
				if (tableGraph == null)
				{
					tableGraph = GameObject.Find("!TableNodes").GetComponent<TableGraph>();
				}
				num = TableGraph.FindUnoccupiedTableForGroup(myNPCGroup.Count);
				if (num >= 0)
				{
					foreach (TableNodes tableNode2 in TableGraph.tables[num - 1].tableNodes)
					{
						if (!tableNode2.occupied)
						{
							targetSeat = tableNode2;
							tableNode2.networkView.RPC("SetOccupied", RPCMode.AllBuffered, true);
							setWants(wants.toGetToSeat);
							pathToFollow = graph.FindPath(FindClosestNode(base.transform.position), FindClosestVisibleNode(targetSeat.transform));
							Enterance.npcsWaiting--;
							break;
						}
					}
				}
				setWants(wants.toIdle);
				idleStartTime = Time.time;
				idleTimeRand = Random.Range(20, 40);
			}
			else
			{
				myNPCGroup = new List<NPC>();
				myNPCGroupLeader = null;
				setWants(wants.toIdle);
				idleStartTime = Time.time;
				idleTimeRand = Random.Range(10, 30);
			}
		}
		else if (currentlyWants == wants.toGetToSeat)
		{
			Vector3 zero6 = Vector3.zero;
			Vector3 zero7 = Vector3.zero;
			if (pathToFollow.Count > 0 && (pathToFollow[0].transform.position - base.transform.position).magnitude < 3f)
			{
				pathToFollow.RemoveAt(0);
			}
			RaycastHit hitInfo3;
			if (Physics.Linecast(base.transform.position, targetSeat.transform.position, out hitInfo3, layerMask))
			{
				if (pathToFollow.Count > 0)
				{
					zero6 += Seek(pathToFollow[0].transform.position) * followWeight;
					zero7 = zero6;
					zero7.y = 0f;
					zero6 += SeparateAvoidNodes() * avoidNodesWeight;
					zero6.y = 0f;
					if (zero6 != Vector3.zero)
					{
						base.transform.rotation = Quaternion.LookRotation(zero7 + base.transform.forward);
					}
					base.rigidbody.AddForce(zero6.normalized * maxSpeed * 8f * Time.deltaTime);
					return;
				}
				float num3 = 1f;
				if ((base.transform.position - targetSeat.transform.position).magnitude < 6f)
				{
					num3 = 0.2f;
				}
				if ((base.transform.position - targetSeat.transform.position).magnitude < 1f)
				{
					targetSeat.table.networkView.RPC("SetNPCAtTable", RPCMode.All, targetSeat.table.networkView.viewID, base.networkView.viewID);
					base.rigidbody.velocity = Vector3.zero;
					setWants(wants.toEat);
					if (Network.peerType == NetworkPeerType.Server)
					{
						targetSeat.table.GenerateFoodOrder();
					}
					foodWaitStartTime = Time.time;
				}
				zero6 += Seek(targetSeat.transform.position) * followWeight;
				zero7 = zero6;
				zero7.y = 0f;
				zero6 += SeparateAvoidNodes() * (avoidNodesWeight * num3);
				zero6.y = 0f;
				if (zero6 != Vector3.zero)
				{
					base.transform.rotation = Quaternion.LookRotation(zero7 + base.transform.forward);
				}
				base.rigidbody.AddForce(zero6.normalized * maxSpeed * 10f * Time.deltaTime);
				return;
			}
			if ((base.transform.position - targetSeat.transform.position).magnitude < 1f)
			{
				targetSeat.table.networkView.RPC("SetNPCAtTable", RPCMode.All, targetSeat.table.networkView.viewID, base.networkView.viewID);
				base.rigidbody.velocity = Vector3.zero;
				setWants(wants.toEat);
				if (Network.peerType == NetworkPeerType.Server)
				{
					targetSeat.table.GenerateFoodOrder();
				}
				foodWaitStartTime = Time.time;
			}
			zero6 += Seek(targetSeat.transform.position) * followWeight * 1.5f;
			zero7 = zero6;
			zero7.y = 0f;
			zero6 += SeparateAvoidNodes() * avoidNodesWeight;
			zero6.y = 0f;
			if (zero6 != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(zero7 + base.transform.forward);
			}
			base.rigidbody.AddForce(zero6.normalized * maxSpeed * 10f * Time.deltaTime);
		}
		else if (currentlyWants == wants.toLeave)
		{
			if (!(myNPCGroupLeader == null))
			{
				return;
			}
			int num4 = 0;
			foreach (NPC item2 in myNPCGroup)
			{
				if (item2.currentlyWants == wants.toLeave)
				{
					num4++;
				}
			}
			if (num4 != myNPCGroup.Count)
			{
				return;
			}
			MonoBehaviour.print("Entire group wants to leave");
			if (desiredGroupSize > 1)
			{
				foreach (NPC item3 in myNPCGroup)
				{
					item3.targetSeat.table.npcAtTable = null;
					item3.targetSeat.table.networkView.RPC("ClearOrder", RPCMode.All);
					item3.targetSeat.networkView.RPC("SetOccupied", RPCMode.AllBuffered, false);
					item3.FindExit(false);
					item3.CreatePath(item3.exitNode.transform.position);
					item3.setWants(wants.toExitScene);
				}
				return;
			}
			targetSeat.table.npcAtTable = null;
			targetSeat.table.networkView.RPC("ClearOrder", RPCMode.All);
			targetSeat.networkView.RPC("SetOccupied", RPCMode.AllBuffered, false);
			FindExit(false);
			CreatePath(exitNode.transform.position);
			setWants(wants.toExitScene);
		}
		else if (currentlyWants == wants.toGetMenu)
		{
			if (Time.time > menuWaitStartTime + menuWaitBaseDuration)
			{
				currentlyWants = wants.toLeave;
			}
		}
		else if (currentlyWants == wants.toEat && Time.time > foodWaitStartTime + foodWaitBaseDuration)
		{
			currentlyWants = wants.toLeave;
		}
	}
	
	public void FindExit(bool excludeNearestExit)
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
		exitListz = list;
	}
	
	private Vector3 Seek(Vector3 Target)
	{
		Vector3 zero = Vector3.zero;
		return (Target - base.transform.position).normalized * maxSpeed - base.rigidbody.velocity;
	}
	
	private Vector3 Arrive()
	{
		Vector3 zero = Vector3.zero;
		float num = 0f;
		float num2 = 0f;
		zero = playerToFollow.position + playerToFollow.forward * 5f - base.transform.position;
		num = zero.magnitude;
		num2 = num * 0.1f;
		return zero.normalized * (maxSpeed * num2) * speed;
	}
	
	private Vector3 SeparateFlock()
	{
		Vector3 zero = Vector3.zero;
		GameObject[] array = GameObject.FindGameObjectsWithTag("NPC");
		foreach (GameObject gameObject in array)
		{
			if ((base.transform.position - gameObject.transform.position).magnitude < unsafeDistance)
			{
				zero -= (gameObject.transform.position - base.transform.position).normalized * maxSpeed;
			}
		}
		return zero;
	}
	
	private Vector3 SeparateAvoidNodes()
	{
		Vector3 zero = Vector3.zero;
		foreach (Transform avoidNode in avoidNodes)
		{
			if ((base.transform.position - avoidNode.transform.position).magnitude < unsafeDistance)
			{
				zero -= (avoidNode.transform.position - base.transform.position).normalized * maxSpeed;
			}
		}
		return zero;
	}
	
	private void CreatePath(Vector3 target)
	{
		int num = FindClosestVisibleNode(base.transform);
		int goal = FindClosestNode(target, num);
		if (num == -1)
		{
			MonoBehaviour.print("Error: no path found");
			num = 0;
		}
		pathToFollow = graph.FindPath(num, goal);
	}
	
	private int FindClosestNode(Vector3 t)
	{
		float num = 9999f;
		int num2 = -1;
		foreach (NavigationScript node in graph.nodes)
		{
			if ((node.transform.position - t).magnitude < num)
			{
				num = (node.transform.position - t).magnitude;
				num2 = graph.nodes.IndexOf(node);
			}
		}
		if (num2 == -1)
		{
			MonoBehaviour.print("Error: no node found");
		}
		return num2;
	}
	
	private int FindClosestNode(Vector3 t, int s)
	{
		float num = 9999f;
		int num2 = -1;
		foreach (NavigationScript node in graph.nodes)
		{
			if ((node.transform.position - t).magnitude < num && node.index != s)
			{
				num = (node.transform.position - t).magnitude;
				num2 = graph.nodes.IndexOf(node);
			}
		}
		if (num2 == -1)
		{
			MonoBehaviour.print("Error: no node found");
		}
		return num2;
	}
	
	private int FindClosestVisibleNode(Transform t)
	{
		float num = 9999f;
		int num2 = -1;
		foreach (NavigationScript node in graph.nodes)
		{
			if (!Physics.Linecast(t.position, node.transform.position, layerMask) && (node.transform.position - t.position).magnitude < num)
			{
				if ((t.transform.position - new Vector3(24.6f, 4.5f, 31.7f)).magnitude < 2f)
				{
					MonoBehaviour.print("Dist: " + (node.transform.position - t.position).magnitude);
				}
				if ((node.transform.position - base.transform.position).magnitude > 0.5f)
				{
					num = (node.transform.position - t.position).magnitude;
					num2 = graph.nodes.IndexOf(node);
				}
			}
		}
		if (num2 == -1)
		{
			MonoBehaviour.print("Error: no visible node found");
		}
		return num2;
	}
	
	[RPC]
	public void setWants(int newWantID)
	{
		previouslyWanted = currentlyWants;
		currentlyWants = (wants)newWantID;
	}
	
	[RPC]
	public void setWants(wants newWant)
	{
		previouslyWanted = currentlyWants;
		currentlyWants = newWant;
	}
	
	[RPC]
	public void setGroupSize(NetworkViewID npcID, int newDesiredGroupSize)
	{
		NPC component = NetworkView.Find(npcID).gameObject.GetComponent<NPC>();
		component.desiredGroupSize = newDesiredGroupSize;
	}
	
	[RPC]
	public void FollowAPlayer(NetworkViewID playerID)
	{
		GameObject gameObject = NetworkView.Find(playerID).gameObject;
		if (gameObject != null)
		{
			playerToFollow = gameObject.transform;
			isFollowingPlayer = true;
		}
		else
		{
			playerToFollow = null;
			isFollowingPlayer = false;
		}
	}
	
	[RPC]
	public void FollowNoPlayer()
	{
		playerToFollow = null;
		isFollowingPlayer = false;
	}
	
	[RPC]
	private void SetNPCTexture(NetworkViewID npcID, string textureName)
	{
		Transform transform = NetworkView.Find(npcID).transform;
		transform.renderer.material = Resources.Load("Skins/Materials/" + textureName) as Material;
	}
	
}