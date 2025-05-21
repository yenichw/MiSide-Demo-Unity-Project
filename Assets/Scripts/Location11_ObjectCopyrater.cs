using System.Collections.Generic;
using UnityEngine;

public class Location11_ObjectCopyrater : MonoBehaviour
{
	public GameObject objectCopy;

	private float timeCreate;

	private bool play;

	[HideInInspector]
	public List<Location11_ObjectCopyraterAnimation> objects;

	private void Start()
	{
	}

	private void Update()
	{
		if (!play)
		{
			return;
		}
		timeCreate += Time.deltaTime;
		if (timeCreate > 0.5f)
		{
			timeCreate = 0f;
			Location11_ObjectCopyraterAnimation item = new Location11_ObjectCopyraterAnimation
			{
				objectT = Object.Instantiate(objectCopy, objectCopy.transform.parent).transform,
				rotationSpeed = GlobalAM.Vector3Random(-1f, 1f),
				moveSpeed = GlobalAM.Vector3Random(-1f, 1f)
			};
			objects.Add(item);
			for (int i = 0; i < objects.Count; i++)
			{
				if (objects[i].objectT == null)
				{
					objects.RemoveAt(i);
					break;
				}
			}
		}
		if (objects == null || objects.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < objects.Count; j++)
		{
			if (!(objects[j].objectT != null))
			{
				continue;
			}
			objects[j].speed += Time.deltaTime;
			objects[j].objectT.rotation *= Quaternion.Euler(objects[j].rotationSpeed.normalized * (objects[j].speed * Time.deltaTime));
			objects[j].objectT.position += (objects[j].moveSpeed + Vector3.up * 0.7f) * (0.1f * objects[j].speed * Time.deltaTime);
			if (objects[j].speed > 1f)
			{
				objects[j].objectT.localScale -= Vector3.one * Time.deltaTime * 0.1f;
				if (objects[j].objectT.localScale.x < 0f)
				{
					Object.Destroy(objects[j].objectT.gameObject);
				}
			}
		}
	}

	public void Play()
	{
		play = true;
	}
}
