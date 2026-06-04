using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkView))]
public class Flamable : MonoBehaviour
{
	public static List<Flamable> AllFires = new List<Flamable>();

	public List<Flamable> NearFlamables = new List<Flamable>();

	private Collider[] proximityBigFires;

	private Collider[] proximityFlamables;

	private Vector3 lastCheckLocation;

	public bool isOnFire;

	public Vector3 fireOffsetLocaiton = Vector3.zero;

	public bool wasOnFire;

	public bool isFlamableAgain = true;

	public bool reflamable;

	public float burnHealth = 20f;

	public float burnoutAtHealth = -300f;

	public float currentBurnHealth;

	public float tempUntilIgniteFire = 100f;

	public float startTemp = 10f;

	public float currentTemp;

	private float fireSpreadRadius = 3f;

	private Food food;

	public PickupObject pickup;

	private GameObject FireGameObject;

	public FireAnimate fireAnimate;

	private static GameObject firePrefab;

	public static FireWatch fireWatch;

	public bool nearBigFire;

	public float currentFireCheckRate;

	public float minFireCheckRate = 0.9f;

	public float maxFireCheckRate = 2f;

	private void Start()
	{
		GameObject fireWatchObject = GameObject.Find("!FireWatch");
		if ((bool)fireWatchObject)
		{
			fireWatch = fireWatchObject.GetComponent<FireWatch>();
		}
		if (!(bool)fireWatch)
		{
			fireWatchObject = new GameObject("!FireWatch");
			fireWatchObject.AddComponent<NetworkView>();
			fireWatch = fireWatchObject.AddComponent<FireWatch>();
			Debug.LogWarning("!FireWatch was missing in this scene. Created runtime FireWatch automatically.");
		}
		if ((bool)GetComponent<Food>())
		{
			food = GetComponent<Food>();
		}
		if ((bool)GetComponent<PickupObject>())
		{
			pickup = GetComponent<PickupObject>();
		}
		firePrefab = Resources.Load("Prefabs/Fire/Fire") as GameObject;
		if ((bool)food)
		{
			startTemp = food.foodTemp;
		}
		currentTemp = startTemp;
		currentBurnHealth = burnHealth;
		currentFireCheckRate = maxFireCheckRate;
	}

	private void Update()
	{
		if (!Network.isServer)
		{
			return;
		}
		if ((bool)food)
		{
			currentTemp = food.foodTemp;
		}
		if (!isOnFire)
		{
			if (wasOnFire && reflamable && !isFlamableAgain)
			{
				currentBurnHealth = Mathf.Lerp(currentBurnHealth, burnHealth, Time.deltaTime * 0.5f);
				if (currentBurnHealth == burnHealth)
				{
					isFlamableAgain = true;
				}
			}
			if (!wasOnFire || (reflamable && isFlamableAgain))
			{
				if (currentTemp > tempUntilIgniteFire)
				{
					FireIgnite();
				}
				if (currentBurnHealth < 0f)
				{
					FireIgnite();
				}
			}
		}
		if ((isOnFire && currentBurnHealth <= burnoutAtHealth) || currentTemp < startTemp)
		{
			FireBurnOut();
		}
		if (isOnFire)
		{
			currentBurnHealth -= Time.deltaTime;
			if (food != null)
			{
				food.cook();
			}
		}
	}

	private IEnumerator FireDetect()
	{
		while (isOnFire)
		{
			if (lastCheckLocation == Vector3.zero || (lastCheckLocation - base.transform.position).magnitude > fireSpreadRadius)
			{
				NearFlamables.Clear();
				proximityFlamables = Physics.OverlapSphere(base.transform.position, fireSpreadRadius);
				Collider[] array = proximityFlamables;
				foreach (Collider c in array)
				{
					if ((bool)c.GetComponent<Flamable>())
					{
						NearFlamables.Add(c.GetComponent<Flamable>());
					}
				}
			}
			yield return new WaitForSeconds(0.4f);
		}
	}

