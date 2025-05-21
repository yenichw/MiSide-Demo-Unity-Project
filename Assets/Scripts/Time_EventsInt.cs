using UnityEngine;

[AddComponentMenu("Functions/Time/Time event int")]
public class Time_EventsInt : MonoBehaviour
{
	public bool active;

	public int timer;

	[SerializeField]
	public TimePointInt[] events;

	private void FixedUpdate()
	{
		if (active)
		{
			timer++;
		}
		for (int i = 0; i < events.Length; i++)
		{
			if ((float)timer == events[i].time)
			{
				events[i]._event.Invoke();
			}
		}
	}

	public void DestroyComponent()
	{
		Object.Destroy(this);
	}

	public void ActivationTimer(bool x)
	{
		active = x;
	}

	public void RestartTimer()
	{
		timer = 0;
	}
}
