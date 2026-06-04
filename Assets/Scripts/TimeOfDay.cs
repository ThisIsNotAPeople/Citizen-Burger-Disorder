using UnityEngine;

public class TimeOfDay : MonoBehaviour
{
	public static float currentTimeOfDay;

	public static float dayStart;

	public static float startTimeOfDay = 8f;

	public static float endTimeOfDay = 22f;

	private static float secondsInAnHour = 60f;

	private Color dayColor;

	private Color nightColor;

	private Color currentColor;

	private void Start()
	{
		dayColor = RenderSettings.ambientLight;
		currentColor = RenderSettings.ambientLight;
		if (Network.isServer)
		{
			nightColor = new Color(0.05f, 0.05f, 0.05f);
			dayStart = (float)Network.time;
		}
	}

	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncTimeOfDay", RPCMode.Others, currentTimeOfDay, dayStart);
	}

	[RPC]
	private void SyncTimeOfDay(float time, float timeStart)
	{
		MonoBehaviour.print("Set time of day to: " + time + " and started at " + timeStart);
		currentTimeOfDay = time;
		dayStart = timeStart;
	}

	private void OnGUI()
	{
		GUI.skin.label.fontSize = 12;
		GUI.Label(new Rect(0f, 0f, 500f, 20f), "Time of day: " + Mathf.Round(currentTimeOfDay * 10f) / 10f);
	}

	private void Update()
	{
		if (currentTimeOfDay > 12f)
		{
			RenderSettings.ambientLight = Color.Lerp(dayColor, nightColor, (currentTimeOfDay - 16f) / 3f);
		}
		else
		{
			RenderSettings.ambientLight = Color.Lerp(nightColor, dayColor, (currentTimeOfDay - 8f) / 2f);
		}
		currentTimeOfDay = startTimeOfDay + ((float)Network.time - dayStart) / secondsInAnHour;
		if (currentTimeOfDay >= endTimeOfDay)
		{
			dayStart = (float)Network.time;
			currentTimeOfDay = startTimeOfDay;
		}
	}
}
