using System.Collections.Generic;
using UnityEngine;

public class NavigationScript : MonoBehaviour
{
	public int index;

	public List<int> edges;

	private float maxEdgeLength = 50f;

	private GraphScript graph;

	public void CalcEdges()
	{
		GraphScript component = base.transform.parent.GetComponent<GraphScript>();
		edges = new List<int>();
		int layerMask = -12545;
		foreach (NavigationScript node in component.nodes)
		{
			if (component.nodes.IndexOf(node) != component.nodes.IndexOf(this))
			{
				float magnitude = (node.transform.position - base.transform.position).magnitude;
				RaycastHit hitInfo;
				if (magnitude < maxEdgeLength && !Physics.SphereCast(base.transform.position, 0.6f, (node.transform.position - base.transform.position).normalized, out hitInfo, (node.transform.position - base.transform.position).magnitude, layerMask))
				{
					edges.Add(component.nodes.IndexOf(node));
				}
			}
		}
	}
}
