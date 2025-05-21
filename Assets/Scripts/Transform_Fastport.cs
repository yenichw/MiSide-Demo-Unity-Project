using UnityEngine;

public class Transform_Fastport : MonoBehaviour
{
	public Transform[] objects;

	public Vector3 position;

	public bool onStart;

	public bool _rotation;

	public Vector3 rotation;

	public Transform orTarget;

	private void Start()
	{
		if (onStart)
		{
			Port();
		}
	}

	public void Port()
	{
		if (objects.Length != 0)
		{
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].position = position;
				if (orTarget != null)
				{
					objects[i].position = orTarget.transform.position + position;
				}
				else
				{
					objects[i].position = position;
				}
				if (_rotation)
				{
					objects[i].rotation = Quaternion.Euler(rotation);
				}
			}
		}
		else
		{
			if (orTarget != null)
			{
				base.transform.position = orTarget.transform.position + position;
			}
			else
			{
				base.transform.position = position;
			}
			if (_rotation)
			{
				base.transform.rotation = Quaternion.Euler(rotation);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (orTarget == null)
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.8f);
			Gizmos.DrawSphere(position, 0.05f);
			Gizmos.color = new Color(1f, 0f, 0f, 1f);
			Gizmos.DrawLine(position + new Vector3(2f, 0f, 0f), position - new Vector3(2f, 0f, 0f));
			Gizmos.DrawLine(position + new Vector3(0f, 2f, 0f), position - new Vector3(0f, 2f, 0f));
			Gizmos.DrawLine(position + new Vector3(0f, 0f, 2f), position - new Vector3(0f, 0f, 2f));
		}
		else
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.8f);
			Gizmos.DrawSphere(orTarget.transform.position + position, 0.05f);
			Gizmos.color = new Color(1f, 0f, 0f, 1f);
			Gizmos.DrawLine(orTarget.transform.position + position + new Vector3(2f, 0f, 0f), orTarget.transform.position + position - new Vector3(2f, 0f, 0f));
			Gizmos.DrawLine(orTarget.transform.position + position + new Vector3(0f, 2f, 0f), orTarget.transform.position + position - new Vector3(0f, 2f, 0f));
			Gizmos.DrawLine(orTarget.transform.position + position + new Vector3(0f, 0f, 2f), orTarget.transform.position + position - new Vector3(0f, 0f, 2f));
		}
	}
}
