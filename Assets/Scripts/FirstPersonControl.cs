using UnityEngine;

[RequireComponent(typeof(AudioSource))][RequireComponent(typeof(CharacterController))]public class FirstPersonControl : MonoBehaviour{public CharacterController controller;
	
	public new Camera camera;
	
	public Transform arm;
	
	public Transform leftArm;
	
	public Transform leftArmObject;
	
	private PickupObject leftArmPickup;
	
	public Transform rightArm;
	
	public Transform rightArmObject;
	
	private PickupObject rightArmPickup;
	
	public float gravity = 9.81f;
	
	public float moveSpeed = 16f;
	
	public float runMultiplier = 1.333f;
	
	public float crouchMultiplier = 0.25f;
	
	private Vector3 moveDir = Vector3.zero;
	
	private float gravityToApply;
	
	public int layerMask;
	
	private float maxCameraAngleFromZero = 86f;
	
	private float armForwardBasedOnRotation;
	
	private float armExtraReach = 1.2f;
	
	public string username = string.Empty;
	
	public static FirstPersonControl localPlayer;
	
	public Vector2 lastDrawPosition = Vector2.zero;
	
	private int spawnedItems;
	
	private int maxSpawnedItems = 8;
	
	private menu mainMenu;

	private Renderer renderer;
	
	private void Awake()
	{
		if (!base.networkView.isMine)
		{
			base.enabled = false;
			return;
		}
		
		if (controller == null)
		{
			controller = base.GetComponent<CharacterController>();
		}
		if (controller == null)
		{
			controller = base.gameObject.AddComponent<CharacterController>();
			controller.height = 2f;
			controller.radius = 0.35f;
			controller.center = new Vector3(0f, 1f, 0f);
			Debug.LogWarning("FirstPersonControl: CharacterController was missing and was created automatically.");
		}
		
		base.gameObject.name = "Player(Mine)";
		localPlayer = this;
		layerMask = -33025;
		if (camera == null)
		{
			camera = Camera.main;
		}
		if (camera != null)
		{
			mainMenu = camera.GetComponent<menu>();
		}
		else
		{
			Debug.LogWarning("FirstPersonControl: Camera is not assigned and Camera.main was not found.");
		}
		renderer = base.GetComponent<Renderer>();
		renderer.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
		renderer.material.SetTextureOffset("_MainTex", new Vector2(1, 0));
	}
	
	private void Update()
	{
		if (!base.networkView.isMine)
		{
			return;
		}
		base.transform.localRotation = Quaternion.Euler(0f, camera.transform.localEulerAngles.y, 0f);
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		moveDir = new Vector3(axis, 0f, axis2);
		moveDir = base.transform.TransformDirection(moveDir);
		moveDir *= moveSpeed;
		if (Input.GetButton("Run"))
		{
			moveDir *= runMultiplier;
			if (camera.fieldOfView < 80f)
			{
				camera.fieldOfView += (80f - camera.fieldOfView) / 0.1f * Time.deltaTime;
			}
		}
		else if (Input.GetButton("Walk"))
		{
			moveDir *= crouchMultiplier;
		}
		else if (camera.fieldOfView > 70f)
		{
			camera.fieldOfView -= Mathf.Abs(70f - camera.fieldOfView) / 0.1f * Time.deltaTime;
		}
		if ((Input.GetButtonUp("Fire1") || Input.GetButtonUp("LeftHand")) && (bool)leftArmObject)
		{
			base.networkView.RPC("setObjectPosition", RPCMode.All, leftArm.FindChild("hand").transform.position + leftArm.FindChild("hand").transform.forward * 2f, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f), leftArmObject.networkView.viewID);
			base.networkView.RPC("setObjectGravity", RPCMode.All, true, leftArmObject.networkView.viewID);
			base.networkView.RPC("setObjectCollisions", RPCMode.All, true, leftArmObject.networkView.viewID);
			leftArmObject.networkView.RPC("SetBeingHeld", RPCMode.All, leftArmObject.networkView.viewID, false, leftArm.networkView.viewID, base.networkView.viewID);
			if (leftArmObject.gameObject.layer == 8)
			{
				leftArmObject.gameObject.layer = 0;
			}
			leftArmObject = null;
			leftArmPickup = null;
		}
		if ((Input.GetButtonUp("Fire2") || Input.GetButtonUp("RightHand")) && (bool)rightArmObject)
		{
			base.networkView.RPC("setObjectPosition", RPCMode.All, rightArm.FindChild("hand").transform.position + rightArm.FindChild("hand").transform.forward * 2f, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f), rightArmObject.networkView.viewID);
			base.networkView.RPC("setObjectGravity", RPCMode.All, true, rightArmObject.networkView.viewID);
			base.networkView.RPC("setObjectCollisions", RPCMode.All, true, rightArmObject.networkView.viewID);
			rightArmObject.networkView.RPC("SetBeingHeld", RPCMode.All, rightArmObject.networkView.viewID, false, rightArm.networkView.viewID, base.networkView.viewID);
			if (rightArmObject.gameObject.layer == 8)
			{
				rightArmObject.gameObject.layer = 0;
			}
			rightArmObject = null;
			rightArmPickup = null;
		}
		if (Input.GetButtonDown("LeftHand"))
		{
			if (leftArm == null)
			{
				leftArm = (Transform)Network.Instantiate(arm, base.transform.position - base.transform.right + base.transform.up + base.transform.forward, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f), 1);
			}
			else
			{
				leftArm.position = base.transform.position - base.transform.right + base.transform.up + base.transform.forward;
				leftArm.rotation = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f);
				base.networkView.RPC("SetArmState", RPCMode.All, leftArm.networkView.viewID, true);
			}
		}
		else if (Input.GetButton("LeftHand") && (bool)leftArm)
		{
			if (camera.transform.rotation.eulerAngles.x <= maxCameraAngleFromZero)
			{
				armForwardBasedOnRotation = armExtraReach * (camera.transform.rotation.eulerAngles.x / maxCameraAngleFromZero);
			}
			else if (camera.transform.rotation.eulerAngles.x >= 360f - maxCameraAngleFromZero)
			{
				armForwardBasedOnRotation = armExtraReach * (Mathf.Abs(360f - camera.transform.rotation.eulerAngles.x) / maxCameraAngleFromZero);
			}
			leftArm.position = Vector3.Lerp(leftArm.position, base.transform.position - base.transform.right + base.transform.up * 1.5f + base.transform.forward + camera.transform.forward * armForwardBasedOnRotation, 25f * Time.deltaTime);
			leftArm.rotation = Quaternion.Lerp(leftArm.rotation, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f), 20f * Time.deltaTime);
		}
		else if (Input.GetButtonUp("LeftHand") && leftArm != null)
		{
			base.networkView.RPC("SetArmState", RPCMode.All, leftArm.networkView.viewID, false);
		}
		if (Input.GetButton("Fire1") && Input.GetButton("LeftHand"))
		{
			RaycastHit hitInfo;
			if ((bool)leftArmObject)
			{
				if ((bool)leftArmPickup)
				{
					if (!leftArmPickup.beingUsed)
					{
						Vector3 to = leftArm.FindChild("hand").transform.position + leftArm.FindChild("hand").transform.forward * 2f * leftArmPickup.heldPositionOffset.z + leftArm.FindChild("hand").transform.right * leftArmPickup.heldPositionOffset.x + leftArm.FindChild("hand").transform.up * leftArmPickup.heldPositionOffset.y;
						Quaternion to2 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x + leftArmPickup.heldRotateXOffset, 0f, 0f);
						leftArmObject.position = Vector3.Lerp(leftArmObject.position, to, 30f * Time.deltaTime);
						leftArmObject.rotation = Quaternion.Lerp(leftArmObject.rotation, to2, 30f * Time.deltaTime);
					}
					else
					{
						Vector3 to3 = camera.transform.position + camera.transform.forward * 1.5f + camera.transform.right * (leftArmPickup.heldPositionOffset.x * 0.3f);
						Quaternion to4 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f);
						leftArmObject.position = Vector3.Lerp(leftArmObject.position, to3, 30f * Time.deltaTime);
						leftArmObject.rotation = Quaternion.Lerp(leftArmObject.rotation, to4, 30f * Time.deltaTime);
					}
				}
				else
				{
					Vector3 to5 = leftArm.FindChild("hand").transform.position + leftArm.FindChild("hand").transform.forward * 2f;
					Quaternion to6 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f);
					leftArmObject.position = Vector3.Lerp(leftArmObject.position, to5, 30f * Time.deltaTime);
					leftArmObject.rotation = Quaternion.Lerp(leftArmObject.rotation, to6, 30f * Time.deltaTime);
				}
				if ((bool)leftArmObject.rigidbody && !leftArmObject.rigidbody.isKinematic)
				{
					leftArmObject.rigidbody.velocity = Vector3.zero;
					leftArmObject.rigidbody.angularVelocity = Vector3.zero;
				}
			}
			else if (Physics.SphereCast(leftArm.FindChild("hand").transform.position, 0.25f, leftArm.transform.forward, out hitInfo, 4.75f, layerMask) && (hitInfo.transform.gameObject.tag.Contains("Physics") || hitInfo.transform.root.tag.Contains("Physics")))
			{
				if (hitInfo.transform.gameObject.tag.Contains("Physics"))
				{
					leftArmObject = hitInfo.transform;
				}
				else if (hitInfo.transform.root.tag.Contains("Physics"))
				{
					leftArmObject = hitInfo.transform.root;
				}
				if (leftArmObject.gameObject.layer == 0)
				{
					leftArmObject.gameObject.layer = 8;
				}
				base.networkView.RPC("setObjectGravity", RPCMode.All, false, leftArmObject.networkView.viewID);
				leftArmPickup = leftArmObject.GetComponent<PickupObject>();
				leftArmPickup.ResetHeldRotation();
				leftArmObject.networkView.RPC("SetBeingHeld", RPCMode.All, leftArmObject.networkView.viewID, true, leftArm.networkView.viewID, base.networkView.viewID);
			}
		}
		if (Input.GetButtonDown("RightHand"))
		{
			if (rightArm == null)
			{
				rightArm = (Transform)Network.Instantiate(arm, base.transform.position + base.transform.right + base.transform.up + base.transform.forward, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f), 1);
			}
			else
			{
				rightArm.position = base.transform.position + base.transform.right + base.transform.up + base.transform.forward;
				rightArm.rotation = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, camera.transform.rotation.eulerAngles.z);
				base.networkView.RPC("SetArmState", RPCMode.All, rightArm.networkView.viewID, true);
			}
		}
		else if (Input.GetButton("RightHand") && (bool)rightArm)
		{
			if (camera.transform.rotation.eulerAngles.x <= maxCameraAngleFromZero)
			{
				armForwardBasedOnRotation = armExtraReach * (camera.transform.rotation.eulerAngles.x / maxCameraAngleFromZero);
			}
			else if (camera.transform.rotation.eulerAngles.x >= 360f - maxCameraAngleFromZero)
			{
				armForwardBasedOnRotation = armExtraReach * (Mathf.Abs(360f - camera.transform.rotation.eulerAngles.x) / maxCameraAngleFromZero);
			}
			if (!rightArmObject || !rightArmPickup.IsBeingUsed())
			{
				rightArm.position = Vector3.Lerp(rightArm.position, base.transform.position + base.transform.right + base.transform.up * 1.5f + base.transform.forward + camera.transform.forward * armForwardBasedOnRotation, 25f * Time.deltaTime);
				rightArm.rotation = Quaternion.Lerp(rightArm.rotation, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f), 20f * Time.deltaTime);
			}
			else if ((bool)rightArmObject && rightArmPickup.IsBeingUsed())
			{
				rightArm.position = Vector3.Lerp(rightArm.position, base.transform.position + base.transform.right * 1.6f + base.transform.up * 1.5f + base.transform.forward + -camera.transform.forward * 1f, 25f * Time.deltaTime);
				rightArm.rotation = Quaternion.Lerp(rightArm.rotation, base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f), 20f * Time.deltaTime);
			}
		}
		else if (Input.GetButtonUp("RightHand") && rightArm != null)
		{
			base.networkView.RPC("SetArmState", RPCMode.All, rightArm.networkView.viewID, false);
		}
		if (Input.GetButton("Fire2") && Input.GetButton("RightHand"))
		{
			RaycastHit hitInfo2;
			if ((bool)rightArmObject)
			{
				if ((bool)rightArmPickup)
				{
					if (!rightArmPickup.beingUsed)
					{
						Vector3 to7 = rightArm.FindChild("hand").transform.position + rightArm.FindChild("hand").transform.forward * 2f * rightArmPickup.heldPositionOffset.z + rightArm.FindChild("hand").transform.right * (0f - rightArmPickup.heldPositionOffset.x) + rightArm.FindChild("hand").transform.up * rightArmPickup.heldPositionOffset.y;
						Quaternion to8 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x + rightArmPickup.heldRotateXOffset, 0f, 0f);
						rightArmObject.position = Vector3.Lerp(rightArmObject.position, to7, 30f * Time.deltaTime);
						rightArmObject.rotation = Quaternion.Lerp(rightArmObject.rotation, to8, 30f * Time.deltaTime);
					}
					else
					{
						Vector3 to9 = camera.transform.position + camera.transform.forward * 1.5f + camera.transform.right * (rightArmPickup.heldPositionOffset.x * 0.3f);
						Quaternion to10 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x, 0f, 0f);
						rightArmObject.position = Vector3.Lerp(rightArmObject.position, to9, 30f * Time.deltaTime);
						rightArmObject.rotation = Quaternion.Lerp(rightArmObject.rotation, to10, 30f * Time.deltaTime);
					}
				}
				else
				{
					Vector3 to11 = rightArm.FindChild("hand").transform.position + rightArm.FindChild("hand").transform.forward * 2f;
					Quaternion to12 = base.transform.rotation * Quaternion.Euler(camera.transform.rotation.eulerAngles.x - 30f, 0f, 0f);
					rightArmObject.position = Vector3.Lerp(rightArmObject.position, to11, 30f * Time.deltaTime);
					rightArmObject.rotation = Quaternion.Lerp(rightArmObject.rotation, to12, 30f * Time.deltaTime);
				}
				if ((bool)rightArmObject.rigidbody && !rightArmObject.rigidbody.isKinematic)
				{
					rightArmObject.rigidbody.velocity = Vector3.zero;
					rightArmObject.rigidbody.angularVelocity = Vector3.zero;
				}
			}
			else if (Physics.SphereCast(rightArm.FindChild("hand").transform.position, 0.25f, rightArm.transform.forward, out hitInfo2, 4.75f, layerMask) && (hitInfo2.transform.gameObject.tag.Contains("Physics") || hitInfo2.transform.root.tag.Contains("Physics")))
			{
				if (hitInfo2.transform.gameObject.tag.Contains("Physics"))
				{
					rightArmObject = hitInfo2.transform;
				}
				else if (hitInfo2.transform.root.tag.Contains("Physics"))
				{
					rightArmObject = hitInfo2.transform.root;
				}
				if (rightArmObject.gameObject.layer == 0)
				{
					rightArmObject.gameObject.layer = 8;
				}
				base.networkView.RPC("setObjectGravity", RPCMode.All, false, rightArmObject.networkView.viewID);
				rightArmPickup = rightArmObject.GetComponent<PickupObject>();
				rightArmPickup.ResetHeldRotation();
				rightArmPickup.networkView.RPC("SetBeingHeld", RPCMode.All, rightArmObject.networkView.viewID, true, rightArm.networkView.viewID, base.networkView.viewID);
			}
		}
		RaycastHit hitInfo3;
		if (Input.GetButtonDown("Fire2") && Input.GetButton("RightHand") && Physics.SphereCast(rightArm.FindChild("hand").transform.position + rightArm.FindChild("hand").transform.right * 0.5f, 0.5f, rightArm.transform.forward, out hitInfo3, 15f, layerMask) && hitInfo3.transform.gameObject.tag == "NPC")
		{
			NPC component = hitInfo3.transform.GetComponent<NPC>();
			if (!component.isFollowingPlayer)
			{
				component.networkView.RPC("FollowAPlayer", RPCMode.All, base.gameObject.networkView.viewID);
			}
			else
			{
				component.networkView.RPC("FollowNoPlayer", RPCMode.All);
			}
		}
		gravityToApply += gravity;
		moveDir.y -= gravityToApply;
		moveDir *= Time.deltaTime;
		controller.Move(moveDir);
		if (controller.isGrounded)
		{
			gravityToApply = 0f;
		}
	}
	
	private void OnPlayerConnected(NetworkPlayer player)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Arm");
		foreach (GameObject gameObject in array)
		{
			MonoBehaviour.print("Found arm");
			base.networkView.RPC("SetArmState", RPCMode.Others, gameObject.networkView.viewID, gameObject.collider.renderer.enabled);
		}
	}
	
	[RPC]
	private void SetArmState(NetworkViewID armID, bool active)
	{
		GameObject gameObject = NetworkView.Find(armID).gameObject;
		GameObject gameObject2 = gameObject.transform.GetChild(0).gameObject;
		gameObject.renderer.enabled = active;
		gameObject2.renderer.enabled = active;
		gameObject2.GetComponent<BoxCollider>().enabled = active;
	}
	
	[RPC]
	private void DestroyObject(NetworkViewID id)
	{
		GameObject gameObject = NetworkView.Find(id).gameObject;
		if (gameObject.tag.Equals("Arm"))
		{
			Network.RemoveRPCs(gameObject.transform.GetChild(0).networkView.viewID);
			Network.Destroy(gameObject.transform.GetChild(0).gameObject);
		}
		Network.RemoveRPCs(id);
		Network.Destroy(gameObject);
	}
	
	[RPC]
	private void setObjectGravity(bool grav, NetworkViewID id)
	{
		Transform parent = NetworkView.Find(id).transform;
		if (parent.name.Equals("PlateModel") || parent.name.Equals("burger-bottom"))
		{
			parent = parent.parent;
		}
		if ((bool)parent.rigidbody)
		{
			parent.rigidbody.useGravity = grav;
		}
	}
	
	[RPC]
	private void setObjectKematic(bool kematic, NetworkViewID id)
	{
		Transform parent = NetworkView.Find(id).transform;
		if (parent.name.Equals("PlateModel") || parent.name.Equals("burger-bottom"))
		{
			parent = parent.parent;
		}
		parent.rigidbody.isKinematic = kematic;
	}
	
	[RPC]
	private void setObjectCollisions(bool collide, NetworkViewID id)
	{
		Transform parent = NetworkView.Find(id).transform;
		if (parent.name.Equals("PlateModel") || parent.name.Equals("burger-bottom"))
		{
			parent = parent.parent;
		}
		if (!parent.name.Contains("notepad") && !parent.name.Contains("StaffMenu"))
		{
			parent.collider.enabled = collide;
			if (parent.collider.isTrigger && !collide)
			{
				MonoBehaviour.print("Error: unexpected isTrigger state");
			}
			parent.collider.isTrigger = !collide;
		}
	}
	
	[RPC]
	private void setObjectPosition(Vector3 pos, Quaternion rot, NetworkViewID id)
	{
		Transform parent = NetworkView.Find(id).transform;
		if (parent.name.Equals("PlateModel") || parent.name.Equals("burger-bottom"))
		{
			parent = parent.parent;
		}
		if ((bool)parent.rigidbody && !parent.rigidbody.isKinematic)
		{
			parent.rigidbody.velocity = Vector3.zero;
			parent.rigidbody.angularVelocity = Vector3.zero;
		}
		parent.position = Vector3.Lerp(parent.position, pos, 30f * Time.deltaTime);
		parent.rotation = Quaternion.Lerp(parent.rotation, rot, 30f * Time.deltaTime);
	}
	
	[RPC]
	private void SetUsername(NetworkViewID nametagID, string username)
	{
		TextMesh component = NetworkView.Find(nametagID).gameObject.GetComponent<TextMesh>();
		component.transform.root.GetComponent<FirstPersonControl>().username = username;
		component.text = username;
	}
	
}