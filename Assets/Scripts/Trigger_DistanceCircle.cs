using UnityEngine;
using UnityEngine.Events;

public class Trigger_DistanceCircle : MonoBehaviour
{
	public Transform target;

	public float radius = 1f;

	public UnityEvent eventEnter;

	public UnityEvent eventExit;

	[Header("Settings")]
	public bool destroyOnExit;

	private bool targetInside;

	private void Start()
	{
		if (target == null)
		{
			target = GlobalTag.player.transform;
		}
	}

	private void Update()
	{
		if (GlobalAM.DistanceFloor(base.transform.position, target.position) < radius)
		{
			if (!targetInside)
			{
				targetInside = true;
				eventEnter.Invoke();
			}
		}
		else if (targetInside)
		{
			targetInside = false;
			eventExit.Invoke();
			if (destroyOnExit)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
