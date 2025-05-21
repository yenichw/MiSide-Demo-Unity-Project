using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class Playable_Animation : MonoBehaviour
{
	public bool useCamera;

	[HideInInspector]
	public Transform cameraTarget;

	public UnityEvent eventStart;

	public UnityEvent eventStop;

	public bool destroyAfter;

	private GameController scrgc;

	private PlayableDirector scrpd;

	private void Start()
	{
		scrpd = GetComponent<PlayableDirector>();
		if (scrpd.playOnAwake)
		{
			Play();
		}
	}

	public void Play()
	{
		scrpd = GetComponent<PlayableDirector>();
		scrpd.stopped += OnPlayableDirectorStopped;
		base.gameObject.SetActive(value: true);
		if (useCamera)
		{
			scrgc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
			scrgc.CutscenePlay(cameraTarget);
			scrpd.Play();
		}
		eventStart.Invoke();
	}

	private void OnPlayableDirectorStopped(PlayableDirector aDirector)
	{
		eventStop.Invoke();
		if (useCamera)
		{
			scrgc.CutsceneStop();
		}
		if (destroyAfter)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
