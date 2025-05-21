using UnityEngine;

public class Object_DontDestroy : MonoBehaviour
{
	private void Start()
	{
		base.transform.parent = null;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
