using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Time/Time event")]
public class Time_Events : MonoBehaviour
{
	public bool StartYield;

	public bool destroyAfter;

	[SerializeField]
	public TimePoint[] EventsOnTime;

	public UnityEvent eventStart;

	private int i;

	private int StartNumYield;

	private bool enableStart;

	private void Start()
	{
		eventStart.Invoke();
	}

	private void OnEnable()
	{
		if (enableStart)
		{
			return;
		}
		enableStart = true;
		if (StartYield)
		{
			for (i = 0; i < EventsOnTime.Length; i++)
			{
				StartCoroutine(OneYieldStart(i));
			}
		}
		if (StartNumYield != 0)
		{
			OneYieldStart(StartNumYield);
		}
	}

	private IEnumerator OneYieldStart(int num)
	{
		if (EventsOnTime[num].timeAnimationClip == null)
		{
			yield return new WaitForSeconds(EventsOnTime[num].time);
			ConsoleMain.ConsolePrintGame("Time start: " + EventsOnTime[num].time);
			EventsOnTime[num]._event.Invoke();
			if (destroyAfter && num == EventsOnTime.Length - 1)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			yield return new WaitForSeconds(EventsOnTime[num].timeAnimationClip.length + EventsOnTime[num].time);
			ConsoleMain.ConsolePrintGame("Time start: " + EventsOnTime[num].timeAnimationClip.length + EventsOnTime[num].time);
			EventsOnTime[num]._event.Invoke();
			if (destroyAfter && num == EventsOnTime.Length - 1)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	[ContextMenu("Запустить таймер")]
	public void YieldRestart()
	{
		base.gameObject.SetActive(value: true);
		for (i = 0; i < EventsOnTime.Length; i++)
		{
			StartCoroutine(OneYieldStart(i));
		}
	}

	public void YieldOne(int num)
	{
		StartCoroutine(OneYieldStart(num));
	}

	public void YieldOneLoadAwake(int num)
	{
		StartNumYield = num;
	}

	public void StopAllTime()
	{
		StopAllCoroutines();
	}

	public void StopAndAllRestart()
	{
		StopAllCoroutines();
		for (i = 0; i < EventsOnTime.Length; i++)
		{
			StartCoroutine(OneYieldStart(i));
		}
	}
}
