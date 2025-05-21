using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Audio_DestroyTime : MonoBehaviour
{
	public AudioSource audioSource;

	public UnityEvent eventStop;

	private void Start()
	{
		StartCoroutine(TimeDestroy());
	}

	private IEnumerator TimeDestroy()
	{
		yield return new WaitForSeconds(audioSource.clip.length);
		eventStop.Invoke();
		Object.Destroy(base.gameObject);
	}
}
