using System.Collections.Generic;
using UnityEngine;

public class PickUpObjects : MonoBehaviour
{
	public new Camera camera;

	public GameObject holdingObject;

	private bool holding;

	private List<float> mouseXMovements = new List<float>();

	private List<float> mouseYMovements = new List<float>();

	private void Start()
	{
		if (base.networkView.isMine)
		{
			camera = Camera.main;
		}
	}

	private void Update()
	{
		if (!base.networkView.isMine)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		mouseXMovements.Add(Input.GetAxis("Mouse X") * 2000f);
		mouseYMovements.Add(Input.GetAxis("Mouse Y") * 2000f);
		if (mouseXMovements.Count >= 5)
		{
			mouseXMovements.RemoveAt(0);
			for (int i = 0; i < mouseXMovements.Count; i++)
			{
				num += mouseXMovements[i];
			}
		}
		num /= (float)mouseXMovements.Count;
		if (mouseYMovements.Count >= 5)
		{
			mouseYMovements.RemoveAt(0);
			for (int j = 0; j < mouseYMovements.Count; j++)
			{
				num2 += mouseYMovements[j];
			}
		}
		num2 /= (float)mouseYMovements.Count;
		if (Input.GetButtonUp("Fire1") && holding)
		{
			holding = false;
			holdingObject.rigidbody.isKinematic = false;
			holdingObject.rigidbody.useGravity = true;
			Vector3 force = holdingObject.transform.up * num2;
			Vector3 force2 = holdingObject.transform.right * num;
			Vector3 force3 = base.transform.forward * Input.GetAxis("Vertical") * 1600f;
			Vector3 force4 = base.transform.right * Input.GetAxis("Horizontal") * 1500f;
			holdingObject.transform.rigidbody.AddForce(force);
			holdingObject.transform.rigidbody.AddForce(force2);
			holdingObject.transform.rigidbody.AddForce(force3);
			holdingObject.transform.rigidbody.AddForce(force4);
			holdingObject = null;
		}
		else if (Input.GetButtonDown("Fire1") && !holding)
		{
			int num3 = 256;
			num3 = ~num3;
			RaycastHit hitInfo;
			if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo, 10f, num3) && hitInfo.transform.gameObject.tag.Equals("PhysicsCube") && hitInfo.rigidbody != null)
			{
				holdingObject = hitInfo.transform.gameObject;
				holdingObject.rigidbody.isKinematic = true;
				holdingObject.rigidbody.useGravity = false;
				holding = true;
			}
		}
		if (holding)
		{
			RaycastHit hitInfo2;
			if (!Physics.Raycast(holdingObject.transform.position, camera.transform.forward, out hitInfo2, 2.5f))
			{
				holdingObject.transform.position = Vector3.Lerp(holdingObject.transform.position, camera.transform.position + camera.transform.forward * 6f + camera.transform.up * 2f, Time.deltaTime * 14f);
			}
			else if (!Physics.Raycast(holdingObject.transform.position, camera.transform.forward, out hitInfo2, 0.6f))
			{
				holdingObject.transform.position = Vector3.Lerp(holdingObject.transform.position, camera.transform.position + camera.transform.forward * 4.5f + camera.transform.up * 2f, Time.deltaTime * 0.5f);
			}
			else if ((camera.transform.position + camera.transform.forward * 4.5f + camera.transform.up * 2f - camera.transform.position).magnitude > (holdingObject.transform.position - camera.transform.position).magnitude)
			{
				holdingObject.transform.position = Vector3.Lerp(holdingObject.transform.position, camera.transform.position, Time.deltaTime * 50f);
			}
			base.networkView.RPC("setHoldingLocation", RPCMode.OthersBuffered, holdingObject.transform.position, base.networkView.viewID);
			holdingObject.transform.rotation = camera.transform.rotation;
		}
	}

	public bool getHolding()
	{
		return holding;
	}

	[RPC]
	private void setHoldingLocation(Vector3 pos, NetworkViewID id)
	{
		if (!base.networkView.isMine)
		{
			Transform transform = NetworkView.Find(id).transform;
			transform.transform.position = pos;
		}
	}
}
