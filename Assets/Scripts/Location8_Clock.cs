using UnityEngine;
using UnityEngine.Events;

public class Location8_Clock : MonoBehaviour
{
	private bool playerCanMoveClock;

	private bool playOtherClock;

	private bool playGame;

	private float timeClock;

	private float timeOtherClock;

	[Header("Часы")]
	public AudioSource audioClockMove;

	public Transform clockSecond;

	public Transform clockMinute;

	public Transform clockHour;

	public Transform[] otherClockSecond;

	public Transform[] otherMinuteSecond;

	public Transform[] otherHourSecond;

	public float[] otherTimesSound;

	public GameObject[] clocks;

	[Header("Прохождение")]
	public UnityEvent eventStopTime;

	public UnityEvent eventCheckWin;

	private void Update()
	{
		if (playerCanMoveClock)
		{
			if (Input.GetAxis("Horizontal") != 0f)
			{
				if (!audioClockMove.isPlaying)
				{
					audioClockMove.Play();
				}
				if (Input.GetAxis("Horizontal") > 0.5f)
				{
					timeClock += Time.deltaTime * 0.05f;
					if (timeClock > 1f)
					{
						timeClock -= 1f;
					}
				}
				if (Input.GetAxis("Horizontal") < -0.5f)
				{
					timeClock -= Time.deltaTime * 0.05f;
					if (timeClock < 0f)
					{
						timeClock += 1f;
					}
				}
			}
			else if (audioClockMove.isPlaying)
			{
				audioClockMove.Stop();
			}
			clockSecond.localRotation = Quaternion.Euler(0f, timeClock * 60f * 360f, 0f);
			clockMinute.localRotation = Quaternion.Euler(0f, timeClock * 24f * 360f, 0f);
			clockHour.localRotation = Quaternion.Euler(0f, timeClock * 360f, 0f);
		}
		if (!playOtherClock)
		{
			return;
		}
		timeClock += Time.deltaTime * 5f;
		if (timeClock > 1f)
		{
			timeClock -= 1f;
		}
		clockSecond.localRotation = Quaternion.Euler(0f, timeClock * 60f * 360f, 0f);
		clockMinute.localRotation = Quaternion.Euler(0f, timeClock * 24f * 360f, 0f);
		clockHour.localRotation = Quaternion.Euler(0f, timeClock * 360f, 0f);
		timeOtherClock += Time.deltaTime * 5f;
		if (timeOtherClock > 1f)
		{
			timeOtherClock -= 1f;
		}
		for (int i = 0; i < otherClockSecond.Length; i++)
		{
			otherTimesSound[i] += Time.deltaTime * 8f;
			if (otherTimesSound[i] > 1f)
			{
				otherTimesSound[i] = 0f;
				clocks[i].GetComponent<ObjectClock>().AudioPlay();
			}
			otherClockSecond[i].localRotation = Quaternion.Euler(0f, timeClock * 60f * 360f, 90f);
			otherMinuteSecond[i].localRotation = Quaternion.Euler(0f, timeClock * 24f * 360f, 90f);
			otherHourSecond[i].localRotation = Quaternion.Euler(0f, timeClock * 360f, 90f);
		}
	}

	public void PlayerCanMoveClock(bool _x)
	{
		playerCanMoveClock = _x;
		if (!_x)
		{
			audioClockMove.Stop();
		}
	}

	public void OtherClockMove(bool _x)
	{
		playOtherClock = _x;
		if (_x)
		{
			for (int i = 0; i < clocks.Length; i++)
			{
				otherTimesSound[i] = Random.Range(0f, 1f);
				clocks[i].GetComponent<ObjectClock>().enabled = false;
			}
		}
		if (!_x)
		{
			playGame = true;
			eventStopTime.Invoke();
			if ((double)timeClock < 0.5)
			{
				timeClock = Random.Range(0.6f, 0.95f);
			}
			else
			{
				timeClock = Random.Range(0.05f, 0.4f);
			}
			clockSecond.localRotation = Quaternion.Euler(0f, timeClock * 60f * 360f, 0f);
			clockMinute.localRotation = Quaternion.Euler(0f, timeClock * 24f * 360f, 0f);
			clockHour.localRotation = Quaternion.Euler(0f, timeClock * 360f, 0f);
		}
	}

	public void CheckTime()
	{
		if (!playGame)
		{
			return;
		}
		if (timeOtherClock < 0.97f && timeOtherClock > 0.03f)
		{
			if ((double)timeClock > (double)timeOtherClock - 0.03 && (double)timeClock < (double)timeOtherClock + 0.03)
			{
				eventCheckWin.Invoke();
			}
		}
		else if ((double)timeClock > 0.97 && (double)timeClock < 0.03)
		{
			eventCheckWin.Invoke();
		}
	}
}
