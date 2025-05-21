using UnityEngine;

public class Transform_RotateOnCamera : MonoBehaviour
{
	public bool lerp = true;

	public float lerpSpeed = 10f;

	public bool reverse;

	private Transform cameraT;

	private void Start()
	{
		cameraT = GlobalTag.cameraPlayer.transform;
	}

	private void Update()
	{
		if (!reverse)
		{
			if (!lerp)
			{
				base.transform.rotation = Quaternion.LookRotation(cameraT.position - base.transform.position, Vector3.up);
			}
			else
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(cameraT.position - base.transform.position, Vector3.up), Time.deltaTime * lerpSpeed);
			}
		}
		else if (!lerp)
		{
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - cameraT.position, Vector3.up);
		}
		else
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(base.transform.position - cameraT.position, Vector3.up), Time.deltaTime * lerpSpeed);
		}
	}
}
