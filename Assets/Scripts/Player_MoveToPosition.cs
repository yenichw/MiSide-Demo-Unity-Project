using UnityEngine;

public class Player_MoveToPosition : MonoBehaviour
{
	public AnimationCurve animationMove;

	private float timeAnimation;

	private Vector3 positionWas;

	public float speed = 1f;

	private Transform player;

	private bool play;

	private void Update()
	{
		if (play && timeAnimation < 1f)
		{
			timeAnimation += Time.deltaTime * speed;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			player.transform.position = Vector3.Lerp(positionWas, base.transform.position, animationMove.Evaluate(timeAnimation));
		}
	}

	public void Play()
	{
		play = true;
		player = GlobalTag.player.transform;
		positionWas = player.position;
		GlobalTag.player.GetComponent<PlayerMove>().dontMove = true;
		timeAnimation = 0f;
	}

	public void Stop()
	{
		play = false;
		GlobalTag.player.GetComponent<PlayerMove>().dontMove = false;
	}
}
