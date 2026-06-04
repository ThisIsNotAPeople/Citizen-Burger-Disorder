using UnityEngine;

public class NetworkPhysicsThirdPersonController : MonoBehaviour
{
	public string username = string.Empty;

	private float speed = 10f;

	private float gravity = 18f;

	private float gravityToApply;

	private float turnSpeed = 25f;

	private float currentTurnspeed = 25f;

	public float rollSpeed = 30f;

	private float rollCurrentSpeed;

	private Vector3 moveDir = Vector3.zero;

	private float timerRollStart;

	private float timerRollDuration = 2f;

	public bool jumping;

	public bool rolling;

	private Color c;

	private TextMesh userText;

	private Transform playerMovement;

	public Transform mainRenderer;

	private NetworkAnimation nani;

	private CharacterController controller;

	private void Start()
	{
		if (!base.networkView.isMine)
		{
			base.enabled = false;
			return;
		}
		generateColor();
		userText = base.transform.FindChild("Username").GetComponent<TextMesh>();
		userText.text = username;
		base.networkView.RPC("setUsername", RPCMode.OthersBuffered, username, base.networkView.viewID);
		MouseOrbit component = Camera.main.GetComponent<MouseOrbit>();
		playerMovement = GameObject.Find("bearings").transform;
		mainRenderer = base.transform.FindChild("Cube");
		nani = GetComponent<NetworkAnimation>();
		controller = GetComponent<CharacterController>();
		if (!component.enabled)
		{
			component.enabled = true;
			component.target = base.transform.FindChild("Eyes");
		}
	}

	private void Update()
	{
		if (playerMovement != null)
		{
			playerMovement.position = base.transform.position;
			playerMovement.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
		}
		if (base.networkView.isMine)
		{
			if (mainRenderer != null && mainRenderer.renderer.material.color != c)
			{
				mainRenderer.renderer.material.SetColor("_Color", c);
			}
			moveDir = new Vector3(0f - Input.GetAxis("Vertical"), 0f, Input.GetAxis("Horizontal"));
			moveDir = base.transform.TransformDirection(moveDir);
			if (!rolling)
			{
				moveDir *= speed;
			}
			if (controller.isGrounded)
			{
				gravityToApply = 0f;
				jumping = false;
				if (Input.GetButtonDown("Jump") && !rolling)
				{
					jumping = true;
					base.transform.rotation = playerMovement.rotation * Quaternion.Euler(0f, 90f, 0f);
				}
				if (Input.GetButtonDown("Roll") && !jumping)
				{
					timerRollStart = Time.time;
					rollCurrentSpeed = rollSpeed;
					rolling = true;
				}
			}
			if (jumping)
			{
				moveDir += base.transform.TransformDirection(-5f, 10f, 0f);
			}
			if (rolling)
			{
				if (Time.time < timerRollStart + timerRollDuration)
				{
					if (Time.time > timerRollStart + timerRollDuration / 4f)
					{
						rollCurrentSpeed /= 1.1f;
					}
					moveDir += base.transform.TransformDirection(new Vector3(0f - Input.GetAxis("Vertical"), 0f, Input.GetAxis("Horizontal")) * rollCurrentSpeed);
				}
				else
				{
					rolling = false;
					timerRollStart = 0f;
				}
			}
			gravityToApply += gravity * Time.deltaTime;
			moveDir.y -= gravityToApply;
			moveDir *= Time.deltaTime;
			controller.Move(moveDir);
			if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f || Input.GetButtonDown("Jump") || Input.GetButtonDown("Roll"))
			{
				currentTurnspeed = turnSpeed;
				if (!controller.isGrounded)
				{
					currentTurnspeed /= 14f;
				}
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, playerMovement.rotation * Quaternion.Euler(0f, 90f, 0f), currentTurnspeed * Time.deltaTime);
				if (rolling)
				{
					if (Time.time < timerRollStart + timerRollDuration / 3f)
					{
						if (!base.animation.IsPlaying("Roll"))
						{
							nani.SyncAnimation("Roll");
						}
					}
					else
					{
						nani.SyncAnimation("Idle");
					}
				}
				else if (!base.animation.IsPlaying("Walk_001"))
				{
					nani.SyncAnimation("Walk_001");
				}
			}
			else
			{
				nani.SyncAnimation("Idle");
			}
		}
		else
		{
			base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.y, 0f);
		}
	}

	private void generateColor()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if ((double)Random.value > 0.5)
		{
			num = 0.8f + Random.value * 0.2f;
		}
		if ((double)Random.value > 0.5)
		{
			num2 = 0.8f + Random.value * 0.2f;
		}
		if ((double)Random.value > 0.5)
		{
			num3 = 0.8f + Random.value * 0.2f;
		}
		if (num == 0f && num2 == 0f && num3 == 0f)
		{
			if ((double)Random.value > 0.3)
			{
				num = 0.7f + Random.value * 0.3f;
			}
			if ((double)Random.value > 0.3)
			{
				num2 = 0.7f + Random.value * 0.3f;
			}
			if ((double)Random.value > 0.3)
			{
				num3 = 0.7f + Random.value * 0.3f;
			}
		}
		if (num == 0f && num2 == 0f && num3 == 0f)
		{
			if ((double)Random.value > 0.1)
			{
				num = 0.4f + Random.value * 0.4f;
			}
			if ((double)Random.value > 0.1)
			{
				num2 = 0.4f + Random.value * 0.4f;
			}
			if ((double)Random.value > 0.1)
			{
				num3 = 0.4f + Random.value * 0.4f;
			}
		}
		c = new Color(num, num2, num3);
		c += new Color(0.6f, 0.6f, 0.6f);
		base.networkView.RPC("setColor", RPCMode.OthersBuffered, num, num2, num3, base.networkView.viewID);
	}

	[RPC]
	private void setUsername(string u, NetworkViewID id)
	{
		if (!base.networkView.isMine)
		{
			NetworkView.Find(id).transform.FindChild("Username").GetComponent<TextMesh>().text = u;
			MonoBehaviour.print(string.Concat("Set ", id, " to have ", u, " as username."));
		}
	}

	[RPC]
	private void setColor(float r, float g, float b, NetworkViewID id)
	{
		if (!base.networkView.isMine)
		{
			c = new Color(r, g, b);
			c += new Color(0.6f, 0.6f, 0.6f);
			Transform transform = NetworkView.Find(id).transform.FindChild("Cube");
			transform.renderer.material.SetColor("_Color", c);
		}
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 value = base.transform.position;
			stream.Serialize(ref value);
		}
		else
		{
			Vector3 value2 = Vector3.zero;
			stream.Serialize(ref value2);
			base.transform.position = value2;
		}
	}
}
