using UnityEngine;

public class Transform_PositionCamera : MonoBehaviour
{
	private Transform cameraT;

	private void Start()
	{
		cameraT = GlobalTag.cameraPlayer.transform;
	}

	private void LateUpdate()
	{
		if (cameraT != null)
		{
			base.transform.SetPositionAndRotation(cameraT.position, cameraT.rotation);
		}
	}
}
