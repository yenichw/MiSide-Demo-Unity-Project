using System.Collections.Generic;
using UnityEngine;

public class Location11_LampMove : MonoBehaviour
{
	public Vector2[] points;

	[HideInInspector]
	public List<Vector3> pointsMove;

	private void Update()
	{
		if (pointsMove != null && pointsMove.Count > 0)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, pointsMove[0], Time.deltaTime * 5f);
			if ((double)Vector3.Distance(base.transform.localPosition, pointsMove[0]) < 0.02)
			{
				base.transform.localPosition = pointsMove[0];
				pointsMove.RemoveAt(0);
			}
		}
	}

	public void ClearPoints()
	{
		if (pointsMove != null && pointsMove.Count > 0)
		{
			pointsMove.Clear();
		}
	}

	public void AddPoint(int x)
	{
		pointsMove.Add(new Vector3(points[x].x, points[x].y, base.transform.localPosition.z));
	}
}
