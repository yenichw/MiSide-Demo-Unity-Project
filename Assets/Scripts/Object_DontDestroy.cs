// based on the original game.Yen Chezky(yenichw)
using UnityEngine;

public class Object_DontDestroy : MonoBehaviour
{
	private void Start()
	{
		base.transform.parent = null;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
