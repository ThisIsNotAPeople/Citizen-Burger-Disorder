using UnityEngine;

public class GScreen : MonoBehaviour
{
	public GameObject InterfacePrefab;

	public GameObject NavigationPrefab;

	public Computer computer;

	public Vector3 screenBasePos;

	public Vector3 screenOrigin;

	private Vector3 screenCentre;

	private Vector3 screenUp;

	private Vector3 screenDown;

	private Vector3 screenLeft;

	private Vector3 screenRight;

	private Vector2 screenBounds;

	public Vector2 screenResolution = new Vector2(800f, 450f);

	public float pixelsPerUnit;

	private INavigation navigationBar;

	private GInterface mainDisplay;

	private void Awake()
	{
		Init();
	}

	public GInterface CreateInterface(float x, float y, float wPercent, float hPercent)
	{
		GInterface component = (Object.Instantiate(InterfacePrefab, screenCentre, base.transform.rotation) as GameObject).GetComponent<GInterface>();
		component.SetScreen(this);
		component.SetBackgroundColor(Color.black);
		component.bounds = new Rect(x, y, BoundsPercentageToPixels(wPercent, 0f).x, BoundsPercentageToPixels(0f, hPercent).y);
		if (Network.isServer)
		{
			component.GetComponent<NetworkView>().viewID = Network.AllocateViewID();
		}
		return component;
	}

	public INavigation CreateNavigation(float x, float y, float wPercent, float hPercent)
	{
		INavigation component = (Object.Instantiate(NavigationPrefab, screenCentre, base.transform.rotation) as GameObject).GetComponent<INavigation>();
		component.SetScreen(this);
		component.SetBackgroundColor(Color.black);
		component.bounds = new Rect(x, y, BoundsPercentageToPixels(wPercent, 0f).x, BoundsPercentageToPixels(0f, hPercent).y);
		if (Network.isServer)
		{
			component.GetComponent<NetworkView>().viewID = Network.AllocateViewID();
		}
		return component;
	}

	private void Init()
	{
		screenBasePos = base.transform.position;
		screenUp = base.transform.up * base.transform.localScale.y * 0.5f;
		screenLeft = -base.transform.right * base.transform.localScale.x * 0.5f;
		screenDown = -screenUp;
		screenRight = -screenLeft;
		screenCentre = screenBasePos + -base.transform.forward * base.transform.localScale.z * 0.51f;
		screenOrigin = screenCentre + screenUp + screenLeft;
		screenBounds = new Vector2((screenLeft - screenRight).magnitude, (screenUp - screenDown).magnitude);
		pixelsPerUnit = screenResolution.x / screenBounds.x;
	}

	private void Update()
	{
		Init();
	}

	public Vector2 BoundsPercentageToPixels(float wPercent = 1f, float hPercent = 1f)
	{
		float x = wPercent * screenBounds.x;
		float y = hPercent * screenBounds.y;
		return new Vector2(x, y) * pixelsPerUnit;
	}

	public Vector2 BoundsPixelPosition(float wPixel = 100f, float hPixel = 100f)
	{
		float x = screenBounds.x / (screenResolution.x / wPixel);
		float y = screenBounds.y / (screenResolution.y / hPixel);
		return new Vector2(x, y) * pixelsPerUnit;
	}

	public void Draw(GInterface gi)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = screenRight * (gi.bounds.x / screenResolution.x) * 2f;
		Vector3 vector2 = screenDown * (gi.bounds.y / screenResolution.y) * 2f;
		zero = screenOrigin + vector + vector2;
		gi.transform.parent = null;
		gi.transform.localScale = new Vector3(gi.bounds.width / pixelsPerUnit, gi.bounds.height / pixelsPerUnit, gi.transform.localScale.z);
		gi.CalculatePositioning();
		gi.transform.parent = base.transform;
		gi.transform.position = zero - gi.up + gi.left + -base.transform.forward * gi.zLayer;
	}

	public void Draw(GElement ge)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = screenRight * ((ge.bounds.x + ge.parentInterface.bounds.x) / screenResolution.x) * 2f;
		Vector3 vector2 = screenDown * ((ge.bounds.y + ge.parentInterface.bounds.y) / screenResolution.y) * 2f;
		zero = screenOrigin + -base.transform.forward * ge.zLayer + vector + vector2;
		ge.transform.parent = null;
		ge.transform.localScale = new Vector3(ge.bounds.width / pixelsPerUnit, ge.bounds.height / pixelsPerUnit, ge.transform.localScale.z);
		ge.CalculatePositioning();
		ge.transform.parent = ge.parentInterface.transform;
		ge.transform.position = zero - ge.up + ge.left;
	}
}
