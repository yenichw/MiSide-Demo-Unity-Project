using UnityEngine;
using UnityEngine.Events;

public class Time_Pause : MonoBehaviour
{
	public UnityEvent eventPause;

	public UnityEvent eventPlay;

	private bool isPause;

	private void LateUpdate()
	{
		if (!isPause)
		{
			if (Time.timeScale == 0f)
			{
				isPause = true;
				eventPause.Invoke();
			}
		}
		else if (Time.timeScale > 0f)
		{
			isPause = false;
			eventPlay.Invoke();
		}
	}

	public void Pause()
	{
		eventPause.Invoke();
	}

	public void Play()
	{
		eventPlay.Invoke();
	}
}
