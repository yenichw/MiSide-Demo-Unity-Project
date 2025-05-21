using UnityEngine;

[AddComponentMenu("Functions/Transform/Transform move points")]
public class Transform_MovePoints : MonoBehaviour
{
	public Vector3[] points = new Vector3[2];

	public float speed = 1f;

	private int indexPoint;

	private void Update()
	{
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, points[indexPoint], Time.deltaTime * speed);
		if (Vector3.Distance(base.transform.localPosition, points[indexPoint]) <= Time.deltaTime * speed)
		{
			Next();
		}
	}

	private void Next()
	{
		indexPoint++;
		if (indexPoint > points.Length - 1)
		{
			indexPoint = 0;
		}
	}
}
