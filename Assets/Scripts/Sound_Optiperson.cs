using UnityEngine;

public class Sound_Optiperson : MonoBehaviour
{
	public AudioPoint[] sounds;

	public AudioSource audioSource;

	private void Start()
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			sounds[i].au = Object.Instantiate(audioSource, base.transform);
			sounds[i].au.transform.localPosition = Vector3.zero;
			sounds[i].au.gameObject.name = "SoundCreate " + i;
		}
		Object.Destroy(audioSource.gameObject);
	}

	public void sEvent(int _index)
	{
		sounds[_index].au.clip = sounds[_index].sounds[Random.Range(0, sounds[_index].sounds.Length)];
		if (sounds[_index].randomPitch)
		{
			sounds[_index].au.pitch = Random.Range(sounds[_index].pitchRandomMin, sounds[_index].pitchRandomMax);
		}
		sounds[_index].au.Play();
	}
}
