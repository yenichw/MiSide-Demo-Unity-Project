using System.Collections.Generic;
using UnityEngine;

public class Unity_FindMeshNull : MonoBehaviour
{
	public List<GameObject> objects;

	[ContextMenu("Найти объекты без меша")]
	public void Reset()
	{
		if (objects != null && objects.Count > 0)
		{
			objects.Clear();
		}
		objects = new List<GameObject>();
		MeshFilter[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshFilter>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sharedMesh == null)
			{
				objects.Add(componentsInChildren[i].gameObject);
			}
		}
	}

	[ContextMenu("Удалить объекты")]
	public void DestroyObjects()
	{
		if (objects != null && objects.Count > 0)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				Object.DestroyImmediate(objects[i]);
			}
			objects.Clear();
		}
	}
}
