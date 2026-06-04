using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
	public static int currentNPCs;

	private int maxNPCs = 20;

	private int maxRestaurantNPCs = 10;

	private float spawnRadius = 6f;

	private float spawnMinDelay = 100f;

	private float currentSpawnMinDelay;

	private static float spawnAdditionalTimeRand;

	private float spawnMaxRand = 40f;

	private float[] timeOfDaySpawnMultipliers = new float[8] { 0.5f, 1f, 1.2f, 0.5f, 2f, 0.9f, 1.5f, 1f };

	private static float lastSpawnTime;

	private float citizenSpawnDelay = 20f;

	private float lastCitizenSpawn;

	private void Start()
	{
		spawnAdditionalTimeRand = 10f;
		currentSpawnMinDelay = spawnMinDelay;
		lastSpawnTime = Time.time - currentSpawnMinDelay;
	}

	private void OnGUI()
	{
		GUI.skin.label.fontSize = 12;
		GUI.Label(new Rect(0f, 20f, 500f, 20f), "Spawn: " + Mathf.Round(lastSpawnTime + currentSpawnMinDelay + spawnAdditionalTimeRand - Time.time));
	}

	private void Update()
	{
		if (!Network.isServer)
		{
			return;
		}
		if (TimeOfDay.currentTimeOfDay < TimeOfDay.endTimeOfDay)
		{
			int num = Mathf.RoundToInt((TimeOfDay.currentTimeOfDay - TimeOfDay.startTimeOfDay) / (TimeOfDay.endTimeOfDay - TimeOfDay.startTimeOfDay) * (float)(timeOfDaySpawnMultipliers.Length - 1));
			currentSpawnMinDelay = spawnMinDelay * (1f / (timeOfDaySpawnMultipliers[num] + 0.001f)) / (float)Mathf.Max(Network.connections.Length + 1, 3);
		}
		if (Time.time > lastCitizenSpawn + citizenSpawnDelay)
		{
			lastCitizenSpawn = Time.time;
			if (Random.value > 0.999f)
			{
				SpawnNewNPC(0, 1, 1);
			}
			else
			{
				SpawnNewNPC();
			}
		}
		if (!(Time.time > lastSpawnTime + (currentSpawnMinDelay + spawnAdditionalTimeRand)))
		{
			return;
		}
		lastSpawnTime = Time.time;
		spawnAdditionalTimeRand = Random.Range(0f, spawnMaxRand + 1f);
		if (currentNPCs < maxNPCs)
		{
			int num2 = 0;
			int num3 = 1;
			num2 = 1;
			float value = Random.value;
			if (value > 0.9f)
			{
				num3 = 3;
			}
			else if (value > 0.7f)
			{
				num3 = 4;
			}
			else if (value > 0.2f)
			{
				num3 = 2;
			}
			if (TableGraph.FindUnoccupiedTableForGroup(num3) == -1)
			{
				num2 = 0;
			}
			for (int i = 0; i < num3; i++)
			{
				SpawnNewNPC(num2, num3);
			}
		}
	}

	private GameObject SpawnNewNPC(int npcWants = 0, int groupSize = 1, int easterEgg = 0)
	{
		Vector3 position = base.transform.position + Random.insideUnitSphere * spawnRadius;
		position.y = base.transform.position.y;
		GameObject gameObject = Network.Instantiate(Resources.Load("Prefabs/NPC/NPC"), position, base.transform.rotation, 2) as GameObject;
		gameObject.networkView.RPC("SetNPCTexture", RPCMode.AllBuffered, gameObject.networkView.viewID, Random.Range(1, 7) + string.Empty);
		if (easterEgg == 1)
		{
			string[] array = new string[2] { "jorji", "cookServe" };
			gameObject.networkView.RPC("SetNPCTexture", RPCMode.AllBuffered, gameObject.networkView.viewID, array[Random.Range(0, array.Length)]);
		}
		gameObject.networkView.RPC("setWants", RPCMode.All, npcWants);
		gameObject.networkView.RPC("setGroupSize", RPCMode.All, gameObject.networkView.viewID, groupSize);
		currentNPCs++;
		return gameObject;
	}
}
