using System.Collections.Generic;
using UnityEngine;

public class GInterface : MonoBehaviour
{
	public GameObject elementPrefab;

	public GameObject buttonPrefab;

	public List<GElement> graphicElements = new List<GElement>();

	public GScreen screen;

	public Vector3 origin;

	public Vector3 up;

	public Vector3 left;

	public Rect bounds = new Rect(0f, 0f, 10f, 10f);

	public Vector2 scale;

	public float zLayer = 0.02f;

	public Color backgroundColor;

	public GElement CreateElement(float x, float y, float wPercent, float hPercent, string text = "", bool store = true)
	{
		GElement component = (Object.Instantiate(elementPrefab, base.transform.position + base.transform.forward * 0.5f, base.transform.rotation) as GameObject).GetComponent<GElement>();
		component.SetInterface(this);
		component.bounds = new Rect(x, y, BoundsPercentageToPixels(wPercent, 0f).x, BoundsPercentageToPixels(0f, hPercent).y);
		component.text = text;
		component.zLayer = 0.07f;
		if (store)
		{
			graphicElements.Add(component);
			component.transform.parent = base.transform;
		}
		if (Network.isServer)
		{
			component.GetComponent<NetworkView>().viewID = Network.AllocateViewID();
		}
		return component;
	}

	public GElement CreateButton(float x, float y, float wPercent, float hPercent, string text = "", bool store = true)
	{
		GButton component = (Object.Instantiate(buttonPrefab, base.transform.position + base.transform.forward * 0.5f, base.transform.rotation) as GameObject).GetComponent<GButton>();
		component.SetInterface(this);
		component.bounds = new Rect(x, y, BoundsPercentageToPixels(wPercent, 0f).x, BoundsPercentageToPixels(0f, hPercent).y);
		component.text = text;
		component.zLayer = 0.07f;
		if (store)
		{
			graphicElements.Add(component);
			component.transform.parent = base.transform;
		}
		if (Network.isServer)
		{
			component.GetComponent<NetworkView>().viewID = Network.AllocateViewID();
		}
		return component;
	}

	private void Update()
	{
		CalculatePositioning();
		screen.Draw(this);
		if (base.renderer.material.color != backgroundColor)
		{
			base.renderer.material.color = backgroundColor;
		}
	}

	public void CalculatePositioning()
	{
		Vector3 position = base.transform.position;
		up = base.transform.up * base.transform.localScale.y * 0.5f;
		left = base.transform.right * base.transform.localScale.x * 0.5f;
		Vector3 vector = position + base.transform.forward * 0.02f;
		origin = vector + up - left;
	}

	public Vector2 BoundsPercentageToPixels(float wPercent = 1f, float hPercent = 1f)
	{
		float x = wPercent * bounds.width;
		float y = hPercent * bounds.height;
		return new Vector2(x, y);
	}

	public Vector2 BoundsPixelPosition(float wPixel = 100f, float hPixel = 100f)
	{
		return screen.BoundsPixelPosition(wPixel, hPixel);
	}

	public void SetScreen(GScreen gScreen)
	{
		screen = gScreen;
		base.transform.parent = screen.transform;
	}

	public void SetBackgroundColor(Color c)
	{
		backgroundColor = c;
		base.transform.renderer.material.color = backgroundColor;
	}

	public void DrawElement(GElement ge)
	{
		screen.Draw(ge);
	}

	public void DrawButton(GButton gb)
	{
		screen.Draw(gb);
	}
}
