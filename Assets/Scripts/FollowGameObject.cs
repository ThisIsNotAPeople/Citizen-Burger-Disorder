using UnityEngine;

public class FollowGameObject : MonoBehaviour
{
	public GameObject follow;

	public Vector3 distance = new Vector3(0f, 0f, 0f);

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (follow != null)
		{
			base.transform.position = follow.transform.position + distance;
		}
	}
}
