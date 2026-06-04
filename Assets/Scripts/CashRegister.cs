using UnityEngine;

public class CashRegister : MonoBehaviour
{
	public float money = 100f;

	private TextMesh display;

	private void Start()
	{
		display = base.transform.parent.FindChild("Display").GetComponent<TextMesh>();
	}

	private void Update()
	{
	}

	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncMoney", RPCMode.Others, money);
	}

	public void RefreshDisplay(float alterationValue = 0f)
	{
		money += alterationValue;
		display.text = "$" + money;
		if (Network.isServer)
		{
			base.networkView.RPC("SyncMoney", RPCMode.Others, money);
		}
	}

	[RPC]
	private void SyncMoney(float currentMoney)
	{
		money = currentMoney;
		RefreshDisplay(0f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.name.Contains("Tip") && !other.GetComponent<PickupObject>().beingHeld)
		{
			RefreshDisplay(2f);
			other.gameObject.GetComponent<PickupObject>().DestroyObject();
		}
	}
}
