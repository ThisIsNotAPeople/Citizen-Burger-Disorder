using UnityEngine;

public class ShrinkWhenClose : MonoBehaviour
{
	private Vector3 startScale;

	private void Start()
	{
		startScale = base.transform.localScale;
	}

	private void Update()
	{
		if ((Camera.main.transform.position - base.transform.position).magnitude < 10f)
		{
			base.transform.localScale = startScale * ((Camera.main.transform.position - base.transform.position).magnitude / 10f);
		}
		else
		{
			base.transform.localScale = startScale;
		}
	}
}
