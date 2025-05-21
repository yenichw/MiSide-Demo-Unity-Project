using UnityEngine;

public class Sound_Several : MonoBehaviour
{
	public AudioSource[] soundsSource;

	public float[] soundsVolume;

	[Space(10f)]
	public float timeMinNext = 5f;

	public float timeMaxNext = 25f;

	public float speedVolume = 1f;

	private int soundIndexWork;

	private float timeNext;

	private float timeNextNeed;

	private void Start()
	{
		for (int i = 0; i < soundsSource.Length; i++)
		{
			soundsSource[i].volume = 0f;
		}
		NextSound();
	}

	private void Update()
	{
		timeNext += Time.deltaTime;
		if (timeNext >= timeNextNeed)
		{
			NextSound();
		}
		for (int i = 0; i < soundsSource.Length; i++)
		{
			if (i != soundIndexWork)
			{
				if (soundsSource[i].volume > 0f)
				{
					soundsSource[i].volume -= Time.deltaTime * speedVolume;
					if (soundsSource[i].volume < 0f)
					{
						soundsSource[i].volume = 0f;
					}
				}
			}
			else if (soundsSource[i].volume < soundsVolume[i])
			{
				soundsSource[i].volume += Time.deltaTime * speedVolume;
				if (soundsSource[i].volume > soundsVolume[i])
				{
					soundsSource[i].volume = soundsVolume[i];
				}
			}
		}
	}

	private void NextSound()
	{
		timeNext = 0f;
		timeNextNeed = Random.Range(timeMinNext, timeMaxNext);
		soundIndexWork = Random.Range(0, soundsSource.Length);
	}
}
