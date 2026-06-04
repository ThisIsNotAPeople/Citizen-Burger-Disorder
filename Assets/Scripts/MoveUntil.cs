using UnityEngine;

public class MoveUntil : MonoBehaviour
{
	public Vector3 movePos = Vector3.zero;

	public float moveTime;

	public bool stopAtCollision;

	public bool pushForward;

	public bool wasCollision;

	private Vector3 moveToPos;

	private Vector3 rotateToPos;

	private float start;

	private float duration = 0.7f;

	private void Start()
	{
		moveToPos = base.transform.position + movePos;
	}

	private void Update()
	{
		if (start == 0f)
		{
			if ((base.transform.position - moveToPos).magnitude >= 0.5f && (!stopAtCollision || !wasCollision))
			{
				base.transform.position = Vector3.Lerp(base.transform.position, moveToPos, moveTime * Time.deltaTime);
			}
			else if (stopAtCollision && wasCollision)
			{
				moveToPos = base.transform.position;
			}
		}
		if ((base.transform.position - moveToPos).magnitude < 0.5f || (pushForward && start != 0f))
		{
			if (pushForward && start == 0f)
			{
				start = Time.time;
			}
			if (pushForward && Time.time < start + duration)
			{
				base.transform.RotateAround(base.transform.position + base.transform.up, base.transform.right, 60f * Time.deltaTime);
			}
			else
			{
				if (base.rigidbody != null)
				{
					base.rigidbody.isKinematic = false;
				}
				base.enabled = false;
				MonoBehaviour.print("disabled 1");
			}
		}
		if (wasCollision && !pushForward)
		{
			base.enabled = false;
			MonoBehaviour.print("disabled due to collision");
		}
	}
}
