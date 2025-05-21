using UnityEngine;

public class Audio_Volume : MonoBehaviour
{
	public bool active = true;

	public float volume = 0.5f;

	public float speed = 5f;

	public bool useUnscaled;

	public bool startZeroVolume;

	public bool onDestroyComponent;

	public bool onPlayRandomTime;

	private AudioSource au;

	private bool destroyMe;

	private void Start()
	{
		au = GetComponent<AudioSource>();
		if (startZeroVolume)
		{
			au.volume = 0f;
		}
	}

	private void Update()
	{
		if (active)
		{
			if (au.volume < volume)
			{
				if (!useUnscaled)
				{
					au.volume += Time.deltaTime * speed;
				}
				else
				{
					au.volume += Time.unscaledDeltaTime * speed;
				}
				if (au.volume > volume)
				{
					au.volume = volume;
				}
			}
			if (au.volume > volume)
			{
				if (!useUnscaled)
				{
					au.volume -= Time.deltaTime * speed;
				}
				else
				{
					au.volume -= Time.unscaledDeltaTime * speed;
				}
				if (au.volume < volume)
				{
					au.volume = volume;
				}
			}
		}
		else
		{
			if (!(au.volume > 0f))
			{
				return;
			}
			if (!useUnscaled)
			{
				au.volume -= Time.deltaTime * speed;
			}
			else
			{
				au.volume -= Time.unscaledDeltaTime * speed;
			}
			if (!(au.volume <= 0f))
			{
				return;
			}
			au.volume = 0f;
			au.Stop();
			if (destroyMe)
			{
				if (!onDestroyComponent)
				{
					Object.Destroy(base.gameObject);
					return;
				}
				Object.Destroy(this);
				Object.Destroy(au);
			}
		}
	}

	public void Volume(float x)
	{
		volume = x;
	}

	public void Play()
	{
		base.enabled = true;
		base.gameObject.SetActive(value: true);
		GetComponent<AudioSource>().Play();
		active = true;
		if (onPlayRandomTime)
		{
			GetComponent<AudioSource>().time = Random.Range(0f, GetComponent<AudioSource>().clip.length);
		}
	}

	public void Activation(bool x)
	{
		active = x;
	}

	public void DestroySmooth()
	{
		active = false;
		destroyMe = true;
		volume = 0f;
	}

	public void Speed(float x)
	{
		speed = x;
	}
}
