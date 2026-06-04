using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PickupObject : MonoBehaviour
{
	public bool createdInScene;

	public NetworkObject netObject;

	public GameObject armHoldingObject;

	public FirstPersonControl playerHolding;

	public FirstPersonControl lastPlayerHolding;

	public bool beingHeld;

	public bool beingUsed;

	public Vector3 heldPositionOffset = new Vector3(0f, 1f, 1f);

	public float heldRotateXOffset = -30f;

	private float currentHeldRotateXOffset;

	private void Awake()
	{
		netObject = GetComponent<NetworkObject>();
		currentHeldRotateXOffset = heldRotateXOffset;
	}

	public void ResetHeldRotation()
	{
		currentHeldRotateXOffset = heldRotateXOffset;
	}

	private void Update()
	{
		if (beingHeld && (bool)playerHolding && !playerHolding.networkView.isMine && armHoldingObject != null)
		{
			Vector3 to = armHoldingObject.transform.FindChild("hand").transform.position + armHoldingObject.transform.FindChild("hand").transform.forward * 2f * heldPositionOffset.z + armHoldingObject.transform.FindChild("hand").transform.right * heldPositionOffset.x + playerHolding.transform.up * heldPositionOffset.y;
			Quaternion to2 = armHoldingObject.transform.rotation * Quaternion.Euler(0f, 0f, 0f);
			base.transform.position = Vector3.Lerp(base.transform.position, to, 30f * Time.deltaTime);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, to2, 30f * Time.deltaTime);
		}
	}

	[RPC]
	private void DestroyObjectBuffered(NetworkViewID objectID)
	{
		Object.Destroy(NetworkView.Find(objectID).gameObject);
	}

	public void DestroyObject()
	{
		if (Network.isServer)
		{
			if ((bool)GetComponent<Flamable>())
			{
				Flamable component = GetComponent<Flamable>();
				component.FireBurnOut();
			}
			if (createdInScene)
			{
				base.networkView.RPC("DestroyObjectBuffered", RPCMode.AllBuffered, base.gameObject.networkView.viewID);
			}
			else
			{
				Network.RemoveRPCs(base.gameObject.networkView.viewID);
				Network.Destroy(base.gameObject);
			}
		}
	}

	[RPC]
	public void SetBeingHeld(NetworkViewID objectID, bool held, NetworkViewID arm, NetworkViewID playerID)
	{
		PickupObject component = NetworkView.Find(objectID).transform.GetComponent<PickupObject>();
		component.beingHeld = held;
		if (held)
		{
			armHoldingObject = NetworkView.Find(arm).gameObject;
			playerHolding = NetworkView.Find(playerID).transform.GetComponent<FirstPersonControl>();
			lastPlayerHolding = playerHolding;
		}
		else
		{
			armHoldingObject = null;
			playerHolding = null;
		}
	}

	public bool IsBeingUsed()
	{
		bool result = false;
		if ((bool)GetComponent<ObjectUsable>())
		{
			result = GetComponent<ObjectUsable>().beingUsed;
		}
		return result;
	}
}
