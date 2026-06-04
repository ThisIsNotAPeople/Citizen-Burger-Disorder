using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class MouseOrbit : MonoBehaviour
{
	public Transform target;

	public float distance;

	public float xSpeed;

	public float ySpeed;

	public int sensitivity;

	public int maxSensitivity;

	public int minSensitivity;

	public int yMinLimit;

	public int yMaxLimit;

	private float x;

	private float y;

	public MouseOrbit()
	{
		distance = 10f;
		xSpeed = 250f;
		ySpeed = 120f;
		sensitivity = 10;
		maxSensitivity = 30;
		minSensitivity = 5;
		yMinLimit = -20;
		yMaxLimit = 80;
	}

	public virtual void Start()
	{
		enabled = false;
		Vector3 eulerAngles = transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		if ((bool)rigidbody)
		{
			rigidbody.freezeRotation = true;
		}
	}

	public virtual void LateUpdate()
	{
		if ((bool)target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, 0f);
			Vector3 to = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
			transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, (float)sensitivity * Time.deltaTime);
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = Vector3.Lerp(transform.position, to, (float)sensitivity * Time.deltaTime);
			Vector3 vector2 = default(Vector3);
			if (Physics.Raycast(vector, target.position - vector, out hitInfo, (target.position - vector).magnitude))
			{
				Debug.Log("Hit! " + Time.time);
				vector2 = (hitInfo.point - target.position) * 0.98f + target.position;
			}
			else
			{
				vector2 = vector;
			}
			transform.position = vector2;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (!(angle >= -360f))
		{
			angle += 360f;
		}
		if (!(angle <= 360f))
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public virtual void Main()
	{
	}
}
