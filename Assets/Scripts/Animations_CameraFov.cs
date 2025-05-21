using UnityEngine;

public class Animations_CameraFov : MonoBehaviour
{
	public AnimationCurve animationFov;

	public float speed = 1f;

	public float fieldOfView;

	public float fieldOfViewStart;

	private Camera cameraPlayer;

	private bool play;

	private float timePlay;

	private float fovLast;

	private void Update()
	{
		if (play && timePlay < 1f)
		{
			timePlay += Time.deltaTime * speed;
			if (timePlay >= 1f)
			{
				timePlay = 1f;
				play = false;
			}
			cameraPlayer.fieldOfView = Mathf.Lerp(fovLast, fieldOfView, animationFov.Evaluate(timePlay));
			cameraPlayer.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
		}
	}

	public void PlayAnimation()
	{
		play = true;
		timePlay = 0f;
		cameraPlayer = GlobalTag.cameraPlayer.GetComponent<Camera>();
		if (fieldOfViewStart > 0f)
		{
			fovLast = fieldOfViewStart;
		}
		else
		{
			fovLast = cameraPlayer.fieldOfView;
		}
	}
}
