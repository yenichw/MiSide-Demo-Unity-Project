using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TimePoint
{
	public float time;

	public AnimationClip timeAnimationClip;

	public UnityEvent _event;
}
