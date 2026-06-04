using System.Collections.Generic;
using UnityEngine;

public class FireWatch : MonoBehaviour
{
	public static List<FireAnimate> AllFireAnimates = new List<FireAnimate>();

	private GameObject firePrefab;

	private void Start()
	{
		firePrefab = Resources.Load("Prefabs/Fire/Fire") as GameObject;
	}

	private void OnPlayerConnected(NetworkPlayer player)
	{
		foreach (FireAnimate allFireAnimate in AllFireAnimates)
		{
			if ((bool)allFireAnimate.fireBase)
			{
				base.networkView.RPC("CreateFireAnimate", player, allFireAnimate.transform.position, allFireAnimate.transform.rotation, allFireAnimate.fireBase.nearBigFire, allFireAnimate.fireBase.networkView.viewID, allFireAnimate.networkView.viewID);
				base.networkView.RPC("SyncAllFlamable", player, allFireAnimate.fireBase.networkView.viewID, allFireAnimate.fireBase.isOnFire, allFireAnimate.fireBase.wasOnFire, allFireAnimate.fireBase.isFlamableAgain, allFireAnimate.fireBase.reflamable, allFireAnimate.fireBase.currentBurnHealth, allFireAnimate.fireBase.currentTemp, allFireAnimate.fireBase.nearBigFire, allFireAnimate.fireBase.currentFireCheckRate);
			}
			else
			{
				base.networkView.RPC("CreateBigFireAnimate", player, allFireAnimate.transform.position, allFireAnimate.transform.rotation, allFireAnimate.networkView.viewID);
			}
		}
	}

	[RPC]
	public void CreateFireAnimate(Vector3 position, Quaternion rotation, bool nearBigFire, NetworkViewID callerID, NetworkViewID newObjectID)
	{
		GameObject gameObject = Object.Instantiate(firePrefab, position, rotation) as GameObject;
		gameObject.networkView.viewID = newObjectID;
		SetupNewFire(gameObject.networkView.viewID, callerID, false, nearBigFire);
		if (Network.isServer)
		{
			AllFireAnimates.Add(gameObject.GetComponent<FireAnimate>());
		}
	}

	[RPC]
	public void CreateBigFireAnimate(Vector3 position, Quaternion rotation, NetworkViewID newObjectID)
	{
		GameObject gameObject = Object.Instantiate(firePrefab, position, rotation) as GameObject;
		gameObject.networkView.viewID = newObjectID;
		SetupNewBigFire(gameObject);
		if (Network.isServer)
		{
			AllFireAnimates.Add(gameObject.GetComponent<FireAnimate>());
		}
	}

	[RPC]
	private void SetupNewFire(NetworkViewID objectID, NetworkViewID creatorObjectID, bool isLargeFire, bool nearBigFire)
	{
		//Discarded unreachable code: IL_0029
		GameObject gameObject;
		Flamable component;
		try
		{
			gameObject = NetworkView.Find(objectID).gameObject;
			component = NetworkView.Find(creatorObjectID).GetComponent<Flamable>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		FollowGameObject component2 = gameObject.GetComponent<FollowGameObject>();
		component2.follow = component.gameObject;
		component2.distance = component.fireOffsetLocaiton;
		component.fireAnimate = component2.GetComponent<FireAnimate>();
		component.fireAnimate.fireBase = component;
		component.nearBigFire = nearBigFire;
	}

	private void SetupNewBigFire(GameObject fire)
	{
		fire.transform.localScale = new Vector3(fire.transform.localScale.x * 3.5f, fire.transform.localScale.y * 2.5f, fire.transform.localScale.z * 2.5f);
		fire.transform.position = fire.transform.position;
		fire.GetComponent<FireAnimate>().isLargeFire = true;
	}

	[RPC]
	private void SyncAllFlamable(NetworkViewID objectID, bool nIsOnFire, bool nWasOnFire, bool nIsFlamableAgain, bool nReflamable, float nCurrentBurnHealth, float nCurrentTemp, bool nNearBigFire, float nCurrentFireCheckRate)
	{
		//Discarded unreachable code: IL_001d
		Flamable component;
		try
		{
			component = NetworkView.Find(objectID).GetComponent<Flamable>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		component.isOnFire = nIsOnFire;
		component.wasOnFire = nWasOnFire;
		component.isFlamableAgain = nIsFlamableAgain;
		component.reflamable = nReflamable;
		component.currentBurnHealth = nCurrentBurnHealth;
		component.currentTemp = nCurrentTemp;
		component.nearBigFire = nNearBigFire;
		component.currentFireCheckRate = nCurrentFireCheckRate;
		if (component.isOnFire)
		{
			component.FireIgnite();
		}
	}

	public static void AddFireReference(FireAnimate newFireRef)
	{
		AllFireAnimates.Add(newFireRef);
	}

	public static void RemoveFireReference(FireAnimate removeFireRef)
	{
		AllFireAnimates.Remove(removeFireRef);
	}
}
