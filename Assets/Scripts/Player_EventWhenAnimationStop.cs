using UnityEngine;
using UnityEngine.Events;

public class Player_EventWhenAnimationStop : MonoBehaviour
{
	private PlayerMove scrpm;

	public bool play;

	public UnityEvent eventsAnimationPlayerStop;

	private void Start()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
	}

	private void Update()
	{
		if (play && !scrpm.animationRun)
		{
			eventsAnimationPlayerStop.Invoke();
			Object.Destroy(this);
		}
	}

	public void Play()
	{
		base.gameObject.SetActive(value: true);
		play = true;
	}
}
