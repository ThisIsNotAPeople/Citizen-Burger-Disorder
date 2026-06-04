using System.Collections.Generic;
using UnityEngine;

public class FloorTrigger : MonoBehaviour
{
	private int maxRatSpawns = 5;

	public static int currentRats;

	private GameObject ratPrefab;

	private Transform ratSpawn;

	private float ratSpawnStartTimer;

	private float ratTimerUntilSpawn = 30f;

	private float ratSpawnCooldown = 5f;

	private float ratSpawnCooldownStart;

	public static List<GameObject> foodDropPosition = new List<GameObject>();

	private void Start()
	{
		ratSpawn = GameObject.Find("!RatGraph").transform.FindChild("RATSPAWN");
		//ratPrefab = Resources.Load("prefabs/npc/rat") as GameObject;
		//Debug.Log ("RatSpawn: " + ratSpawn.name + "RatPrefab: " + ratPrefab.name);
	}

	private void Update()
	{
		//ratSpawn = GameObject.Find("!RatGraph").transform.FindChild("RATSPAWN");
		//ratPrefab = Resources.Load("prefabs/npc/rat") as GameObject;
		//Debug.Log ("RatSpawn: " + ratSpawn.name + "RatPrefab: " + ratPrefab.name);
		if (!Network.isServer || currentRats >= maxRatSpawns || !(Time.time > ratSpawnCooldown + ratSpawnCooldownStart))
		{
			return;
		}
		if (Time.time > ratSpawnStartTimer + ratTimerUntilSpawn && foodDropPosition.Count > 0)
		{
			for (int i = 0; i < Mathf.Min(foodDropPosition.Count, 3); i++)
			{
				if (currentRats > maxRatSpawns)
				{
					break;
				}
				Vector3 position;
				if (foodDropPosition.Count == 0)
				{
					position = ratSpawn.transform.position;
				}
				else
				{
					int index = Random.Range(0, foodDropPosition.Count - 1);
					position = foodDropPosition[index].transform.position;
				}
				//Rat component = (Network.Instantiate(ratPrefab, ratSpawn.transform.position, Quaternion.identity, 3) as GameObject).GetComponent<Rat>();
				GameObject ratPrefab = Resources.Load("Prefabs/NPC/rat", typeof(GameObject)) as GameObject;
				GameObject component = (Network.Instantiate(ratPrefab, ratSpawn.position, Quaternion.identity, 3) as GameObject);
				component.GetComponent<Rat>();
					if (component != null)
					{
							base.networkView.RPC("SetRatTarget", RPCMode.AllBuffered, component.networkView.viewID, position);
							currentRats++;
					}
					else
					{
						Debug.LogError("Network.Instantiate return null!");
					}
				}
			if (Random.value > 0.1f)
			{
				ratSpawnCooldownStart = Time.time;
			}
		}
		else if (foodDropPosition.Count == 0)
		{
			ratSpawnStartTimer = Time.time;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Network.isServer && (bool)other.GetComponent<Food>() && !other.name.Contains("rat") && !foodDropPosition.Contains(other.gameObject))
		{
			other.GetComponent<Food>().foodBeenOnFloor = true;
			if (Network.isServer)
			{
				foodDropPosition.Add(other.gameObject);
				ratSpawnStartTimer = Time.time;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (Network.isServer && (bool)other.GetComponent<Food>() && !other.name.Contains("rat") && foodDropPosition.Contains(other.gameObject))
		{
			foodDropPosition.Remove(other.gameObject);
		}
	}

	[RPC]
	private void SetRatTarget(NetworkViewID ratID, Vector3 target)
	{
		Transform transform = NetworkView.Find(ratID).transform;
		Rat component = transform.GetComponent<Rat>();
		component.SetTargetFood(target);
	}
}
