using UnityEngine;
using UnityEngine.Events;

public class Trigger_MouseEvent : MonoBehaviour
{
	public UnityEvent eventEnter;

	private bool enterVoke;

	public UnityEvent eventExit;

	private bool exitVoke;

	public bool oneTime;

	public bool destroyEnterMouse;

	public GameObject[] addonTrigger;

	private bool mouse;

	private PlayerMove scrpm;

	private bool timeEventReady;

	private float timeHoldNow;

	[Header("Время")]
	public float timeHold;

	public bool timeContinueHold;

	public UnityEvent eventTimeEnd;

	[Header("Настройки")]
	public float distance;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		timeHoldNow = timeHold;
		if (distance == 0f)
		{
			distance = 1000f;
		}
	}

	private void Update()
	{
		if (addonTrigger == null)
		{
			if (!mouse && scrpm.objectCast == base.gameObject && scrpm.distanceCast < distance)
			{
				Enter();
			}
			if (mouse && (scrpm.objectCast != base.gameObject || scrpm.distanceCast < distance))
			{
				Exit();
			}
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < addonTrigger.Length; i++)
			{
				if (scrpm.objectCast == addonTrigger[i])
				{
					flag = true;
					break;
				}
			}
			if (scrpm.objectCast == base.gameObject && scrpm.distanceCast < distance)
			{
				flag = true;
			}
			if (!mouse && flag)
			{
				Enter();
			}
			if (mouse && !flag)
			{
				Exit();
			}
		}
		if (!timeEventReady && timeHoldNow > 0f && mouse)
		{
			timeHoldNow -= Time.deltaTime;
			if (timeHoldNow <= 0f)
			{
				eventTimeEnd.Invoke();
				timeEventReady = true;
			}
		}
	}

	public void Enter()
	{
		mouse = true;
		if (!enterVoke)
		{
			eventEnter.Invoke();
		}
		if (oneTime)
		{
			enterVoke = true;
		}
		if (destroyEnterMouse)
		{
			Object.Destroy(base.gameObject);
		}
		if (!timeContinueHold)
		{
			timeHoldNow = timeHold;
		}
	}

	public void Exit()
	{
		mouse = false;
		if (!exitVoke)
		{
			eventExit.Invoke();
		}
		if (oneTime)
		{
			exitVoke = true;
		}
	}

	public void DestroyMe()
	{
		Object.Destroy(base.gameObject);
	}
}
