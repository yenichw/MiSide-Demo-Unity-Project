using UnityEngine;

public class Material_EmissionObjects : MonoBehaviour
{
	public Material_EmissionObjectsObject[] objects;

	public bool deactiveStart;

	private void Start()
	{
		for (int i = 0; i < objects.Length; i++)
		{
			if (objects[i].objectMesh.GetComponent<MeshRenderer>() != null)
			{
				for (int j = 0; j < objects[i].objectMesh.GetComponent<MeshRenderer>().materials.Length; j++)
				{
					objects[i].colorsWas[j] = objects[i].objectMesh.GetComponent<MeshRenderer>().materials[j].GetColor("_EmissionColor");
				}
			}
			if (objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>() != null)
			{
				for (int k = 0; k < objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>().materials.Length; k++)
				{
					objects[i].colorsWas[k] = objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>().materials[k].GetColor("_EmissionColor");
				}
			}
		}
		if (deactiveStart)
		{
			Activation(x: false);
		}
	}

	public void Activation(bool x)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			if (objects[i].objectMesh.GetComponent<MeshRenderer>() != null)
			{
				for (int j = 0; j < objects[i].objectMesh.GetComponent<MeshRenderer>().materials.Length; j++)
				{
					if (x)
					{
						objects[i].objectMesh.GetComponent<MeshRenderer>().materials[j].SetColor("_EmissionColor", objects[i].colorsWas[j]);
					}
					else
					{
						objects[i].objectMesh.GetComponent<MeshRenderer>().materials[j].SetColor("_EmissionColor", Color.black);
					}
				}
			}
			if (!(objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>() != null))
			{
				continue;
			}
			for (int k = 0; k < objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>().materials.Length; k++)
			{
				if (x)
				{
					objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>().materials[k].SetColor("_EmissionColor", objects[i].colorsWas[k]);
				}
				else
				{
					objects[i].objectMesh.GetComponent<SkinnedMeshRenderer>().materials[k].SetColor("_EmissionColor", Color.black);
				}
			}
		}
	}
}
