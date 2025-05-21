using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Location14_AnimationCursor
{
	public Transform targetMouse;

	public Vector3 offset;

	public float timeWait;

	public UnityEvent eventClick;
}
