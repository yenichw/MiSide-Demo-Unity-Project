using UnityEngine;
using UnityEngine.Events;

public class ItemInteractive : MonoBehaviour
{
	public UnityEvent eventClick;

	public Transform childObject;

	public float crosshairSize = 1f;

	[Header("Pause")]
	public UnityEvent eventPauseActive;

	public UnityEvent eventPauseDeactive;

	private bool toggleActive;

	private PlayerMove scrpm;

	private GameController scrgc;

	[Header("Информация")]
	public bool canUseOther;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		scrgc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		if (childObject != null)
		{
			childObject.parent = null;
		}
	}

	private void Update()
	{
		if (Time.timeScale != 0f && !scrpm.scrppik.itemDown && !scrpm.animationHandRun && !scrpm.animationRun && scrpm.objectCastHandInteractive == null && scrpm.objectCastInteractive == null)
		{
			if (Input.GetMouseButtonDown(0))
			{
				eventClick.Invoke();
			}
			if (!toggleActive)
			{
				toggleActive = true;
				scrgc.crosshairSize = crosshairSize;
			}
		}
		else if (toggleActive)
		{
			toggleActive = false;
			scrgc.crosshairSize = 1f;
		}
		if (childObject != null)
		{
			childObject.position = base.transform.position;
			childObject.rotation = base.transform.rotation;
		}
		if (Time.timeScale != 0f && !scrpm.animationHandRun && !scrpm.animationRun && !scrgc.cutsceneRun)
		{
			canUseOther = true;
		}
		else
		{
			canUseOther = false;
		}
	}

	private void OnDestroy()
	{
		if (childObject != null)
		{
			scrgc.crosshairSize = 1f;
			Object.Destroy(childObject.gameObject);
		}
	}

	public void Pause(bool _pause)
	{
		if (_pause)
		{
			eventPauseActive.Invoke();
		}
		else
		{
			eventPauseDeactive.Invoke();
		}
	}
}
