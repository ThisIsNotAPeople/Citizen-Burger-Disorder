using UnityEngine;

public class TableNodes : MonoBehaviour
{
	public Table table;

	public bool occupied;

	public float occupiedStartTime;

	private void Awake()
	{
		if (!(table == null))
		{
			return;
		}
		float num = 99999f;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Table");
		foreach (GameObject gameObject in array)
		{
			if ((base.transform.position - gameObject.transform.position).magnitude < num)
			{
				num = (base.transform.position - gameObject.transform.position).magnitude;
				GameObject gameObject2 = gameObject;
				table = gameObject2.GetComponent<Table>();
			}
		}
		table.myTableNode = this;
	}

	[RPC]
	private void SetOccupied(bool occ)
	{
		occupied = occ;
		occupiedStartTime = Time.time;
	}
}
