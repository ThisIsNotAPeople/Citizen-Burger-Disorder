using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	public float existForDuration = 4f;

	private float initTime;

	private void Start()
	{
		initTime = Time.time;
	}

	private void Update()
	{
		if (Time.time > initTime + existForDuration)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
