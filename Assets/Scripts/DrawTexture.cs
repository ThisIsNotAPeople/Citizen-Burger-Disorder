using UnityEngine;

[RequireComponent(typeof(ObjectUsable))]
public class DrawTexture : MonoBehaviour
{
	public bool initNewTexOnStart = true;

	public bool editable = true;

	public Vector2 lastDrawPos = Vector2.zero;

	public bool seenTutorial;

	public Material generalTutorial;

	public Material leftHandTutorial;

	public Material rightHandTutorial;

	public Material usingLeftHandTutorial;

	public Material usingRightHandTutorial;

	public Material paperTutorial;

	private ObjectUsable obj;

	private void Start()
	{
		obj = GetComponent<ObjectUsable>();
		if (initNewTexOnStart)
		{
			NewTex();
		}
	}

	private void Update()
	{
		if (!editable)
		{
			return;
		}
		if (!seenTutorial)
		{
			if (obj.holdingRightHandObject)
			{
				if (obj.beingUsed)
				{
					base.renderer.material.SetTexture("_Drawing", usingRightHandTutorial.GetTexture("_Drawing"));
				}
				else
				{
					base.renderer.material.SetTexture("_Drawing", rightHandTutorial.GetTexture("_Drawing"));
				}
			}
			else if (obj.holdingLeftHandObject)
			{
				if (obj.beingUsed)
				{
					base.renderer.material.SetTexture("_Drawing", usingLeftHandTutorial.GetTexture("_Drawing"));
				}
				else
				{
					base.renderer.material.SetTexture("_Drawing", leftHandTutorial.GetTexture("_Drawing"));
				}
			}
			else
			{
				base.renderer.material.SetTexture("_Drawing", generalTutorial.GetTexture("_Drawing"));
			}
		}
		if (obj.beingUsed && seenTutorial)
		{
			if (((Input.GetButton("Fire1") && obj.usingRightHandObject) || (Input.GetButton("Fire2") && obj.usingLeftHandObject)) && (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f || Input.GetButtonDown("Fire1")))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				int layerMask = 1 << LayerMask.NameToLayer("Drawable");
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, 2f, layerMask))
				{
					DrawOnTransformTextureAtCursorPosition(hitInfo.transform, hitInfo);
				}
			}
			if (obj.usingRightHandObject && Input.GetButton("LeftHand") && Input.GetButtonDown("Fire1"))
			{
				GameObject gameObject = Network.Instantiate(Resources.Load("Prefabs/Drawing/Paper"), obj.control.leftArm.GetChild(0).transform.position + obj.control.leftArm.GetChild(0).transform.forward * 0.5f, obj.control.leftArm.transform.rotation, 0) as GameObject;
				base.networkView.RPC("MoveNotePageToPaper", RPCMode.All, base.networkView.viewID, gameObject.networkView.viewID, obj.control.networkView.viewID, false);
				obj.control.leftArmObject = gameObject.transform;
				obj.StopUsingObject();
			}
			else if (obj.usingLeftHandObject && Input.GetButton("RightHand") && Input.GetButtonDown("Fire2"))
			{
				GameObject gameObject2 = Network.Instantiate(Resources.Load("Prefabs/Drawing/Paper"), obj.control.rightArm.GetChild(0).transform.position + obj.control.rightArm.GetChild(0).transform.forward * 0.5f, obj.control.rightArm.transform.rotation, 0) as GameObject;
				base.networkView.RPC("MoveNotePageToPaper", RPCMode.All, base.networkView.viewID, gameObject2.networkView.viewID, obj.control.networkView.viewID, false);
				obj.control.rightArmObject = gameObject2.transform;
				obj.StopUsingObject();
			}
			if (obj.usingLeftHandObject && Input.GetButtonUp("Fire2"))
			{
				base.networkView.RPC("SetLastDraw", RPCMode.All, obj.control.networkView.viewID, Vector3.zero);
			}
			if (obj.usingRightHandObject && Input.GetButtonUp("Fire1"))
			{
				base.networkView.RPC("SetLastDraw", RPCMode.All, obj.control.networkView.viewID, Vector3.zero);
			}
		}
		else if (obj.beingUsed && !seenTutorial)
		{
			if (obj.usingRightHandObject && Input.GetButton("LeftHand") && Input.GetButtonDown("Fire1"))
			{
				GameObject gameObject3 = Network.Instantiate(Resources.Load("Prefabs/Drawing/Paper"), obj.control.leftArm.GetChild(0).transform.position + obj.control.leftArm.GetChild(0).transform.forward * 0.5f, obj.control.leftArm.transform.rotation, 0) as GameObject;
				base.networkView.RPC("MoveNotePageToPaper", RPCMode.All, base.networkView.viewID, gameObject3.networkView.viewID, obj.control.networkView.viewID, true);
				obj.control.leftArmObject = gameObject3.transform;
				seenTutorial = true;
				obj.StopUsingObject();
			}
			else if (obj.usingLeftHandObject && Input.GetButton("RightHand") && Input.GetButtonDown("Fire2"))
			{
				GameObject gameObject4 = Network.Instantiate(Resources.Load("Prefabs/Drawing/Paper"), obj.control.rightArm.GetChild(0).transform.position + obj.control.rightArm.GetChild(0).transform.forward * 0.5f, obj.control.rightArm.transform.rotation, 0) as GameObject;
				base.networkView.RPC("MoveNotePageToPaper", RPCMode.All, base.networkView.viewID, gameObject4.networkView.viewID, obj.control.networkView.viewID, true);
				obj.control.rightArmObject = gameObject4.transform;
				seenTutorial = true;
				obj.StopUsingObject();
			}
		}
	}

	private void DrawOnTransformTextureAtCursorPosition(Transform transformToDrawOn, RaycastHit hit)
	{
		int num = 4;
		Texture2D texture2D = transformToDrawOn.renderer.material.GetTexture("_Drawing") as Texture2D;
		Vector2 textureCoord = hit.textureCoord;
		textureCoord.x *= texture2D.width;
		textureCoord.y *= texture2D.height;
		base.networkView.RPC("DrawOnTexture", RPCMode.All, obj.control.networkView.viewID, transformToDrawOn.networkView.viewID, (int)textureCoord.x, (int)textureCoord.y, num);
	}

	[RPC]
	private void MoveNotePageToPaper(NetworkViewID targetNotepad, NetworkViewID targetPaper, NetworkViewID playerCreating, bool ignoreTexture = false)
	{
		GameObject gameObject = NetworkView.Find(targetNotepad).gameObject;
		GameObject gameObject2 = NetworkView.Find(targetPaper).gameObject;
		gameObject2.GetComponent<DrawTexture>().initNewTexOnStart = false;
		if (!seenTutorial)
		{
			seenTutorial = true;
		}
		Texture2D texture2D = (ignoreTexture ? GetNewTex() : (Object.Instantiate(gameObject.renderer.material.GetTexture("_Drawing")) as Texture2D));
		Texture2D texture2D2 = new Texture2D(256, 256, TextureFormat.ARGB32, false);
		texture2D2.SetPixels(texture2D.GetPixels(228, 15, 256, 256));
		texture2D2.Apply();
		if (!ignoreTexture)
		{
			gameObject2.renderer.material.SetTexture("_Drawing", texture2D2);
		}
		else
		{
			gameObject2.renderer.material.SetTexture("_Drawing", paperTutorial.GetTexture("_Drawing"));
		}
		gameObject.GetComponent<DrawTexture>().NewTex();
	}

	[RPC]
	private void DrawOnTexture(NetworkViewID callingPlayer, NetworkViewID targetTransform, int pixelX, int pixelY, int brushSize)
	{
		FirstPersonControl component = NetworkView.Find(callingPlayer).GetComponent<FirstPersonControl>();
		Texture2D texture2D = NetworkView.Find(targetTransform).renderer.material.GetTexture("_Drawing") as Texture2D;
		DrawTexture component2 = NetworkView.Find(targetTransform).GetComponent<DrawTexture>();
		Color[] array = new Color[25];
		for (int i = 0; i < 25; i++)
		{
			array[i] = Color.black;
		}
		Vector2 lastDrawPosition = new Vector2(pixelX, pixelY);
		if (component.lastDrawPosition != Vector2.zero)
		{
			float magnitude = new Vector2(component.lastDrawPosition.x - lastDrawPosition.x, component.lastDrawPosition.y - lastDrawPosition.y).magnitude;
			if (magnitude > 3f)
			{
				float num = Mathf.Round(magnitude / 3f);
				for (int j = 0; (float)j < num; j++)
				{
					texture2D.SetPixels((int)Mathf.Lerp(component.lastDrawPosition.x, lastDrawPosition.x, (float)j / num), (int)Mathf.Lerp(component.lastDrawPosition.y, lastDrawPosition.y, (float)j / num), brushSize, brushSize, array);
				}
				texture2D.Apply();
			}
		}
		component.lastDrawPosition = lastDrawPosition;
		texture2D.SetPixels(pixelX, pixelY, brushSize, brushSize, array);
		texture2D.Apply();
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 value = new Vector3(lastDrawPos.x, lastDrawPos.y, 0f);
			stream.Serialize(ref value);
		}
		else
		{
			Vector3 value2 = Vector3.zero;
			stream.Serialize(ref value2);
			lastDrawPos = new Vector2(value2.x, value2.y);
		}
	}

	[RPC]
	public void SetLastDraw(NetworkViewID playerID, Vector3 lastDraw3)
	{
		FirstPersonControl component = NetworkView.Find(playerID).GetComponent<FirstPersonControl>();
		component.lastDrawPosition = new Vector2(lastDraw3.x, lastDraw3.y);
	}

	public Texture2D GetNewTex()
	{
		Texture2D texture2D = new Texture2D(512, 512, TextureFormat.ARGB32, true);
		Color[] array = new Color[262144];
		for (int i = 0; i < 262144; i++)
		{
			array[i] = Color.white;
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	[RPC]
	public void NewTex()
	{
		Texture2D texture2D = new Texture2D(512, 512, TextureFormat.ARGB32, true);
		Color[] array = new Color[262144];
		for (int i = 0; i < 262144; i++)
		{
			array[i] = Color.white;
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		base.renderer.material.SetTexture("_Drawing", texture2D);
	}
}
