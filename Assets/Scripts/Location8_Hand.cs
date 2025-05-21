using Colorful;
using RootMotion.FinalIK;
using UnityEngine;

public class Location8_Hand : MonoBehaviour
{
	private Transform playerHead;

	private float timeStart;

	private bool nearTrigger;

	private float timeNoiseHead;

	private AudioSource au;

	public Transform targetLimb;

	public LimbIK ikHand;

	public AudioClip[] soundsKick;

	private void Start()
	{
		playerHead = GlobalTag.cameraPlayer.transform;
		ikHand.solver.target = targetLimb;
		timeStart = 1.5f;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (timeStart == 0f)
		{
			targetLimb.position = Vector3.Lerp(targetLimb.position, playerHead.position, Time.deltaTime * 2f);
			targetLimb.localPosition = Vector3.Normalize(targetLimb.localPosition) * 4f;
			if ((double)ikHand.solver.IKPositionWeight < 0.7)
			{
				ikHand.solver.IKPositionWeight += Time.deltaTime * 0.1f;
				if ((double)ikHand.solver.IKPositionWeight > 0.7)
				{
					ikHand.solver.IKPositionWeight = 0.7f;
				}
			}
			if (nearTrigger)
			{
				timeNoiseHead += Time.deltaTime;
				if ((double)timeNoiseHead > 0.5)
				{
					AudioPlay();
					timeNoiseHead = 0f;
					GlobalTag.player.GetComponent<PlayerMove>().CameraNoise(1f);
					GlobalTag.player.GetComponent<PlayerMove>().SlowTimeMove(1.5f);
				}
				if ((double)Vector3.Distance(base.transform.position, playerHead.position) > 0.8)
				{
					nearTrigger = false;
					GlobalTag.cameraPlayer.GetComponent<Glitch>().enabled = false;
					GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectNoise(0f);
				}
			}
			else if ((double)Vector3.Distance(base.transform.position, playerHead.position) < 0.8)
			{
				nearTrigger = true;
				GlobalTag.cameraPlayer.GetComponent<Glitch>().enabled = true;
				GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectNoise(0.5f);
				GlobalTag.player.GetComponent<PlayerMove>().CameraNoise(3f);
				GlobalTag.player.GetComponent<PlayerMove>().SlowTimeMove(1.5f);
				AudioPlay();
			}
		}
		else
		{
			timeStart -= Time.deltaTime;
			if (timeStart < 0f)
			{
				timeStart = 0f;
			}
		}
	}

	private void AudioPlay()
	{
		au.clip = soundsKick[Random.Range(0, soundsKick.Length)];
		au.pitch = Random.Range(0.95f, 1.05f);
		au.Play();
	}
}
