using UnityEngine;

public class Audio_Hold : MonoBehaviour
{
	public float speedLerp = 3f;

	public float volume;

	[Header("Settings")]
	public bool holdActive;

	public AudioSource audioSource;

	public bool dontStop;

	private int hold;

	private void Start()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
	}

	private void Update()
	{
		if (hold > 0 || holdActive)
		{
			if (hold > 0)
			{
				hold--;
			}
			if (audioSource.volume != volume)
			{
				audioSource.volume = Mathf.Lerp(audioSource.volume, volume, speedLerp * Time.deltaTime);
			}
		}
		else
		{
			if (audioSource.volume > 0f)
			{
				audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, speedLerp * Time.deltaTime);
			}
			if (audioSource.isPlaying && (double)audioSource.volume < 0.01)
			{
				audioSource.Pause();
			}
		}
	}

	public void Hold()
	{
		hold = 2;
	}

	public void HoldActivation(bool x)
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		holdActive = x;
		if (!audioSource.isPlaying)
		{
			audioSource.Play();
		}
	}

	public void HoldActivationNet(bool x)
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		holdActive = x;
		if (!audioSource.isPlaying)
		{
			audioSource.Play();
		}
	}

	public void Volume(float x)
	{
		volume = x;
	}
}
