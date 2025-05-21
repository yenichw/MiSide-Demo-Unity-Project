using UnityEngine;

public class Transform_Rotation : MonoBehaviour
{
	public bool local;

	public Vector3 speed;

	private void Update()
	{
		if (local)
		{
			base.transform.localRotation *= Quaternion.Euler(speed * Time.deltaTime);
		}
		else
		{
			base.transform.rotation *= Quaternion.Euler(speed * Time.deltaTime);
		}
	}
}
