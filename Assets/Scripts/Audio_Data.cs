using UnityEngine;

public class Audio_Data : MonoBehaviour
{
	public AudioSource audioSound;

	public AudioClip[] sounds;

	[Header("Рандом")]
	[Range(-10f, 10f)]
	public float pitchMax = 1.05f;

	[Range(-10f, 10f)]
	public float pitchMin = 0.95f;

	[Header("Старт")]
	public bool onStartRandomPitch;

	public bool onStartRandom;

	public bool onEnableRandomPitch;

	public bool onEnableRandom;

	private float timeOutNow;

	[Header("Настройки")]
	public float timeOut;

	private int lastSound;

	private void Start()
	{
		if (GetComponent<AudioSource>() != null)
		{
			audioSound = GetComponent<AudioSource>();
		}
		if (onStartRandomPitch)
		{
			RandomPlayPitch();
		}
		if (onStartRandom)
		{
			RandomPlay();
		}
	}

	private void Update()
	{
		if (timeOutNow > 0f)
		{
			timeOutNow -= Time.deltaTime;
			if (timeOutNow < 0f)
			{
				timeOutNow = 0f;
			}
		}
	}

	private void OnEnable()
	{
		if (onEnableRandomPitch)
		{
			RandomPlayPitch();
		}
		if (onEnableRandom)
		{
			RandomPlay();
		}
	}

	public void SoundPlay(int x)
	{
		audioSound.clip = sounds[x];
		audioSound.Play();
	}

	public void RandomPlay()
	{
		int num = FixRandomSound();
		audioSound.pitch = 1f;
		audioSound.clip = sounds[num];
		audioSound.Play();
	}

	public void RandomPlayPitch()
	{
		if (timeOutNow == 0f)
		{
			int num = FixRandomSound();
			audioSound.pitch = Random.Range(pitchMin, pitchMax);
			audioSound.clip = sounds[num];
			audioSound.Play();
			timeOutNow = timeOut;
		}
	}

	public void SoundPlayPitch(int x)
	{
		audioSound.pitch = Random.Range(pitchMin, pitchMax);
		audioSound.clip = sounds[x];
		audioSound.Play();
	}

	public void Play()
	{
		audioSound.pitch = Random.Range(pitchMin, pitchMax);
		audioSound.Play();
	}

	private int FixRandomSound()
	{
		int num = Random.Range(0, sounds.Length);
		if (sounds.Length > 1 && num == lastSound)
		{
			num++;
			if (num > sounds.Length - 1)
			{
				num = 0;
			}
		}
		lastSound = num;
		return num;
	}
}
