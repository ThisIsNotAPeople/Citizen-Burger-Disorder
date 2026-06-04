using System.Collections.Generic;
using UnityEngine;

public class GraphScript : MonoBehaviour
{
	public List<NavigationScript> nodes = new List<NavigationScript>();

	public NavigationScript enterance;

	private bool updateEdges;

	private int edgeUpdateProgress = -1;

	private void Start()
	{
		int num = 0;
		foreach (Transform item in base.transform)
		{
			NavigationScript component = item.GetComponent<NavigationScript>();
			if (component != null)
			{
				nodes.Add(component);
				component.index = num;
				component.transform.FindChild("Speech").GetComponent<TextMesh>().text = num + string.Empty;
				num++;
			}
		}
		foreach (NavigationScript node in nodes)
		{
			node.CalcEdges();
		}
		enterance = GameObject.Find("Enterance").GetComponent<NavigationScript>();
	}

	private void Update()
	{
		if (updateEdges)
		{
			if (edgeUpdateProgress < nodes.Count - 1)
			{
				nodes[edgeUpdateProgress].GetComponent<NavigationScript>().CalcEdges();
				edgeUpdateProgress++;
			}
			else
			{
				updateEdges = false;
				edgeUpdateProgress = -1;
			}
		}
	}

	public void AddNode(GameObject n)
	{
		NavigationScript component = n.GetComponent<NavigationScript>();
		nodes.Add(component);
		component.index = nodes.Count - 1;
	}

	public void RecalculatePaths()
	{
		updateEdges = true;
		edgeUpdateProgress = 0;
	}

	public List<NavigationScript> FindPath(int start, int goal)
	{
		if (start >= nodes.Count)
		{
			MonoBehaviour.print("Error: no start node found for index " + start);
		}
		if (!updateEdges)
		{
			int[] array = new int[nodes.Count];
			int[] array2 = new int[nodes.Count];
			List<int> list = new List<int>();
			float[] array3 = new float[nodes.Count];
			float[] array4 = new float[nodes.Count];
			float[] array5 = new float[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				array2[i] = 0;
				array[i] = -1;
				array3[i] = -1f;
				array5[i] = -1f;
				array4[i] = Hueristic(i, goal);
			}
			list.Add(start);
			array2[start] = 1;
			array[start] = -1;
			array3[start] = 0f;
			array5[start] = array4[start];
			while (list.Count > 0)
			{
				float num = 0f;
				float num2 = array5[list[0]];
				for (int j = 1; j < list.Count; j++)
				{
					if (array5[list[j]] < num2)
					{
						num2 = array5[list[j]];
						num = j;
					}
				}
				float num3 = list[(int)num];
				if (num3 == (float)goal)
				{
					return ConstructedPath(array, goal);
				}
				array2[(int)num3] = 2;
				list.RemoveAt((int)num);
				foreach (int edge in nodes[(int)num3].edges)
				{
					if (array2[edge] != 2)
					{
						int num4 = (int)array3[(int)num3] + (int)Hueristic((int)num3, edge);
						if (array2[edge] != 1)
						{
							array2[edge] = 1;
							list.Add(edge);
							array[edge] = (int)num3;
							array3[edge] = num4;
							array5[edge] = array3[edge] + array4[edge];
						}
						else if ((float)num4 < array3[edge])
						{
							array[edge] = (int)num3;
							array3[edge] = num4;
							array5[edge] = array3[edge] + array4[edge];
						}
					}
				}
			}
		}
		return new List<NavigationScript>();
	}

	private List<NavigationScript> ConstructedPath(int[] f, int g)
	{
		List<int> list = new List<int>();
		List<NavigationScript> list2 = new List<NavigationScript>();
		for (int num = g; num != -1; num = f[num])
		{
			list.Add(num);
			list2.Add(nodes[num]);
		}
		list.Reverse();
		list2.Reverse();
		return list2;
	}

	private float Hueristic(int a, int b)
	{
		return (nodes[a].transform.position - nodes[b].transform.position).magnitude;
	}
}
