using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class Location8_SlouchLife : MonoBehaviour
{
	public Animator animatorSlouch;

	public AnimationClip animationWalkSlouch;

	public Animator doorAnim;

	public Location8_Slouch slouch;

	public UnityEvent eventScreamer;

	public UnityEvent eventRestart;

	public Location8_MitaBrokeLife mita;

	public Location8_InfinityRoom world;

	private float timeSlow;

	private float timeAnimationWalkSlouch;

	private void Start()
	{
		base.transform.position += new Vector3(-10f, 0f, 11.333333f);
		RestartWalk();
	}

	private void Update()
	{
		timeAnimationWalkSlouch -= Time.deltaTime;
		if (timeAnimationWalkSlouch <= 0f)
		{
			RestartWalk();
		}
		if (timeSlow > 0f)
		{
			timeSlow -= Time.deltaTime;
			if (timeSlow < 0f)
			{
				GetComponent<PlayableDirector>().Play();
			}
		}
	}

	private void RestartWalk()
	{
		GetComponent<PlayableDirector>().Stop();
		GetComponent<PlayableDirector>().Play();
		timeAnimationWalkSlouch = animationWalkSlouch.length;
		base.transform.position -= new Vector3(-10f, 0f, 11.333333f);
		if (base.transform.position == Vector3.zero)
		{
			doorAnim.SetTrigger("SlouchOpen");
		}
	}

	public void SlouchBack()
	{
		base.transform.position += new Vector3(-10f, 0f, 11.333333f);
	}

	public void StartLife()
	{
		base.gameObject.SetActive(value: true);
		GetComponent<PlayableDirector>().enabled = true;
		GetComponent<PlayableDirector>().Stop();
		GetComponent<PlayableDirector>().Play();
		doorAnim.SetTrigger("SlouchOpen");
		timeAnimationWalkSlouch = animationWalkSlouch.length;
	}

	public void StopLife()
	{
		GetComponent<PlayableDirector>().Pause();
		slouch.enabled = false;
		slouch.GetComponent<BoxCollider>().enabled = false;
	}

	public void Restart()
	{
		base.transform.position = Vector3.zero;
		eventRestart.Invoke();
		base.gameObject.SetActive(value: false);
		world.MitaWindowTeleport();
	}

	public void Screamer()
	{
		GetComponent<PlayableDirector>().enabled = false;
		eventScreamer.Invoke();
		mita.DontMolestTime();
	}

	public void SlowTime()
	{
		GetComponent<PlayableDirector>().Pause();
		timeSlow = 2f;
	}
}
