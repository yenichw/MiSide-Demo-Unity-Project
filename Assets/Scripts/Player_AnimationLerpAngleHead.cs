using UnityEngine;

public class Player_AnimationLerpAngleHead : MonoBehaviour
{
	public float speed;

	public AnimationCurve animationAngle;

	private float timeAnimation;

	private float angleNeed;

	private float angleWas;

	private PlayerMove scrPlayer;

	private void Update()
	{
		if (timeAnimation > 0f)
		{
			timeAnimation -= Time.deltaTime * speed;
			if (timeAnimation < 0f)
			{
				timeAnimation = 0f;
			}
			scrPlayer.angleHeadRotateAnimation = Mathf.Lerp(angleWas, angleNeed, animationAngle.Evaluate(1f - timeAnimation));
		}
	}

	public void Angle(float x)
	{
		scrPlayer = GlobalTag.player.GetComponent<PlayerMove>();
		angleWas = scrPlayer.angleHeadRotateAnimation;
		angleNeed = x;
		timeAnimation = 1f;
	}

	public void Speed(float x)
	{
		speed = x;
	}
}
