using UnityEngine;
using UnityEngine.Events;

public class Location2LongCorridor : MonoBehaviour
{
	public Transform corridorScale;

	public Transform doorExit;

	public UnityEvent eventNear;

	private float audioAdd;

	private AudioSource au;

	private float distanceClamp;

	private float distanceNow;

	private Transform playerPosition;

	private void Start()
	{
		playerPosition = GlobalTag.player.transform;
		corridorScale.localScale = new Vector3(26f, 1f, 1f);
		doorExit.transform.localPosition = new Vector3(63.565628f, -0.4866521f, 0f);
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (playerPosition.position.x < 0f && playerPosition.position.x > -10f)
		{
			distanceNow = Mathf.Clamp((10f - (0f - playerPosition.position.x)) * 0.1f, distanceClamp, 1f);
			if (distanceNow > distanceClamp)
			{
				distanceClamp = distanceNow;
				audioAdd = 0.2f;
				corridorScale.localScale = new Vector3(1f + (0f - playerPosition.position.x) * 2.5f * (1f - distanceNow), 1f, 1f);
				doorExit.transform.localPosition = new Vector3(4.866128f + (0f - playerPosition.position.x) * 2.5f * 2.34798f * (1f - distanceNow), -0.4866521f, 0f);
			}
		}
		if (playerPosition.position.x >= 0f)
		{
			distanceNow = 1f;
			corridorScale.localScale = new Vector3(1f, 1f, 1f);
			doorExit.transform.localPosition = new Vector3(4.866128f, -0.4866521f, 0f);
			eventNear.Invoke();
		}
		if (audioAdd > 0f)
		{
			audioAdd -= Time.deltaTime;
			if (audioAdd < 0f)
			{
				audioAdd = 0f;
			}
			if (!au.isPlaying)
			{
				au.Play();
				if (distanceNow > 0f)
				{
					au.time = au.clip.length * distanceNow;
				}
			}
			if ((double)au.volume < 0.2)
			{
				au.volume += Time.deltaTime;
				if ((double)au.volume > 0.2)
				{
					au.volume = 0.2f;
				}
			}
		}
		else
		{
			if (!(au.volume > 0f))
			{
				return;
			}
			au.volume -= Time.deltaTime;
			if (au.volume <= 0f)
			{
				au.volume = 0f;
				au.Stop();
				if (playerPosition.position.x >= 0f)
				{
					audioAdd = 0f;
					Object.Destroy(au);
					Object.Destroy(this);
				}
			}
		}
	}
}
