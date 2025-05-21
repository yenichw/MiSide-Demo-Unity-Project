using EPOOutline;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outlinable))]
public class ObjectInteractive : MonoBehaviour
{
	private Outlinable outline;

	private PlayerMove scrpm;

	private GameController scrgc;

	public bool active = true;

	public bool ignoryAnimationHand;

	public float timeDeactive = -1f;

	private float timeDeactiveNow;

	public UnityEvent eventClick;

	[Header("Ссылка на объект")]
	public GameObject objectInteractive;

	[HideInInspector]
	public bool interactiveIK;

	[Header("Дополнительные настройки")]
	public bool destroyObjectInteractive;

	public bool destroyComponent;

	public bool deactiveObject;

	public float distanceFloor = 1f;

	private void Start()
	{
		outline = GetComponent<Outlinable>();
		outline.enabled = false;
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		if (GetComponent<ObjectInteractiveReqIK>() != null)
		{
			interactiveIK = true;
		}
	}

	private void Update()
	{
		if (((!interactiveIK && scrpm.objectCastInteractive == objectInteractive && (!scrpm.animationHandRun || (scrpm.animationHandRun && ignoryAnimationHand))) || (interactiveIK && scrpm.objectCastHandInteractive == base.gameObject)) && active)
		{
			if (outline.OutlineParameters.Color.a < 1f)
			{
				if (!outline.enabled)
				{
					outline.enabled = true;
				}
				outline.OutlineParameters.Color = new Color(1f, 1f, 1f, outline.OutlineParameters.Color.a + Time.deltaTime * 10f);
				if (outline.OutlineParameters.Color.a >= 1f)
				{
					outline.OutlineParameters.Color = new Color(1f, 1f, 1f, 1f);
				}
			}
			if (objectInteractive != null)
			{
				scrgc.timeLightCast = 2;
			}
			if (scrpm.blockInteractive == 0 && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Interactive")))
			{
				Click();
			}
		}
		else if (outline.OutlineParameters.Color.a > 0f)
		{
			outline.OutlineParameters.Color = new Color(1f, 1f, 1f, outline.OutlineParameters.Color.a - Time.deltaTime * 10f);
			if (outline.OutlineParameters.Color.a <= 0f)
			{
				outline.OutlineParameters.Color = new Color(1f, 1f, 1f, 0f);
				outline.enabled = false;
			}
		}
		if (timeDeactiveNow > 0f)
		{
			timeDeactiveNow -= Time.deltaTime;
			if (timeDeactiveNow <= 0f)
			{
				timeDeactiveNow = 0f;
				active = true;
			}
		}
	}

	public void Click()
	{
		if (active)
		{
			eventClick.Invoke();
			timeDeactiveNow = timeDeactive;
			if (timeDeactiveNow < 0f || timeDeactiveNow > 0f)
			{
				active = false;
			}
			if (destroyObjectInteractive)
			{
				Object.Destroy(base.gameObject);
			}
			if (destroyComponent)
			{
				Object.Destroy(this);
				outline.enabled = false;
			}
			if (deactiveObject)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void Activation(bool x)
	{
		active = x;
		timeDeactiveNow = 0f;
	}

	public void ActivationTime(float _x)
	{
		timeDeactiveNow = _x;
	}

	public void DestroyObject()
	{
		Object.Destroy(base.gameObject);
	}
}
