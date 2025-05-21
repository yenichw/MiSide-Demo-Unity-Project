using UnityEngine;

public class ObjectInteractiveReqIK : MonoBehaviour
{
	public Transform transformRotation;

	public int rotate;

	public float dotLimit = 0.7f;

	public Vector3 GetDirection()
	{
		Vector3 result = Vector3.zero;
		if (rotate == 0)
		{
			result = transformRotation.forward;
		}
		if (rotate == 1)
		{
			result = -transformRotation.forward;
		}
		if (rotate == 2)
		{
			result = transformRotation.right;
		}
		if (rotate == 3)
		{
			result = -transformRotation.right;
		}
		if (rotate == 4)
		{
			result = transformRotation.up;
		}
		if (rotate == 5)
		{
			result = -transformRotation.up;
		}
		return result;
	}
}
