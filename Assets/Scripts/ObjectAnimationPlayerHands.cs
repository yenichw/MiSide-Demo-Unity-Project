using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectAnimationPlayerHands : MonoBehaviour
{
	public AnimationClip animationStart;

	public AnimationClip animationLoop;

	public AnimationClip animationStop;

	public UnityEvent eventStartLoop;

	public List<UnityEvent> eventsPlayer;

	public bool face;

	public bool keepItem;

	private PlayerMove scrpm;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
	}

	public void AnimationPlay()
	{
		scrpm.AnimationHandPlay(animationStart, animationLoop, animationStop, eventStartLoop, eventsPlayer, face, keepItem);
	}

	public void AnimationStop()
	{
		scrpm.AnimationHandPlayStop();
	}
}
