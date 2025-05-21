using UnityEngine;

public class Audio_Start : MonoBehaviour
{
	[Header("Settings")]
	public bool destroyOnStart;

	public bool enableRestart;

	[Header("Pitch")]
	[Range(-3f, 3f)]
	public float pitchMax = 1.05f;

	[Range(-3f, 3f)]
	public float pitchMin = 0.95f;

	[Header("Time")]
	[Range(0f, 1f)]
	public float timeStartMin;

	[Range(0f, 1f)]
	public float timeStartMax;

	private bool fs;

	private void OnEnable()
	{
		if (!fs)
		{
			AudioSource component = GetComponent<AudioSource>();
			component.pitch = Random.Range(pitchMin, pitchMax);
			if (timeStartMin != timeStartMax)
			{
				component.time = Random.Range(component.clip.length * timeStartMin, component.clip.length * timeStartMax);
			}
			if (destroyOnStart)
			{
				Object.Destroy(this);
			}
			if (!enableRestart)
			{
				fs = true;
			}
		}
	}

	private void DestroyComponent()
	{
		Object.Destroy(this);
	}

	private void DestroyObject()
	{
		Object.Destroy(base.gameObject);
	}
}
