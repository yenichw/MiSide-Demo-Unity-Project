using UnityEngine;

public class GameObject_CreateMe : MonoBehaviour
{
	private int tmd = 2;

	private Transform createMeTransform;

	private Vector3 createMePositionLocal;

	private Vector3 createMeRotationLocal;

	private void LateUpdate()
	{
		tmd--;
		if (createMeTransform != null)
		{
			base.transform.SetPositionAndRotation(createMeTransform.position + createMeTransform.forward * createMePositionLocal.z + createMeTransform.right * createMePositionLocal.x + createMeTransform.up * createMePositionLocal.y, createMeTransform.rotation * Quaternion.Euler(createMeRotationLocal));
		}
		if (tmd == 0)
		{
			Object.Destroy(this);
		}
	}

	public void CreatePositionTransform(Transform _Transform, Vector3 _localPosition, Vector3 _localRotation)
	{
		createMeTransform = _Transform;
		createMePositionLocal = _localPosition;
		createMeRotationLocal = _localRotation;
		base.transform.SetPositionAndRotation(createMeTransform.position + createMeTransform.forward * createMePositionLocal.z + createMeTransform.right * createMePositionLocal.x + createMeTransform.up * createMePositionLocal.y, createMeTransform.rotation * Quaternion.Euler(createMeRotationLocal));
	}
}
