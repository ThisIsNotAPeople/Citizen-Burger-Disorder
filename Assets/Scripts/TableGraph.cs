using System.Collections.Generic;
using UnityEngine;

public class TableGraph : MonoBehaviour
{
	public class TableGroup
	{
		public int capacity;

		public List<Table> tableMats;

		public List<TableNodes> tableNodes;
	}

	public List<TableNodes> nodes = new List<TableNodes>();

	public static TableGroup[] tables;

	private void Start()
	{
		int num = 0;
		foreach (Transform item in base.transform)
		{
			TableNodes component = item.GetComponent<TableNodes>();
			nodes.Add(component);
			if (num < component.table.tableNumber)
			{
				num = component.table.tableNumber;
			}
		}
		tables = new TableGroup[num];
		for (int i = 0; i < tables.Length; i++)
		{
			tables[i] = new TableGroup();
			tables[i].capacity = 0;
			tables[i].tableMats = new List<Table>();
			tables[i].tableNodes = new List<TableNodes>();
		}
		foreach (TableNodes node in nodes)
		{
			int num2 = node.table.tableNumber - 1;
			tables[num2].tableNodes.Add(node);
			tables[num2].tableMats.Add(node.table);
			tables[num2].capacity++;
		}
	}

	private int GetCapacityOfTable(int tableNumber)
	{
		return tables[tableNumber - 1].capacity;
	}

	public static int FindUnoccupiedTableForGroup(int groupSize)
	{
		if (tables != null)
		{
			for (int i = 0; i < tables.Length; i++)
			{
				MonoBehaviour.print("Table " + (i + 1) + " has a capacity of: " + tables[i].capacity);
				if (tables[i].capacity < groupSize)
				{
					continue;
				}
				MonoBehaviour.print("The group of " + groupSize + " can fit here");
				bool flag = false;
				foreach (TableNodes tableNode in tables[i].tableNodes)
				{
					if (tableNode.occupied)
					{
						MonoBehaviour.print("... but the table is already occupied.");
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return i + 1;
				}
			}
		}
		return -1;
	}
}
