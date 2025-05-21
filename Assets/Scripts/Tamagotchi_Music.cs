using UnityEngine;

public class Tamagotchi_Music : MonoBehaviour
{
	private bool audioMusicActive;

	[SerializeField]
	private bool effectAudioMobile;

	[Header("Звуки")]
	public AudioSource audioMusic;

	public AudioHighPassFilter audioMusicEffect;

	private float timeBlock;

	private void Update()
	{
		if (timeBlock == 0f)
		{
			if (audioMusicActive)
			{
				if (audioMusic.volume < 0.3f)
				{
					audioMusic.volume += Time.deltaTime * 0.25f;
					if (audioMusic.volume > 0.3f)
					{
						audioMusic.volume = 0.3f;
					}
				}
			}
			else if (audioMusic.volume > 0f)
			{
				audioMusic.volume -= Time.deltaTime * 0.25f;
				if (audioMusic.volume < 0f)
				{
					audioMusic.volume = 0f;
				}
			}
			if (!effectAudioMobile && audioMusicEffect.cutoffFrequency > 15f)
			{
				audioMusicEffect.cutoffFrequency -= Time.deltaTime * 1000f;
				if (audioMusicEffect.cutoffFrequency < 15f)
				{
					audioMusicEffect.cutoffFrequency = 15f;
				}
			}
		}
		if (timeBlock > 0f)
		{
			timeBlock -= Time.deltaTime;
			if (timeBlock < 0f)
			{
				timeBlock = 0f;
			}
		}
	}

	public void AudioActivation(bool x)
	{
		audioMusicActive = x;
		if (x)
		{
			audioMusic.volume = 0f;
			audioMusic.Play();
			audioMusic.time = Random.Range(0f, audioMusic.clip.length * 0.8f);
		}
		else
		{
			audioMusic.Stop();
		}
		timeBlock = 0.5f;
	}

	public void AudioVolumeZero()
	{
		audioMusicActive = false;
	}

	public void AudioEffectActive(bool x)
	{
		effectAudioMobile = x;
		if (x)
		{
			audioMusicEffect.cutoffFrequency = 2000f;
			audioMusic.volume = 0f;
		}
		timeBlock = 0.5f;
	}
}
