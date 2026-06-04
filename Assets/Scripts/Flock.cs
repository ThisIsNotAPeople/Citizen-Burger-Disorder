using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
	public int flockSize = 10;

	private float spawnRange = 20f;

	public GameObject boidPrefab;

	private List<GameObject> flock;

	private int villagersSpawned;

	private int recordedTotalHouses;

	private void Start()
	{
		InitFlock();
	}

	private void InitFlock()
	{
		flock = new List<GameObject>();
		for (int i = 0; i < flockSize; i++)
		{
			GameObject gameObject = Object.Instantiate(boidPrefab, base.transform.position, base.transform.rotation) as GameObject;
			Vector3 vector = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y) + base.collider.bounds.size;
			Vector3 localPosition = new Vector3(vector.x + Random.Range(0f - spawnRange, spawnRange), 0f, vector.y + Random.Range(0f - spawnRange, spawnRange));
			gameObject.transform.localPosition = localPosition;
			flock.Add(gameObject);
			villagersSpawned++;
		}
	}

	public void NewBoid()
	{
		GameObject gameObject = Object.Instantiate(boidPrefab, base.transform.position, base.transform.rotation) as GameObject;
		Vector3 vector = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y) + base.collider.bounds.size;
		Vector3 localPosition = new Vector3(vector.x + Random.Range(0f - spawnRange, spawnRange), 0f, vector.y + Random.Range(0f - spawnRange, spawnRange));
		gameObject.transform.localPosition = localPosition;
		flock.Add(gameObject);
		villagersSpawned++;
		flockSize++;
	}

	private void Update()
	{
	}
}
