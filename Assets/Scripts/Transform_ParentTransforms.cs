using UnityEngine;

public class Transform_ParentTransforms : MonoBehaviour
{
	public Transform[] transforms;

	public Transform parent;

	private void Start()
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			transforms[i].SetParent(parent);
		}
		Object.Destroy(this);
	}
}
