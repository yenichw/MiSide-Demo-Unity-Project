using EPOOutline;
using UnityEngine;
using UnityEngine.Events;

public class Trigger_MouseClick : MonoBehaviour
{
	public bool mouseInterface;

	public bool deactiveAfterClick;

	public UnityEvent eventClick;

	public UnityEvent eventEnter;

	public UnityEvent eventExit;

	private bool deactiveOutline;

	private Outlinable scroutline;

	private bool mouse;

	private bool active;

	private PlayerMove scrpm;

	private GameController scrgc;

	private void Start()
	{
		active = true;
		if (!mouseInterface)
		{
			scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		}
		else
		{
			scrgc = GlobalTag.gameController.GetComponent<GameController>();
		}
		if (GetComponent<Outlinable>() != null)
		{
			scroutline = GetComponent<Outlinable>();
		}
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (!mouseInterface)
		{
			if (!mouse && scrpm.objectCast == base.gameObject)
			{
				Enter();
			}
			if (mouse && scrpm.objectCast != base.gameObject)
			{
				Exit();
			}
			if (mouse && scrpm.objectCast == base.gameObject && Input.GetMouseButtonDown(0))
			{
				Click();
			}
		}
		else
		{
			if (!mouse && scrgc.objectCast == base.gameObject)
			{
				Enter();
			}
			if (mouse && scrgc.objectCast != base.gameObject)
			{
				Exit();
			}
			if (mouse && scrgc.objectCast == base.gameObject && Input.GetMouseButtonDown(0))
			{
				Click();
			}
		}
	}

	public void ActivationOutline(bool x)
	{
		deactiveOutline = !x;
		if (!x && scroutline != null)
		{
			scroutline.enabled = false;
		}
	}

	public void Enter()
	{
		mouse = true;
		eventEnter.Invoke();
		if (scroutline != null && !deactiveOutline)
		{
			scroutline.enabled = true;
		}
	}

	public void Exit()
	{
		mouse = false;
		eventExit.Invoke();
		if (scroutline != null && !deactiveOutline)
		{
			scroutline.enabled = false;
		}
	}

	[ContextMenu("Клик")]
	public void Click()
	{
		eventClick.Invoke();
		if (deactiveAfterClick)
		{
			ActivatedObject(x: false);
		}
	}

	private void OnDisable()
	{
		mouse = false;
		eventExit.Invoke();
		if (scroutline != null)
		{
			scroutline.enabled = false;
		}
	}

	public void ActivatedObject(bool x)
	{
		base.enabled = x;
		active = x;
		if (!x && GetComponent<Outlinable>() != null)
		{
			GetComponent<Outlinable>().enabled = false;
		}
	}

	public void Restart()
	{
		active = true;
		if (scroutline != null)
		{
			scroutline.enabled = false;
		}
	}
}
