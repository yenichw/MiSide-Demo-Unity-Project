using UnityEngine;
using UnityEngine.Events;

public class GameObject_Optimization : MonoBehaviour
{
	public UnityEvent eventOptimization;

	public GameObject[] objectsDestroy;

	public void OptimizationBake()
	{
		base.gameObject.isStatic = true;
		if (GetComponent<SpringJoint>() != null)
		{
			Object.Destroy(GetComponent<SpringJoint>());
		}
		if (GetComponent<Rigidbody>() != null)
		{
			Object.Destroy(GetComponent<Rigidbody>());
		}
		if (GetComponent<BoxCollider>() != null)
		{
			Object.Destroy(GetComponent<BoxCollider>());
		}
		if (objectsDestroy != null && objectsDestroy.Length != 0)
		{
			for (int i = 0; i < objectsDestroy.Length; i++)
			{
				if (objectsDestroy[i] != null)
				{
					Object.Destroy(objectsDestroy[i]);
				}
			}
		}
		eventOptimization.Invoke();
		Object.Destroy(this);
	}
}
