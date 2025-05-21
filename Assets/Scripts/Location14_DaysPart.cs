using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Location14_DaysPart
{
	public AnimationClip animationPlayer;

	public UnityEvent eventEnd;

	public ObjectAnimationPlayer animationPlayerObject;

	public Animator[] otherAnimations;

	public string[] otherAnimationsStatePlay;

	public UnityEvent eventAfter3Day;

	public AnimationClip animationPlayer3Day;
}
