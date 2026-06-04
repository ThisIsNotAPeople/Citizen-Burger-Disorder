using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	private float speed = 10f;

	private float maxVelocity = 15f;

	private Color c;

	private Transform playerMovement;

	private float noInputDrag;

	private float inputDrag;

	private void Start()
	{
		if (!base.networkView.isMine)
		{
			base.enabled = false;
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if ((double)Random.value > 0.5)
		{
			num = 0.6f + Random.value * 0.4f;
		}
		if ((double)Random.value > 0.5)
		{
			num2 = 0.6f + Random.value * 0.4f;
		}
		if ((double)Random.value > 0.5)
		{
			num3 = 0.6f + Random.value * 0.4f;
		}
		if (num == 0f && num2 == 0f && num3 == 0f)
		{
			if ((double)Random.value > 0.3)
			{
				num = 0.4f + Random.value * 0.6f;
			}
			if ((double)Random.value > 0.3)
			{
				num2 = 0.4f + Random.value * 0.6f;
			}
			if ((double)Random.value > 0.3)
			{
				num3 = 0.4f + Random.value * 0.6f;
			}
		}
		if (num == 0f && num2 == 0f && num3 == 0f)
		{
			if ((double)Random.value > 0.1)
			{
				num = 0.3f + Random.value * 0.7f;
			}
			if ((double)Random.value > 0.1)
			{
				num2 = 0.3f + Random.value * 0.7f;
			}
			if ((double)Random.value > 0.1)
			{
				num3 = 0.3f + Random.value * 0.7f;
			}
		}
		c = new Color(num, num2, num3);
		c += new Color(0.6f, 0.6f, 0.6f);
		base.networkView.RPC("setColor", RPCMode.OthersBuffered, num, num2, num3, base.networkView.viewID);
		MouseOrbit component = Camera.main.GetComponent<MouseOrbit>();
		playerMovement = GameObject.Find("bearings").transform;
		inputDrag = base.rigidbody.drag;
		noInputDrag = inputDrag * 200f;
		if (!component.enabled)
		{
			component.enabled = true;
			component.target = base.transform;
		}
	}

	[RPC]
	private void setColor(float r, float g, float b, NetworkViewID id)
	{
		c = new Color(r, g, b);
		Transform transform = NetworkView.Find(id).transform.FindChild("Cube");
		transform.renderer.material.SetColor("_Color", c);
	}

	private void Update()
	{
		playerMovement.position = base.transform.position;
		playerMovement.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
		if (Input.GetKeyDown(KeyCode.B))
		{
			buildStructureLogic();
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			buildLadderLogic();
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			buildPhysLogic();
		}
		if (!base.networkView.isMine)
		{
			return;
		}
		if (base.transform.FindChild("Cube").renderer.material.color != c)
		{
			base.transform.FindChild("Cube").renderer.material.SetColor("_Color", c);
		}
		float num = Input.GetAxis("Horizontal");
		float num2 = Input.GetAxis("Vertical");
		if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
		{
			if (base.rigidbody.angularDrag != inputDrag)
			{
				base.rigidbody.angularDrag = inputDrag;
			}
		}
		else if (base.rigidbody.angularDrag != noInputDrag)
		{
			base.rigidbody.angularDrag = noInputDrag;
		}
		RaycastHit hitInfo;
		if (!Physics.Raycast(base.transform.position, -Vector3.up, out hitInfo, base.transform.localScale.y * 1.5f))
		{
			num2 *= 0.35f;
			num *= 0.35f;
		}
		if (base.rigidbody.velocity.magnitude <= maxVelocity)
		{
			base.rigidbody.AddForce((num * Camera.main.transform.right + (0f - num2) * -playerMovement.forward) * speed);
		}
	}

	private void buildStructureLogic()
	{
		RaycastHit hitInfo;
		RaycastHit hitInfo2;
		if (!Physics.Raycast(base.transform.position, playerMovement.forward, out hitInfo, 4f) && Physics.Raycast(base.transform.position, -Vector3.up, out hitInfo, 5f) && Physics.Raycast(base.transform.position + playerMovement.forward * 4f, -Vector3.up, out hitInfo2, 4f))
		{
			Vector3 position = hitInfo2.point + playerMovement.forward * 10f;
			GameObject gameObject = (GameObject)Network.Instantiate(Resources.Load("Prefabs/Structure"), position, playerMovement.rotation, 1);
			MoveUntil component = gameObject.GetComponent<MoveUntil>();
			component.movePos = new Vector3(0f, 17f, 0f);
			component.moveTime = 2f;
			SettingColor component2 = gameObject.GetComponent<SettingColor>();
			float num = base.renderer.material.color.r * 0.3f;
			float num2 = base.renderer.material.color.g * 0.3f;
			float num3 = base.renderer.material.color.b * 0.3f;
			component2.networkView.RPC("setColorNetwork", RPCMode.AllBuffered, num, num2, num3, gameObject.networkView.viewID);
		}
	}

	private void buildLadderLogic()
	{
		RaycastHit hitInfo;
		if (!Physics.Raycast(base.transform.position, playerMovement.forward, out hitInfo, 0.1f) && Physics.Raycast(base.transform.position, -Vector3.up, out hitInfo, 7f))
		{
			GameObject gameObject = (GameObject)Network.Instantiate(position: (!Physics.Raycast(base.transform.position, playerMovement.forward, out hitInfo, 15f)) ? (base.transform.position + -playerMovement.up * 10f + playerMovement.forward * 16f) : (base.transform.position + -playerMovement.up * 10f + -playerMovement.forward * 4f), prefab: Resources.Load("Prefabs/Ladder"), rotation: playerMovement.rotation * Quaternion.Euler(0f, 0f, 0f), group: 1);
			MoveUntil component = gameObject.GetComponent<MoveUntil>();
			gameObject.rigidbody.isKinematic = true;
			component.movePos = new Vector3(0f, 35f, 0f);
			component.pushForward = true;
			component.moveTime = 2f;
			SettingColor component2 = gameObject.GetComponent<SettingColor>();
			float num = base.renderer.material.color.r * 0.4f;
			float num2 = base.renderer.material.color.g * 0.4f;
			float num3 = base.renderer.material.color.b * 0.4f;
			component2.networkView.RPC("setColorNetwork", RPCMode.AllBuffered, num, num2, num3, gameObject.networkView.viewID);
		}
	}

	private void buildPhysLogic()
	{
		RaycastHit hitInfo;
		if (!Physics.Raycast(base.transform.position, playerMovement.forward, out hitInfo, 16f) && Physics.Raycast(base.transform.position, -Vector3.up, out hitInfo, 30f))
		{
			Vector3 position = hitInfo.point + playerMovement.up * 2f + playerMovement.forward * 16f;
			GameObject gameObject = (GameObject)Network.Instantiate(Resources.Load("Prefabs/Ball"), position, playerMovement.rotation * Quaternion.Euler(0f, 90f, 0f), 1);
			SettingColor component = gameObject.GetComponent<SettingColor>();
			float num = base.renderer.material.color.r * 2f;
			float num2 = base.renderer.material.color.g * 2f;
			float num3 = base.renderer.material.color.b * 2f;
			component.networkView.RPC("setColorNetwork", RPCMode.AllBuffered, num, num2, num3, gameObject.networkView.viewID);
		}
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 value = base.transform.position;
			stream.Serialize(ref value);
			Vector3 value2 = base.rigidbody.velocity;
			stream.Serialize(ref value2);
		}
		else
		{
			Vector3 value3 = Vector3.zero;
			stream.Serialize(ref value3);
			base.transform.position = value3;
			Vector3 value4 = Vector3.zero;
			stream.Serialize(ref value4);
			base.rigidbody.velocity = value4;
		}
	}
}
