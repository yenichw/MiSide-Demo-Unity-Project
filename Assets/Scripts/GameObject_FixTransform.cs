using UnityEngine;

public class GameObject_FixTransform : MonoBehaviour
{
	public Transform target;

	public Vector3 positionForward;

	public bool positionTarget = true;

	public bool rotationTarget;

	public string tagObject;

	private void Update()
	{
		if (target == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (positionTarget)
		{
			base.transform.position = target.position + target.forward * positionForward.z + target.right * positionForward.x + target.up * positionForward.y;
		}
		if (rotationTarget)
		{
			base.transform.rotation = target.rotation;
		}
		if (!target.gameObject.activeInHierarchy)
		{
			base.transform.parent = target;
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (tagObject != null && tagObject != "")
		{
			target = GameObject.FindWithTag(tagObject).transform;
		}
		if (GameObject.FindWithTag("World") != null)
		{
			base.transform.parent = GameObject.FindWithTag("World").transform;
		}
		else
		{
			base.transform.parent = null;
		}
		if (target == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (positionTarget)
		{
			base.transform.position = target.position + target.forward * positionForward.z + target.right * positionForward.x + target.up * positionForward.y;
		}
		if (rotationTarget)
		{
			base.transform.rotation = target.rotation;
		}
	}
}
