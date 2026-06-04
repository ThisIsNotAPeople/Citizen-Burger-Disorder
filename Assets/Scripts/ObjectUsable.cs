using UnityEngine;

public class ObjectUsable : PickupObject
{
	public FirstPersonControl control;

	private Vector3 defaultHeldPositionOffset;

	public Object pencilPrefab;

	private Transform pencil;

	private Vector2 lastDrawPosition = Vector2.zero;

	public bool usingRightHandObject;

	public bool usingLeftHandObject;

	public bool holdingRightHandObject;

	public bool holdingLeftHandObject;

	private bool stopUsingObject;

	private int layerMask;

	private void Start()
	{
		layerMask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default")));
	}

	public void StopUsingObject(bool stop = true)
	{
		stopUsingObject = stop;
	}

	private void Update()
	{
		usingRightHandObject = false;
		usingLeftHandObject = false;
		if (beingHeld)
		{
			if (!control)
			{
				control = GameObject.Find("Player(Mine)").GetComponent<FirstPersonControl>();
			}
			if (Input.GetButton("Fire2") && Input.GetButton("RightHand") && !usingLeftHandObject)
			{
				holdingRightHandObject = true;
				if (Input.GetButton("RightHand") && beingUsed)
				{
					usingRightHandObject = true;
				}
				if (Input.GetButtonDown("Fire1") && !Input.GetButton("LeftHand"))
				{
					usingRightHandObject = true;
					if (beingUsed)
					{
						RaycastHit hitInfo;
						if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 6f, layerMask))
						{
							MonoBehaviour.print("Won't exit");
						}
						if (hitInfo.transform != base.transform)
						{
							MonoBehaviour.print("Will exit");
							stopUsingObject = true;
						}
					}
				}
			}
			else
			{
				usingRightHandObject = false;
				holdingRightHandObject = false;
			}
			if (Input.GetButton("Fire1") && Input.GetButton("LeftHand") && !usingRightHandObject)
			{
				holdingLeftHandObject = true;
				if (Input.GetButton("LeftHand") && beingUsed)
				{
					usingLeftHandObject = true;
				}
				if (Input.GetButtonDown("Fire2") && !Input.GetButton("RightHand"))
				{
					usingLeftHandObject = true;
					RaycastHit hitInfo2;
					if (beingUsed && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo2))
					{
						if (hitInfo2.transform != base.transform)
						{
							MonoBehaviour.print("Will exit");
							stopUsingObject = true;
						}
						else
						{
							MonoBehaviour.print("Won't exit");
						}
					}
				}
			}
			else
			{
				usingLeftHandObject = false;
				holdingLeftHandObject = false;
			}
		}
		if ((usingRightHandObject || usingLeftHandObject) && !beingUsed)
		{
			beingUsed = true;
			Camera.main.GetComponent<MouseLook>().enabled = false;
			Screen.lockCursor = false;
			if (pencil == null)
			{
				GameObject gameObject = Object.Instantiate(pencilPrefab, base.transform.position, base.transform.rotation) as GameObject;
				pencil = gameObject.transform;
			}
			else
			{
				pencil.transform.rotation = base.transform.rotation;
				pencil.transform.position = base.transform.position;
			}
			control.networkView.RPC("setObjectCollisions", RPCMode.AllBuffered, false, base.networkView.viewID);
		}
		if ((stopUsingObject || !beingHeld) && beingUsed)
		{
			beingUsed = false;
			Camera.main.GetComponent<MouseLook>().enabled = true;
			Screen.lockCursor = true;
			Object.Destroy(pencil.gameObject);
			if (!base.collider.enabled)
			{
				control.networkView.RPC("setObjectCollisions", RPCMode.AllBuffered, true, base.networkView.viewID);
			}
		}
		if (beingUsed)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo3;
			if (Physics.Raycast(ray, out hitInfo3, 5f, layerMask))
			{
				pencil.position = hitInfo3.point;
			}
		}
		stopUsingObject = false;
	}
}
