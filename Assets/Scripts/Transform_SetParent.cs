using UnityEngine;

[AddComponentMenu("Functions/Transform/Hierarchy Parent")]
public class Transform_SetParent : MonoBehaviour
{
	public void ParentHierarchy(Transform x)
	{
		base.transform.SetParent(x);
	}
}
