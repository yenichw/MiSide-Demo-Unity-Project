using UnityEngine;

public class Location7_InfinityArka : MonoBehaviour
{
	public GameObject objectExample;

	public void GenerateInfinity()
	{
		objectExample.SetActive(value: true);
		for (int i = 0; i < 20; i++)
		{
			GameObject obj = Object.Instantiate(objectExample, base.transform);
			obj.transform.localPosition = new Vector3(-5.6f * (float)i, 0f, 0f);
			obj.transform.localRotation = Quaternion.identity;
		}
		for (int j = 1; j < 20; j++)
		{
			GameObject obj2 = Object.Instantiate(objectExample, base.transform);
			obj2.transform.localPosition = new Vector3(5.6f + 5.6f * (float)j, 0f, 0f);
			obj2.transform.localRotation = Quaternion.identity;
		}
		Object.Destroy(objectExample);
		Object.Destroy(this);
	}
}
