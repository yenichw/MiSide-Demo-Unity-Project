using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[AddComponentMenu("Functions/Animation/Animation Player")]
public class ObjectAnimationPlayer : MonoBehaviour
{
	public AnimationClip animationStart;

	public AnimationClip animationLoop;

	public AnimationClip animationStop;

	public PlayableDirector playableDirector;

	public bool stopPositionHips;

	[Header("Settings")]
	public bool keepItem;

	[Range(0f, 90f)]
	public float angleHeadRotate = 30f;

	[Header("Events")]
	public List<UnityEvent> eventsPlayer;

	public UnityEvent eventStartLoop;

	[HideInInspector]
	public bool firstEventFinishReady;

	public bool firstEventFinish;

	public UnityEvent eventFinish;

	private PlayerMove scrpm;

	private bool fs;

	private void Start()
	{
		if (!fs)
		{
			fs = true;
			scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		}
	}

	public void AnimationPlayFast()
	{
		if (!fs)
		{
			Start();
		}
		scrpm.animationFast = true;
		AnimationPlay();
	}

	[ContextMenu("Запустить")]
	public void AnimationPlay()
	{
		if (!fs)
		{
			Start();
		}
		firstEventFinishReady = false;
		scrpm.AnimationPlay(animationStart, animationLoop, animationStop, base.transform, eventStartLoop, eventsPlayer, keepItem, this);
		scrpm.angleHeadRotateAnimation = angleHeadRotate;
		if (GetComponent<ObjectAnimationPlayerHold>() != null)
		{
			GetComponent<ObjectAnimationPlayerHold>().Restart();
		}
		if (playableDirector != null)
		{
			playableDirector.Play();
			ConsoleMain.ConsolePrintGame("ObjectAnimationPlayer.AnimationPlay.playableDirector.Play()");
		}
	}

	public void AnimationStop()
	{
		if (!fs)
		{
			Start();
		}
		scrpm.AnimationPlayStop();
	}
}
