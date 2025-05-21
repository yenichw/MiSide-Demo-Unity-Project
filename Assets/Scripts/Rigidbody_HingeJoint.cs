using UnityEngine;

public class Rigidbody_HingeJoint : MonoBehaviour
{
	private bool limitShot;

	[Header("Звук")]
	public AudioSource audioSource;

	public AudioClip soundMove;

	public AudioClip[] soundsLimit;

	private float angleJoint;

	private HingeJoint hj;

	private void Start()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		hj = GetComponent<HingeJoint>();
	}

	private void Update()
	{
		if (hj.angle > hj.limits.min + 0.1f && hj.angle < hj.limits.max - 0.1f)
		{
			limitShot = false;
			if ((double)angleJoint < (double)hj.angle - 0.1 || (double)angleJoint > (double)hj.angle + 0.1)
			{
				angleJoint = hj.angle;
				if (audioSource.clip != soundMove)
				{
					audioSource.clip = soundMove;
					audioSource.volume = 0f;
					audioSource.loop = true;
					audioSource.Play();
				}
				audioSource.volume = Mathf.Lerp(audioSource.volume, 0.5f, Time.deltaTime * 10f);
			}
			else
			{
				audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 8f);
			}
		}
		else if (!limitShot)
		{
			limitShot = true;
			angleJoint = hj.angle;
			audioSource.volume /= 2.5f;
			audioSource.loop = false;
			audioSource.clip = soundsLimit[Random.Range(0, soundsLimit.Length)];
			audioSource.pitch = Random.Range(0.95f, 1.05f);
			audioSource.Play();
		}
	}
}
