using System;
using UnityEngine;

public class ObjectClock : MonoBehaviour
{
	public Transform pointerS;

	public Transform pointerM;

	public Transform pointerH;

	public AudioClip[] soundsClock;

	public float zRot = 90f;

	private AudioSource au;

	private DateTime timeNow;

	private float timeUpdate;

	private void Start()
	{
		timeNow = DateTime.Now;
		if (GetComponent<AudioSource>() != null)
		{
			au = GetComponent<AudioSource>();
		}
	}

	private void Update()
	{
		timeUpdate += Time.deltaTime;
		if (timeUpdate >= 1f)
		{
			timeNow = DateTime.Now;
			pointerS.localRotation = Quaternion.Euler(0f, (float)(-timeNow.Second) * 6f, zRot);
			pointerM.localRotation = Quaternion.Euler(0f, (float)(-timeNow.Minute) * 6f, zRot);
			pointerH.localRotation = Quaternion.Euler(0f, (float)(-timeNow.Hour) * 30f, zRot);
			timeUpdate = 0f;
			AudioPlay();
		}
	}

	public void AudioPlay()
	{
		if (au != null)
		{
			au.clip = soundsClock[UnityEngine.Random.Range(0, soundsClock.Length)];
			au.Play();
		}
	}
}
