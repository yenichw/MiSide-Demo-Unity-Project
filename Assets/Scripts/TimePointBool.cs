// based on the original game.Yen Chezky(yenichw)
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TimePointBool
{
	public float time;

	public UnityEvent _event;

	[HideInInspector]
	public bool eventActive;
}
