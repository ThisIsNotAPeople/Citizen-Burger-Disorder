using UnityEngine;

public class GButton : GElement
{
	private FirstPersonControl player;

	public bool usable = true;

	public Color hoverColor = Color.Lerp(Color.blue, Color.white, 0.7f);

	public Color pressedColor = Color.Lerp(Color.green, Color.white, 0.6f);

	private bool playerHoveringLeft;

	private bool playerHoveringRight;

	private bool playerPressed;

	private int layerMask;

	private bool portable;

	private ObjectUsable obj;

	private void Start()
	{
		layerMask = 1 << LayerMask.NameToLayer("Button");
		portable = base.transform.root.GetComponent<Computer>().portable;
		if (portable)
		{
			obj = base.transform.root.GetComponent<ObjectUsable>();
		}
	}

	public override void Update()
	{
		if (usable)
		{
			if ((bool)player)
			{
				if (portable)
				{
					if (obj.beingUsed)
					{
						if ((Input.GetButton("Fire1") && obj.usingRightHandObject) || (Input.GetButton("Fire2") && obj.usingLeftHandObject))
						{
							Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
							RaycastHit hitInfo;
							if (Physics.Raycast(ray, out hitInfo, 10f, layerMask) && hitInfo.collider.transform == base.collider.transform)
							{
								playerPressed = true;
								if (obj.usingRightHandObject)
								{
									playerHoveringLeft = true;
								}
								else
								{
									playerHoveringRight = true;
								}
							}
						}
						else
						{
							Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
							RaycastHit hitInfo2;
							if (Physics.Raycast(ray2, out hitInfo2, 10f, layerMask))
							{
								if (hitInfo2.collider.transform == base.collider.transform)
								{
									if (obj.usingRightHandObject)
									{
										playerHoveringLeft = true;
									}
									else
									{
										playerHoveringRight = true;
									}
								}
							}
							else
							{
								playerHoveringLeft = false;
								playerHoveringRight = false;
							}
						}
					}
				}
				else
				{
					if (Input.GetButton("LeftHand") && (bool)player.leftArm)
					{
						Ray ray3 = new Ray(player.leftArm.transform.position, player.leftArm.transform.forward);
						RaycastHit hitInfo3;
						if (Physics.Raycast(ray3, out hitInfo3, 14f, layerMask))
						{
							if (hitInfo3.collider.transform == base.collider.transform)
							{
								playerHoveringLeft = true;
							}
						}
						else
						{
							playerHoveringLeft = false;
							playerPressed = false;
						}
					}
					if (Input.GetButton("RightHand") && (bool)player.rightArm)
					{
						RaycastHit hitInfo4;
						if (Physics.Raycast(player.rightArm.transform.position, player.rightArm.transform.forward, out hitInfo4, 6f, layerMask))
						{
							if (hitInfo4.collider.transform == base.collider.transform)
							{
								playerHoveringRight = true;
							}
						}
						else
						{
							playerHoveringRight = false;
							playerPressed = false;
						}
					}
					if (!Input.GetButton("LeftHand") && !Input.GetButton("RightHand"))
					{
						playerHoveringLeft = false;
						playerHoveringRight = false;
					}
					if ((Input.GetButton("Fire1") && playerHoveringLeft) || (Input.GetButton("Fire2") && playerHoveringRight))
					{
						playerPressed = true;
					}
				}
			}
			else if (Time.frameCount % 10 == 0)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				foreach (GameObject gameObject in array)
				{
					if (gameObject.GetComponent<FirstPersonControl>().networkView.isMine)
					{
						player = gameObject.GetComponent<FirstPersonControl>();
					}
				}
			}
			if (playerPressed)
			{
				color = pressedColor;
				if ((Input.GetButtonUp("Fire1") && playerHoveringLeft) || (Input.GetButtonUp("Fire2") && playerHoveringRight))
				{
					if (text == string.Empty)
					{
						MonoBehaviour.print("Error: no name.");
					}
					foreach (Computer computer in Computer.computers)
					{
						computer.networkView.RPC("SetButtonDown", RPCMode.All, text);
					}
					playerPressed = false;
				}
			}
			else if (playerHoveringRight || playerHoveringLeft)
			{
				color = Color.Lerp(hoverColor, pressedColor, 0.6f);
			}
			else
			{
				color = Color.Lerp(Color.Lerp(normalColor, hoverColor, 0.5f), Color.Lerp(normalColor, hoverColor, 1f), Mathf.Sin(Time.time * 2f) * 1f + 0.3f);
			}
		}
		playerHoveringLeft = false;
		playerHoveringRight = false;
		CalculatePositioning();
		RefreshDisplay();
		parentInterface.DrawButton(this);
	}
}
