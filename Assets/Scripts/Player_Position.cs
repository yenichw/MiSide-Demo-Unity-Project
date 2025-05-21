using UnityEngine;

public class Player_Position : MonoBehaviour
{
	public AnimationCurve animationMove;

	public float speed = 1f;

	public bool finishActive;

	public bool rotation;

	public Transform playerTarget;

	private float timeAnimationMove;

	private bool play;

	private Vector3 positionPlayerWas;

	private Quaternion rotationPlayerWas;

	private void Update()
	{
		if (play)
		{
			timeAnimationMove += Time.deltaTime * speed;
			if (timeAnimationMove > 1f)
			{
				timeAnimationMove = 1f;
				play = false;
			}
			playerTarget.position = Vector3.Lerp(positionPlayerWas, base.transform.position, timeAnimationMove);
			if (rotation)
			{
				playerTarget.rotation = Quaternion.Lerp(rotationPlayerWas, base.transform.rotation, timeAnimationMove);
			}
			if (timeAnimationMove == 1f && finishActive)
			{
				Stop();
			}
		}
	}

	public void Play()
	{
		if (playerTarget == null)
		{
			playerTarget = GlobalTag.player.transform;
		}
		play = true;
		positionPlayerWas = playerTarget.position;
		rotationPlayerWas = playerTarget.rotation;
		if (playerTarget == GlobalTag.player.transform)
		{
			playerTarget.GetComponent<PlayerMove>().dontMove = true;
		}
	}

	public void Stop()
	{
		play = false;
		if (playerTarget == GlobalTag.player.transform)
		{
			playerTarget.GetComponent<PlayerMove>().dontMove = false;
		}
	}
}
