using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CheatInput
{
	public string inputText;

	public UnityEvent eventCheat;

	public bool oneTime;

	[HideInInspector]
	public bool oneTimeReady;
}
