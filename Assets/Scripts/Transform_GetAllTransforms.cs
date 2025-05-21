using UnityEngine;

public class Transform_GetAllTransforms : MonoBehaviour
{
	public Transform[] transforms;

	public void Scan()
	{
		transforms = base.transform.GetComponentsInChildren<Transform>();
	}

	public void CopyTransformPos(Transform_GetAllTransforms _transform)
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			transforms[i].SetLocalPositionAndRotation(_transform.transforms[i].localPosition, _transform.transforms[i].localRotation);
		}
	}
}
