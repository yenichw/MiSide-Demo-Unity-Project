using UnityEngine;

public class Audio_Pitch : MonoBehaviour
{
	public float pitch = 1f;

	public float speed = 0.1f;

	private AudioSource au;

	private void Start()
	{
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (au.pitch < pitch)
		{
			au.pitch += Time.deltaTime * speed;
			if (au.pitch > pitch)
			{
				au.pitch = pitch;
			}
		}
		if (au.pitch > pitch)
		{
			au.pitch -= Time.deltaTime * speed;
			if (au.pitch < pitch)
			{
				au.pitch = pitch;
			}
		}
	}

	public void Pitch(float x)
	{
		pitch = x;
	}

	public void Speed(float x)
	{
		speed = x;
	}
}
