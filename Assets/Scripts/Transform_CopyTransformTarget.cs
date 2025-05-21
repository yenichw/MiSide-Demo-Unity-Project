using UnityEngine;

public class Transform_CopyTransformTarget : MonoBehaviour
{
	public bool onEnable = true;

	public bool onUpdate;

	public bool parentNull;

	public bool player;

	public bool magnetFloor;

	public Transform target;

	public Transform targetY;

	public bool offRotation;

	private void LateUpdate()
	{
		if (parentNull && target == null)
		{
			Object.Destroy(base.gameObject);
		}
		if (onUpdate && target != null)
		{
			CopyTransform();
		}
	}

	private void OnEnable()
	{
		if (player)
		{
			target = GlobalTag.player.transform;
		}
		if (parentNull)
		{
			if (GameObject.FindWithTag("World") != null)
			{
				base.transform.parent = GameObject.FindWithTag("World").transform;
			}
			else
			{
				base.transform.parent = null;
			}
		}
		if (onEnable)
		{
			CopyTransform();
		}
	}

	public void CopyTransform()
	{
		if (!(target != null))
		{
			return;
		}
		if (!target.gameObject.activeInHierarchy)
		{
			base.transform.position = Vector3.one * 1000f;
			return;
		}
		if (!targetY)
		{
			base.transform.position = target.position;
			if (!offRotation)
			{
				base.transform.rotation = target.rotation;
			}
		}
		else
		{
			base.transform.position = new Vector3(target.position.x, targetY.position.y, target.position.z);
			if (!offRotation)
			{
				base.transform.rotation = target.rotation;
			}
		}
		if (magnetFloor && Physics.Raycast(base.transform.position, -Vector3.up, out var hitInfo, 10f, LayerMask.GetMask("Default")))
		{
			base.transform.position = hitInfo.point;
		}
	}
}
