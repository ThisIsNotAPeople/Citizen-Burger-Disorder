using System.Collections.Generic;
using UnityEngine;

public class MovementAnimation : MonoBehaviour
{
	public List<Vector3> startPositions = new List<Vector3>();

	public List<Quaternion> startRotation = new List<Quaternion>();

	public List<Vector3> endPositions = new List<Vector3>();

	public List<Quaternion> endRotation = new List<Quaternion>();

	public List<float> speed = new List<float>();

	public List<bool> lerp = new List<bool>();

	public int currentIndex;

	private float startTime;

	private float distLength;

	private void Start()
	{
		RenderSettings.fogDensity = 0.07f;
		if (currentIndex == 2)
		{
			RenderSettings.fogDensity = 0.3f;
		}
		if (startPositions.Count == 0)
		{
			base.enabled = false;
			return;
		}
		base.transform.position = startPositions[currentIndex];
		distLength = (base.transform.position - endPositions[currentIndex]).magnitude;
	}

	private void Update()
	{
		float magnitude = (base.transform.position - endPositions[currentIndex]).magnitude;
		float num = (Time.time - startTime) * speed[currentIndex];
		float t = num / distLength;
		if (lerp[currentIndex])
		{
			base.transform.position = Vector3.Lerp(base.transform.position, endPositions[currentIndex], speed[currentIndex] * Time.deltaTime);
		}
		else
		{
			base.transform.position = Vector3.Lerp(startPositions[currentIndex], endPositions[currentIndex], t);
		}
		base.transform.rotation = Quaternion.Lerp(startRotation[currentIndex], endRotation[currentIndex], t);
		if (currentIndex == 1 && Time.time - startTime > 6f)
		{
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0.07f, speed[currentIndex] * 4.6f * Time.deltaTime);
		}
		if (currentIndex == 2 && Time.time - startTime > 4f)
		{
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0.07f, speed[currentIndex] * 4.6f * Time.deltaTime);
		}
		if (currentIndex == 3 && Time.time - startTime > 20f)
		{
			RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0.06f, speed[currentIndex] * 4.6f * Time.deltaTime);
		}
		if (magnitude < 0.8f && currentIndex != 3)
		{
			currentIndex = (currentIndex + 1) % startPositions.Count;
			startTime = Time.time;
			base.transform.position = startPositions[currentIndex];
			base.transform.rotation = startRotation[currentIndex];
			distLength = (base.transform.position - endPositions[currentIndex]).magnitude;
			if (currentIndex == 0)
			{
				RenderSettings.fogDensity = 0.085f;
			}
			else if (currentIndex == 1)
			{
				RenderSettings.fogDensity = 0.2f;
			}
			else if (currentIndex == 2)
			{
				RenderSettings.fogDensity = 0.3f;
			}
			else
			{
				RenderSettings.fogDensity = 0.07f;
			}
			if (currentIndex == 3)
			{
				RenderSettings.fogDensity = 0.13f;
				GameObject.Find("PlayerRender").renderer.enabled = false;
			}
			else
			{
				GameObject.Find("PlayerRender").renderer.enabled = true;
			}
		}
	}

	public void editStart(int index)
	{
		startPositions[index] = base.transform.position;
		startRotation[index] = base.transform.rotation;
	}

	public void editEnd(int index)
	{
		endPositions[index] = base.transform.position;
		endRotation[index] = base.transform.rotation;
	}

	public void saveNewStart()
	{
		startPositions.Add(base.transform.position);
		startRotation.Add(base.transform.rotation);
	}

	public void saveNewEnd()
	{
		endPositions.Add(base.transform.position);
		endRotation.Add(base.transform.rotation);
	}

	public void setSpeed(float newSpeed)
	{
		speed.Add(newSpeed);
	}

	public void setLerp(bool newLerp)
	{
		lerp.Add(newLerp);
	}

	public void ClearSpecific(int index)
	{
		startPositions.RemoveAt(index);
		startRotation.RemoveAt(index);
		endPositions.RemoveAt(index);
		endRotation.RemoveAt(index);
		speed.RemoveAt(index);
	}

	public void ClearLast()
	{
		int index = startPositions.Count - 1;
		startPositions.RemoveAt(index);
		startRotation.RemoveAt(index);
		endPositions.RemoveAt(index);
		endRotation.RemoveAt(index);
		speed.RemoveAt(index);
	}

	public void ClearAll()
	{
		startPositions = new List<Vector3>();
		startRotation = new List<Quaternion>();
		endPositions = new List<Vector3>();
		endRotation = new List<Quaternion>();
		speed = new List<float>();
		lerp = new List<bool>();
	}
}
