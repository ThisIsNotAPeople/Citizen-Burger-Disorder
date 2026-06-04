using System.Collections;
using UnityEngine;

public class StaticCoroutine : MonoBehaviour
{
	private static StaticCoroutine mInstance;

	private static StaticCoroutine instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Object.FindObjectOfType(typeof(StaticCoroutine)) as StaticCoroutine;
				if (mInstance == null)
				{
					mInstance = new GameObject("StaticCoroutine").AddComponent<StaticCoroutine>();
				}
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	private IEnumerator Perform(IEnumerator coroutine)
	{
		yield return StartCoroutine(coroutine);
		Die();
	}

	public static void DoCoroutine(IEnumerator coroutine)
	{
		instance.StartCoroutine(instance.Perform(coroutine));
	}

	private void Die()
	{
		mInstance = null;
		Object.Destroy(base.gameObject);
	}

	private void OnApplicationQuit()
	{
		mInstance = null;
	}
}
