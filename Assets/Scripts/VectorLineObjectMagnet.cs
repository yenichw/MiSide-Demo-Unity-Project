using UnityEngine;

public class VectorLineObjectMagnet : MonoBehaviour
{
	[Range(1f, 120f)]
	public int frameUpdate = 15;

	private int fu;

	public float distance = 1f;

	public VectorLineObjectMagnetPoint[] lines;

	[Header("Objects")]
	public GameObject objectMove;

	public GameObject objectTarget;

	[Header("Settings")]
	public Vector3 generalPoint;

	public float generalDistance;

	private void Start()
	{
		UpdateObject();
	}

	private void Update()
	{
		if (Vector3.Distance(objectTarget.transform.position, generalPoint) < generalDistance)
		{
			fu--;
			if (fu < 0)
			{
				UpdateObject();
			}
		}
	}

	private void UpdateObject()
	{
		fu = frameUpdate;
		float num = 1000f;
		int num2 = 0;
		int num3 = 0;
		if (lines != null && lines.Length != 0)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				float num4 = Vector3.Distance(lines[i].positionStart, lines[i].positionEnd) / (float)lines[i].countPoints;
				for (int j = 0; j < lines[i].countPoints; j++)
				{
					if (num > Vector3.Distance(Vector3.MoveTowards(lines[i].positionStart, lines[i].positionEnd, num4 * (float)j), objectTarget.transform.position))
					{
						num = Vector3.Distance(Vector3.MoveTowards(lines[i].positionStart, lines[i].positionEnd, num4 * (float)j), objectTarget.transform.position);
						num2 = i;
						num3 = j;
					}
				}
			}
		}
		objectMove.transform.position = Vector3.MoveTowards(lines[num2].positionStart, lines[num2].positionEnd, Vector3.Distance(lines[num2].positionStart, lines[num2].positionEnd) / (float)lines[num2].countPoints * (float)num3);
		objectMove.transform.rotation = Quaternion.Euler(lines[num2].rotationObject);
	}
}
