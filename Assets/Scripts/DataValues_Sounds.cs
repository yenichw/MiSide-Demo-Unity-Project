using UnityEngine;

public class DataValues_Sounds : MonoBehaviour
{
	public AudioSource audioSource;

	public AudioClip[] sounds;

	[Header("Loop sound")]
	public AudioClip soundLoop;

	public float volumeSoundLoop = 0.5f;

	public float speedOffLoop = 5f;

	[HideInInspector]
	public string nameFileLocation;

	private bool fs;

	public void StartComponent()
	{
		if (!fs)
		{
			fs = true;
			nameFileLocation = "LocationDialogue " + GlobalTag.world.GetComponent<World>().nameLocation;
			if (soundLoop != null)
			{
				audioSource.clip = soundLoop;
			}
		}
	}

	private void Update()
	{
		if (soundLoop != null && audioSource != null && audioSource.volume > 0f)
		{
			audioSource.volume -= Time.deltaTime * speedOffLoop;
			if (audioSource.volume <= 0f)
			{
				audioSource.volume = 0f;
				audioSource.Stop();
			}
		}
	}
}
