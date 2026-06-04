using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAnimate : MonoBehaviour
{
	public static List<FireAnimate> AllFires = new List<FireAnimate>();

	public Flamable fireBase;

	public bool isLargeFire;

	private Material fireMat;

	private Light fireLight;

	private float lightSpeed = 1f;

	private float lightIntensity = 5f;

	private float lightOffset = 1f;

	private float scaleSpeed = 1f;

	private float scaleIntensity = 0.2f;

	private float scaleOffset = 1f;

	private float lastMaterialSwapTime;

	private float materialSwapDuration;

	private Light fireGlow;

	private Collider[] colliderProximity;

	private ParticleEmitter smoke;

	private void OnParticleCollision(GameObject other)
	{
		if (Network.isServer && other.name.Equals("WaterEmitter"))
		{
			PutOut();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)other.GetComponent<Flamable>() && !other.GetComponent<Flamable>().isOnFire)
		{
			other.GetComponent<Flamable>().currentBurnHealth -= 4f;
		}
	}

	private void Start()
	{
		smoke = base.transform.FindChild("Smoke").GetComponent<ParticleEmitter>();
		fireMat = base.renderer.material;
		fireGlow = GetComponent<Light>();
		AllFires.Add(this);
		StartCoroutine(RenderFires());
		Vector3 localScale = base.transform.localScale;
		localScale.y = 0f - localScale.y;
		base.transform.localScale = localScale;
	}

	private void Update()
	{
		smoke.emit = isLargeFire;
		if (lastMaterialSwapTime + materialSwapDuration < Time.time)
		{
			base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
			lastMaterialSwapTime = Time.time;
			materialSwapDuration = Random.Range(0.1f, 0.3f);
		}
	}

	private IEnumerator RenderFires()
	{
		while ((bool)this)
		{
			FlameOn();
			colliderProximity = Physics.OverlapSphere(base.transform.position, 1f);
			Collider[] array = colliderProximity;
			foreach (Collider c in array)
			{
				if ((bool)c.GetComponent<FireAnimate>() && c.GetComponent<FireAnimate>().isLargeFire && c != base.collider)
				{
					FlameOff();
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void FlameOn()
	{
		if (!base.renderer.enabled)
		{
			base.renderer.enabled = true;
		}
	}

	private void FlameOff()
	{
		if (base.renderer.enabled && fireBase != null && fireBase.pickup != null && !fireBase.pickup.beingHeld)
		{
			base.renderer.enabled = false;
		}
	}

	public void PutOut(bool sentFromFlamable = false)
	{
		if (Network.isServer)
		{
			FireWatch.RemoveFireReference(this);
			if (isLargeFire)
			{
				RemoveFromAllFires();
			}
			else if (!sentFromFlamable)
			{
				fireBase.FireBurnOut();
			}
			base.networkView.RPC("PutOutFire", RPCMode.Others, base.networkView.viewID);
			Object.Destroy(base.gameObject);
		}
	}

	[RPC]
	private void PutOutFire(NetworkViewID objectID)
	{
		//Discarded unreachable code: IL_001d
		FireAnimate component;
		try
		{
			component = NetworkView.Find(objectID).GetComponent<FireAnimate>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		if (component.isLargeFire)
		{
			component.smoke.transform.parent = base.transform.root;
			component.smoke.emit = false;
			component.smoke.GetComponent<ParticleAnimator>().autodestruct = true;
		}
		Object.Destroy(component.gameObject);
	}

	public void RemoveFromAllFires()
	{
		AllFires.Remove(this);
	}
}
