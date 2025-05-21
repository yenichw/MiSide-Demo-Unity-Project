using System;
using UnityEngine;

[Serializable]
public class AudioPoint
{
	[HideInInspector]
	public AudioSource au;

	public AudioClip[] sounds;

	[Header("Settings")]
	public bool randomPitch = true;

	public float pitchRandomMin = 0.95f;

	public float pitchRandomMax = 1.05f;
}
