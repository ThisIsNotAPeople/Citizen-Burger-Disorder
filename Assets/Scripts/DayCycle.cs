using UnityEngine;

public class DayCycle : MonoBehaviour
{
	public float dayLengthInSeconds = 300f;

	private float rotationPerFrame;

	private void Start()
	{
		rotationPerFrame = 360f / dayLengthInSeconds;
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(rotationPerFrame * Time.deltaTime, 0f, 0f));
	}
}
