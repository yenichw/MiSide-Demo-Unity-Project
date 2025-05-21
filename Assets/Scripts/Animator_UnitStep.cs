using UnityEngine;

public class Animator_UnitStep : MonoBehaviour
{
	public AudioSource audioAddone;

	public AudioSource audioStep;

	public AudioClip[] soundsStep;

	public string nameFloatMove;

	[Header("Дополнение")]
	public bool middleAudio;

	public Transform footRight;

	public Transform footLeft;

	private int lastSound;

	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (middleAudio)
		{
			audioStep.transform.position = Vector3.Lerp(footRight.position, footLeft.position, 0.5f);
		}
	}

	public void Step()
	{
		audioStep.clip = soundsStep[FixRandomSound()];
		audioStep.pitch = Random.Range(0.95f, 1.05f);
		audioStep.Play();
		if (audioAddone != null)
		{
			audioAddone.Play();
		}
	}

	public void StepMove()
	{
		if (nameFloatMove != null && nameFloatMove != "")
		{
			if (Mathf.Abs(anim.GetFloat(nameFloatMove)) > 0.1f)
			{
				Step();
			}
		}
		else
		{
			Step();
		}
	}

	private int FixRandomSound()
	{
		int num = Random.Range(0, soundsStep.Length);
		if (soundsStep.Length > 1 && num == lastSound)
		{
			num++;
			if (num > soundsStep.Length - 1)
			{
				num = 0;
			}
		}
		return num;
	}
}
