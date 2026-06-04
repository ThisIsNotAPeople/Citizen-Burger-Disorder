using UnityEngine;

public class FireInputController : MonoBehaviour
{
	public bool WaterOn;

	private PickupObject pickup;

	private ParticleEmitter waterEmitter;

	private ParticleEmitter foamEmitter;

	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncAllFireInput", player, base.networkView.viewID, WaterOn);
	}

	[RPC]
	private void SyncAllFireInput(NetworkViewID objectID, bool nWaterOn)
	{
		//Discarded unreachable code: IL_001d
		FireInputController component;
		try
		{
			component = NetworkView.Find(objectID).GetComponent<FireInputController>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		component.WaterOn = nWaterOn;
	}

	private void Start()
	{
		waterEmitter = base.transform.FindChild("WaterEmitter").GetComponent<ParticleEmitter>();
		foamEmitter = base.transform.FindChild("Foam").GetComponent<ParticleEmitter>();
		waterEmitter.emit = WaterOn;
		foamEmitter.emit = WaterOn;
		pickup = GetComponent<PickupObject>();
	}

	private void Update()
	{
		if (pickup.beingHeld && pickup.playerHolding == FirstPersonControl.localPlayer)
		{
			if (Input.GetKey(OppositeHandKey()))
			{
				WaterOn = true;
				base.networkView.RPC("EmitToggle", RPCMode.Others, base.networkView.viewID, WaterOn);
			}
			else
			{
				WaterOn = false;
				base.networkView.RPC("EmitToggle", RPCMode.Others, base.networkView.viewID, WaterOn);
			}
		}
		if (base.networkView.isMine && !pickup.playerHolding)
		{
			WaterOn = false;
		}
	}

	[RPC]
	private void EmitToggle(NetworkViewID objectID, bool nWaterOn)
	{
		//Discarded unreachable code: IL_001d
		FireInputController component;
		try
		{
			component = NetworkView.Find(objectID).GetComponent<FireInputController>();
		}
		catch (UnityException message)
		{
			Debug.Log(message);
			return;
		}
		component.WaterOn = nWaterOn;
	}

	private void LateUpdate()
	{
		waterEmitter.emit = WaterOn;
		foamEmitter.emit = WaterOn;
	}

	private KeyCode OppositeHandKey()
	{
		if ((bool)pickup.playerHolding.leftArmObject && pickup.playerHolding.leftArmObject == base.transform)
		{
			return KeyCode.Mouse1;
		}
		if ((bool)pickup.playerHolding.rightArmObject && pickup.playerHolding.rightArmObject == base.transform)
		{
			return KeyCode.Mouse0;
		}
		return KeyCode.P;
	}
}