	private IEnumerator FireSpread()
	{
		while (isOnFire)
		{
			Vector3 avgPos = Vector3.zero;
			int localFireCount = 0;
			for (int i = 0; i < NearFlamables.Count; i++)
			{
				if (NearFlamables[i].isOnFire && NearFlamables[i] != this)
				{
					localFireCount++;
				}
				else if (!NearFlamables[i].isOnFire && NearFlamables[i] != this)
				{
					NearFlamables[i].currentBurnHealth -= 1f;
				}
				avgPos += NearFlamables[i].transform.position;
			}
			avgPos /= (float)NearFlamables.Count;
			Debug.DrawLine(base.transform.position, avgPos, Color.green, 2f);
			RaycastHit hit;
			if (!nearBigFire && localFireCount > 3 && !CheckNearBigFire() && Physics.Raycast(avgPos + Vector3.up, Vector3.down, out hit, 2f, ~((1 << LayerMask.NameToLayer("Food")) | (1 << LayerMask.NameToLayer("Fire")))))
			{
				MonoBehaviour.print("Hit: " + hit.transform.name);
				if (Network.isServer)
				{
					fireWatch.networkView.RPC("CreateBigFireAnimate", RPCMode.All, hit.point + new Vector3(0f, Random.Range(-0.5f, 0.5f) + 1f, 0f), base.transform.rotation, Network.AllocateViewID());
				}
			}
			yield return new WaitForSeconds(Random.Range(0.8f, 1.2f));
		}
	}

	private IEnumerator LoopCheckNearBigFire()
	{
		while (isOnFire)
		{
			CheckNearBigFire();
			yield return new WaitForSeconds(Random.Range(3f, 8f));
		}
	}

	private bool CheckNearBigFire()
	{
		if (!nearBigFire || (nearBigFire && (lastCheckLocation - base.transform.position).magnitude > fireSpreadRadius))
		{
			proximityBigFires = Physics.OverlapSphere(base.transform.position, fireSpreadRadius * 1f);
			bool flag = false;
			Collider[] array = proximityBigFires;
			foreach (Collider collider in array)
			{
				if ((bool)collider.GetComponent<FireAnimate>() && collider.GetComponent<FireAnimate>().isLargeFire)
				{
					flag = true;
				}
			}
			nearBigFire = flag;
		}
		return nearBigFire;
	}

	public void FireIgnite()
	{
		if (Network.isServer)
		{
			isOnFire = true;
			StartCoroutine(FireSpread());
			StartCoroutine(FireDetect());
			StartCoroutine(LoopCheckNearBigFire());
			fireWatch.networkView.RPC("CreateFireAnimate", RPCMode.All, base.transform.position + fireOffsetLocaiton, base.transform.rotation, false, base.networkView.viewID, Network.AllocateViewID());
			currentFireCheckRate = minFireCheckRate;
			AllFires.Add(this);
			fireWatch.networkView.RPC("SyncAllFlamable", RPCMode.Others, base.networkView.viewID, isOnFire, wasOnFire, isFlamableAgain, reflamable, currentBurnHealth, currentTemp, nearBigFire, currentFireCheckRate);
		}
	}

	public void FireBurnOut(bool resetBurnTemp = false)
	{
		if (!Network.isServer)
		{
			return;
		}
		isOnFire = false;
		wasOnFire = true;
		isFlamableAgain = false;
		StopCoroutine(FireSpread());
		StopCoroutine(FireDetect());
		StopCoroutine(LoopCheckNearBigFire());
		if ((bool)fireAnimate)
		{
			fireAnimate.PutOut(true);
		}
		Object.Destroy(FireGameObject);
		FireGameObject = null;
		currentFireCheckRate = maxFireCheckRate;
		if (resetBurnTemp)
		{
			currentBurnHealth = burnHealth;
			currentTemp = startTemp;
			if (food != null)
			{
				food.foodTemp = startTemp;
				food.CallSyncFood();
			}
		}
		AllFires.Remove(this);
		fireWatch.networkView.RPC("SyncAllFlamable", RPCMode.Others, base.networkView.viewID, isOnFire, wasOnFire, isFlamableAgain, reflamable, currentBurnHealth, currentTemp, nearBigFire, currentFireCheckRate);
	}
}
