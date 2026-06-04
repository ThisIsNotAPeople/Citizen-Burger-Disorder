using UnityEngine;

public class RotateToLookAtCamera : MonoBehaviour
{
	public float YFlip = -180f;

	public float XFlip;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (base.renderer.isVisible)
		{
			base.transform.LookAt(Camera.main.transform);
			base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x + XFlip, base.transform.rotation.eulerAngles.y + YFlip, base.transform.rotation.eulerAngles.z);
		}
	}
}
