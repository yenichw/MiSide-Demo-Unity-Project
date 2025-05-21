using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Transform/Magnet Event")]
public class Transform_Magnet_Event : MonoBehaviour
{
	public bool Active = true;

	[Range(0f, 1f)]
	public float speed = 0.1f;

	public Transform transformObject;

	public Vector3 position;

	public bool rotationUse = true;

	public Vector3 rotation;

	[Range(0f, 1f)]
	public float useForce;

	public float MinimalDistanceEvent = 0.05f;

	public UnityEvent joinedEvent;

	[HideInInspector]
	public float spds;

	private void Start()
	{
		spds = speed;
	}

	private void FixedUpdate()
	{
		Quaternion b = Quaternion.Euler(rotation);
		if (Active)
		{
			if (transformObject == null)
			{
				if (Vector3.Distance(base.transform.position, position) < MinimalDistanceEvent && Quaternion.Angle(base.transform.rotation, b) - 180f < MinimalDistanceEvent)
				{
					joinedEvent.Invoke();
				}
				base.transform.position = Vector3.Lerp(base.transform.position, position, speed);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, speed);
			}
			else
			{
				if (rotationUse)
				{
					if (Vector3.Distance(base.transform.position, transformObject.position + position) < MinimalDistanceEvent && Quaternion.Angle(base.transform.rotation, b) - 180f < MinimalDistanceEvent)
					{
						joinedEvent.Invoke();
					}
					b.eulerAngles = transformObject.rotation.eulerAngles + rotation;
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, speed);
				}
				else if (Vector3.Distance(base.transform.position, transformObject.position + position) < MinimalDistanceEvent)
				{
					joinedEvent.Invoke();
				}
				base.transform.position = Vector3.Lerp(base.transform.position, transformObject.position + position, speed);
			}
			if (useForce != 0f)
			{
				speed = Mathf.Lerp(speed, 1f, useForce);
			}
		}
		else
		{
			speed = spds;
		}
	}

	public void PassComponent(Transform_Magnet_Event obj)
	{
		obj.speed = speed;
		obj.spds = speed;
		obj.position = position;
		obj.rotation = rotation;
		obj.transformObject = transformObject;
	}

	public void Activation(bool x)
	{
		Active = x;
	}
}
