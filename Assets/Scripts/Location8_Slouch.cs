using UnityEngine;

public class Location8_Slouch : MonoBehaviour
{
	public Location8_SlouchLife slouchLife;

	private float timeEyeLook;

	private Vector2 eyesRotations;

	[Header("Глаза")]
	public Transform eyeL;

	public Transform eyeR;

	private Transform playerHeadT;

	[Header("Звук")]
	public LayerMask layerWall;

	public AudioSource audioLife;

	public AudioSource audioStep;

	private void Start()
	{
		playerHeadT = GlobalTag.cameraPlayer.transform;
		EyesLook();
	}

	private void Update()
	{
		timeEyeLook -= Time.deltaTime;
		if (timeEyeLook < 0f)
		{
			EyesLook();
		}
		eyeL.localRotation = Quaternion.Lerp(eyeL.localRotation, Quaternion.Euler(eyesRotations.x, 0f, eyesRotations.y), Time.deltaTime * 25f);
		eyeR.localRotation = Quaternion.Lerp(eyeR.localRotation, Quaternion.Euler(eyesRotations.x, 0f, eyesRotations.y), Time.deltaTime * 25f);
		if (audioLife.volume < 1f)
		{
			audioLife.volume += Time.deltaTime;
			if (audioLife.volume > 1f)
			{
				audioLife.volume = 1f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!Physics.Linecast(audioLife.transform.position, playerHeadT.position, layerWall))
		{
			audioLife.GetComponent<AudioLowPassFilter>().cutoffFrequency = Mathf.Lerp(audioLife.GetComponent<AudioLowPassFilter>().cutoffFrequency, 5007.7f, Time.fixedDeltaTime * 5f);
		}
		else
		{
			audioLife.GetComponent<AudioLowPassFilter>().cutoffFrequency = Mathf.Lerp(audioLife.GetComponent<AudioLowPassFilter>().cutoffFrequency, 10f, Time.fixedDeltaTime * 5f);
		}
		audioStep.GetComponent<AudioLowPassFilter>().cutoffFrequency = audioLife.GetComponent<AudioLowPassFilter>().cutoffFrequency;
	}

	private void EyesLook()
	{
		timeEyeLook = Random.Range(0.1f, 1.75f);
		eyesRotations = new Vector2(Random.Range(-15f, 15f), Random.Range(-15f, 15f));
	}

	public void StepFoot()
	{
		audioStep.pitch = Random.Range(0.9f, 1.1f);
		audioStep.Play();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == GlobalTag.player)
		{
			slouchLife.Screamer();
			GetComponent<Animator>().Rebind();
			GetComponent<Animator>().Play("Slouch Screamer", 0, 0f);
		}
	}
}
