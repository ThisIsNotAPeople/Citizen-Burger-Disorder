using UnityEngine;

public class GElement : MonoBehaviour
{
	public GInterface parentInterface;

	public INavigation parentNavigation;

	public GameObject textPrefab;

	private TextMesh attachedText;

	public Vector3 origin;

	public Vector3 up;

	public Vector3 left;

	public Rect bounds = new Rect(0f, 0f, 10f, 10f);

	public float zLayer = 0.01f;

	public Color color;

	protected Color normalColor;

	public Color textColor;

	public string text;

	public int textSize;

	protected Material defaultMaterial;

	private void Awake()
	{
		text = "< INIT >";
		CreateText(new Vector2(bounds.x, bounds.y));
		normalColor = color;
		defaultMaterial = base.renderer.material;
	}

	private void Start()
	{
	}

	[RPC]
	public void SetText(string txt, bool hide = false)
	{
		text = txt;
		HideText(hide);
	}

	public void SetMaterialToFoodLocal(string food)
	{
		base.renderer.material = Menu.GetFoodMaterial(food);
	}

	[RPC]
	public void SetMaterialToFood(NetworkViewID id, string food)
	{
		Transform transform = NetworkView.Find(id).transform;
		transform.renderer.material = Menu.GetFoodMaterial(food);
	}

	[RPC]
	public void ResetMaterial(NetworkViewID id)
	{
		Transform transform = NetworkView.Find(id).transform;
		transform.renderer.material = transform.GetComponent<GElement>().defaultMaterial;
	}

	public bool SetUseable(bool active)
	{
		bool result = false;
		if ((bool)GetComponent<GButton>())
		{
			GetComponent<GButton>().usable = active;
			result = true;
		}
		return result;
	}

	public bool SetColor()
	{
		color = normalColor;
		return true;
	}

	public bool SetColor(Color c, string name = "")
	{
		bool result = false;
		switch (name)
		{
		case "":
			normalColor = c;
			color = c;
			result = true;
			break;
		case "highlight":
			if ((bool)GetComponent<GButton>())
			{
				GetComponent<GButton>().hoverColor = c;
				result = true;
			}
			break;
		case "pressed":
			if ((bool)GetComponent<GButton>())
			{
				GetComponent<GButton>().pressedColor = c;
				result = true;
			}
			break;
		}
		return result;
	}

	public void CalculatePositioning()
	{
		Vector3 position = base.transform.position;
		up = base.transform.up * base.transform.localScale.y * 0.5f;
		left = base.transform.right * base.transform.localScale.x * 0.5f;
		Vector3 vector = position + base.transform.forward * base.transform.localScale.z * 0.51f;
		origin = vector + up + left;
	}

	public void HideText(bool hide = true)
	{
		attachedText.gameObject.SetActive(!hide);
	}

	public bool IsTextHidden()
	{
		return attachedText.gameObject.activeSelf;
	}

	private void CreateText(Vector2 position, int textSize = 16)
	{
		if (!attachedText)
		{
			GameObject gameObject = Object.Instantiate(textPrefab, base.transform.position + base.transform.up * 0.6f - base.transform.forward * 0.01f, base.transform.rotation) as GameObject;
			attachedText = gameObject.GetComponent<TextMesh>();
			gameObject.transform.parent = base.transform;
			attachedText.fontSize = textSize;
			attachedText.renderer.material.SetColor("_Text Color", textColor);
			textSize = attachedText.fontSize;
		}
	}

	public void RefreshDisplay()
	{
		if (base.renderer.material.color != color)
		{
			base.renderer.material.color = color;
		}
		if (attachedText.renderer.material.GetColor("_Color") != textColor)
		{
			attachedText.renderer.material.SetColor("_Color", textColor);
		}
		if (!attachedText.text.Equals(text))
		{
			attachedText.text = text;
		}
		if (attachedText.fontSize != textSize)
		{
			attachedText.fontSize = textSize;
		}
	}

	public virtual void Update()
	{
		CalculatePositioning();
		RefreshDisplay();
		parentInterface.DrawElement(this);
	}

	public void SetInterface(GInterface gi)
	{
		parentInterface = gi;
		base.transform.parent = gi.transform;
	}

	public void SetInterface(INavigation inav)
	{
		parentInterface = inav;
		base.transform.parent = inav.transform;
	}
}
