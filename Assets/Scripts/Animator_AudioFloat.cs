using UnityEngine;

public class Animator_AudioFloat : MonoBehaviour
{
	public float intensity = 1f;

	public string floatName;

	public AudioSource audioSource;

	public float pitch = 1f;

	public float intensityPitch;

	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		audioSource.volume = anim.GetFloat(floatName) * intensity;
		audioSource.pitch = pitch + anim.GetFloat(floatName) * intensityPitch;
	}
}
