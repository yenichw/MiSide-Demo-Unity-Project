using UnityEngine;
using UnityEngine.Events;

public class Trigger_Event : MonoBehaviour
{
	public UnityEvent eventEnter;

	public UnityEvent eventExit;

	public GameObject objectTarget;

	[Header("Settings")]
	public bool destroyOnExit;

	public bool loop;

	private Vector3 updateSize;

	public bool updateCast;

	public LayerMask layerUpdateCast;

	private bool enterTime;

	private bool exitTime;

	[Space(20f)]
	public bool triggerActive;

	private void Start()
	{
		if (objectTarget == null)
		{
			objectTarget = GlobalTag.player;
		}
		if (updateCast)
		{
			updateSize = GetComponent<BoxCollider>().size / 2f;
			Object.Destroy(GetComponent<BoxCollider>());
		}
	}

	private void FixedUpdate()
	{
		if (!updateCast)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(base.gameObject.transform.position, updateSize, base.transform.rotation, layerUpdateCast);
		if (!triggerActive)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject == objectTarget)
				{
					TargetEnter();
					break;
				}
			}
			return;
		}
		bool flag = true;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].gameObject == objectTarget)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			TargetExit();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (objectTarget != null && other.gameObject == objectTarget)
		{
			TargetEnter();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (objectTarget != null && other.gameObject == objectTarget)
		{
			TargetExit();
		}
	}

	private void TargetEnter()
	{
		if (!triggerActive && !enterTime)
		{
			triggerActive = true;
			if (!loop)
			{
				enterTime = true;
			}
			eventEnter.Invoke();
		}
	}

	private void TargetExit()
	{
		if (!triggerActive || exitTime)
		{
			return;
		}
		triggerActive = false;
		if (destroyOnExit)
		{
			Object.Destroy(base.gameObject);
			if (!loop)
			{
				exitTime = true;
			}
		}
		eventExit.Invoke();
	}

	public void DestroyMe()
	{
		Object.Destroy(base.gameObject);
	}

	[ContextMenu("Restart trigger")]
	public void Restart()
	{
		enterTime = false;
		exitTime = false;
		triggerActive = false;
	}

	private void OnDisable()
	{
		triggerActive = false;
	}
}
