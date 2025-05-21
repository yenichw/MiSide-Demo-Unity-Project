using UnityEngine;

public class Time_UpdateEvents : MonoBehaviour
{
	public bool active = true;

	public float timeAddRandom;

	public float time;

	public bool loop;

	public TimePointBool[] eventsOnTime;

	private float maxTime;

	private void Start()
	{
		for (int i = 0; i < eventsOnTime.Length; i++)
		{
			if (maxTime < eventsOnTime[i].time)
			{
				maxTime = eventsOnTime[i].time;
			}
		}
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (timeAddRandom > 0f)
		{
			time += Time.deltaTime + timeAddRandom * Random.Range(0f, 1f) * Time.deltaTime;
		}
		else
		{
			time += Time.deltaTime;
		}
		for (int i = 0; i < eventsOnTime.Length; i++)
		{
			if (!eventsOnTime[i].eventActive && time >= eventsOnTime[i].time)
			{
				eventsOnTime[i]._event.Invoke();
				eventsOnTime[i].eventActive = true;
			}
		}
		if (loop && time > maxTime)
		{
			ResetTime();
		}
	}

	private void ResetTime()
	{
		time = 0f;
		for (int i = 0; i < eventsOnTime.Length; i++)
		{
			eventsOnTime[i].eventActive = false;
		}
	}

	public void Activation(bool x)
	{
		active = x;
	}
}
