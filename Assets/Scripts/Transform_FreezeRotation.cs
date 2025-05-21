using UnityEngine;

public class Transform_FreezeRotation : MonoBehaviour
{
	public Vector3 rotation;

	private void Update()
	{
		base.transform.rotation = Quaternion.Euler(rotation);
	}
}
