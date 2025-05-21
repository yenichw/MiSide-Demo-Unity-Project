using UnityEngine;

[AddComponentMenu("Functions/GameObject/Destroys")]
public class GameObject_Destroys : MonoBehaviour
{
	public bool onStart;

	public GameObject[] objects;

	public GameObject[] objectsActive;

	public GameObject[] objectsDeactive;

	public bool selfDestroy;

	private void Start()
	{
		if (onStart)
		{
			Destroys();
		}
	}

	public void Destroys()
	{
		for (int i = 0; i < objects.Length; i++)
		{
			if (objects[i] != null)
			{
				Object.Destroy(objects[i]);
			}
		}
		if (objectsActive != null && objectsActive.Length != 0)
		{
			for (int j = 0; j < objectsActive.Length; j++)
			{
				if (objectsActive[j] != null)
				{
					objectsActive[j].SetActive(value: true);
				}
			}
		}
		if (objectsDeactive != null && objectsDeactive.Length != 0)
		{
			for (int k = 0; k < objectsDeactive.Length; k++)
			{
				if (objectsDeactive[k] != null)
				{
					objectsDeactive[k].SetActive(value: false);
				}
			}
		}
		if (selfDestroy)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
