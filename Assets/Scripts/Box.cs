using UnityEngine;

public class Box : MonoBehaviour
{
	public enum BoxContents
	{
		PattMcRat = 0,
		SeedyCedric = 1,
		GreenGrace = 2
	}

	private GameObject boxOpenedPrefab;

	public BoxContents contents;

	public GameObject pattyPre;

	public GameObject baconPre;

	public GameObject bunBotPre;

	public GameObject bunTopPre;

	public GameObject cheesePre;

	public GameObject lettucePre;

	public GameObject TomatoPre;

	private void Awake()
	{
		boxOpenedPrefab = Resources.Load("Prefabs/Misc/BoxOpened") as GameObject;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Network.isServer && collision.gameObject.name.Equals("hand"))
		{
			OpenBox();
		}
	}

	private Vector3 randomSpawnPos()
	{
		return base.transform.position + Random.insideUnitSphere * 1f;
	}

	[RPC]
	private void SyncContents(NetworkViewID boxID, int contentsID)
	{
		Box component = NetworkView.Find(boxID).transform.GetComponent<Box>();
		switch (contentsID)
		{
		case 0:
			component.contents = BoxContents.PattMcRat;
			break;
		case 1:
			component.contents = BoxContents.SeedyCedric;
			break;
		case 2:
			component.contents = BoxContents.GreenGrace;
			break;
		default:
			component.contents = BoxContents.PattMcRat;
			break;
		}
		MonoBehaviour.print("Setting box contents to " + contentsID + ", " + component.contents);
	}

	private void OpenBox()
	{
		Network.Instantiate(boxOpenedPrefab, base.transform.position, base.transform.rotation, 1);
		switch (contents)
		{
		case BoxContents.PattMcRat:
		{
			for (int l = 0; l < Random.Range(4, 7); l++)
			{
				Network.Instantiate(pattyPre, randomSpawnPos(), base.transform.rotation, 1);
			}
			for (int m = 0; m < Random.Range(4, 7); m++)
			{
				Network.Instantiate(baconPre, randomSpawnPos(), base.transform.rotation, 1);
			}
			break;
		}
		case BoxContents.SeedyCedric:
		{
			for (int n = 0; n < Random.Range(4, 7); n++)
			{
				Network.Instantiate(bunTopPre, randomSpawnPos(), base.transform.rotation, 1);
				Network.Instantiate(bunBotPre, randomSpawnPos(), base.transform.rotation, 1);
			}
			break;
		}
		case BoxContents.GreenGrace:
		{
			for (int i = 0; i < Random.Range(2, 5); i++)
			{
				Network.Instantiate(lettucePre, randomSpawnPos(), base.transform.rotation, 1);
			}
			for (int j = 0; j < Random.Range(4, 7); j++)
			{
				Network.Instantiate(cheesePre, randomSpawnPos(), base.transform.rotation, 1);
			}
			for (int k = 0; k < Random.Range(2, 5); k++)
			{
				Network.Instantiate(TomatoPre, randomSpawnPos(), base.transform.rotation, 1);
			}
			break;
		}
		}
		GetComponent<PickupObject>().DestroyObject();
	}

	private void Update()
	{
	}
}
