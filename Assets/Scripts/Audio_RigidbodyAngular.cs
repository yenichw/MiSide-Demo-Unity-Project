using UnityEngine;

public class Audio_RigidbodyAngular : MonoBehaviour
{
	public float intensity = 1f;

	[Range(0f, 1f)]
	public float min = 0.1f;

	[Header("Pitch")]
	public bool pitchRandomLowVolume;

	public float pitchMin = 1.1f;

	public float pitchMax = 0.9f;

	[Header("Speed")]
	public bool speedVelocity;

	private Rigidbody rb;

	private AudioSource au;

	private float velocityAngular;

	private float velocitySpeed;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		au = GetComponent<AudioSource>();
		if (pitchRandomLowVolume)
		{
			au.pitch = Random.Range(pitchMin, pitchMax);
		}
	}

	private void Update()
	{
		velocityAngular = Mathf.Lerp(velocityAngular, Vector3.Distance(rb.angularVelocity, Vector3.zero) * intensity, Time.deltaTime * 5f);
		if (speedVelocity)
		{
			velocitySpeed = Mathf.Lerp(velocitySpeed, Vector3.Distance(rb.velocity, Vector3.zero) * intensity, Time.deltaTime * 5f);
		}
		if (Vector3.Distance(rb.angularVelocity, Vector3.zero) * intensity > min)
		{
			if (!speedVelocity)
			{
				au.volume = Mathf.Lerp(velocityAngular, Vector3.Distance(rb.angularVelocity, Vector3.zero), Time.deltaTime * 3f);
			}
			else
			{
				au.volume = Mathf.Lerp(velocityAngular + velocitySpeed, Vector3.Distance(rb.angularVelocity, Vector3.zero) + Vector3.Distance(rb.velocity, Vector3.zero), Time.deltaTime * 3f);
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
				if (pitchRandomLowVolume)
				{
					au.pitch = Random.Range(pitchMin, pitchMax);
				}
			}
		}
	}

	public void SoundAdd(float x)
	{
		velocityAngular += x;
	}
}
