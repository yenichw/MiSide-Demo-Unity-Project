using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EventDistance
{
	public float distance;

	public UnityEvent eventLess;

	public UnityEvent eventMore;

	[HideInInspector]
	public int position;
}
