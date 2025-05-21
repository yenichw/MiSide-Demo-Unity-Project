using System;
using UnityEngine;

[Serializable]
public class ObjectInspectionVariant
{
	public AnimationClip animationLoop;

	public AnimationClip animationStop;

	public AnimationClip animationRechange;

	public bool rightHand;

	[Header("View pose")]
	public Vector2 rotationMin;

	public Vector2 rotationMax;

	public Vector3 rotationNow;

	[HideInInspector]
	public Vector3 rotationWas;
}
