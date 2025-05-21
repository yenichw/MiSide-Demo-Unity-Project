using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BlackScreenAnimation : MonoBehaviour
{
	public Image blackScreen;

	private bool tStart;

	public float time1Start;

	public float time1Finish;

	public UnityEvent eventStop1;

	private bool tStop;

	public float time0Start;

	public float time0Finish;

	public UnityEvent eventStop0;

	private bool play;

	private float timeBlack;

	private void Update()
	{
		if (play)
		{
			timeBlack += Time.deltaTime;
			if (timeBlack >= time1Start && timeBlack < time1Finish)
			{
				blackScreen.color = new Color(0f, 0f, 0f, (timeBlack - time1Start) / (time1Finish - time1Start));
			}
			if (!tStart && timeBlack >= time1Finish)
			{
				tStart = true;
				blackScreen.color = new Color(0f, 0f, 0f, 1f);
				eventStop1.Invoke();
			}
			if (timeBlack >= time0Start && timeBlack < time0Finish)
			{
				blackScreen.color = new Color(0f, 0f, 0f, 1f - (timeBlack - time0Start) / (time0Finish - time0Start));
			}
			if (!tStop && timeBlack >= time0Finish)
			{
				tStop = true;
				blackScreen.color = new Color(0f, 0f, 0f, 0f);
				play = false;
				eventStop0.Invoke();
			}
		}
	}

	public void Play()
	{
		base.gameObject.SetActive(value: true);
		play = true;
		timeBlack = 0f;
		tStart = false;
		tStop = false;
	}
}
