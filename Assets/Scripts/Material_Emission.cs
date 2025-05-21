using UnityEngine;

public class Material_Emission : MonoBehaviour
{
	public Color colorEmission;

	public float multiple = 1f;

	public bool activeOnStart = true;

	public bool deactiveOnStart;

	public Color[] colors;

	private void Start()
	{
		if (activeOnStart)
		{
			Activation(x: true);
		}
		if (deactiveOnStart)
		{
			Activation(x: false);
		}
	}

	public void Activation(bool x)
	{
		if (GetComponent<MeshRenderer>() != null)
		{
			if (x)
			{
				GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", colorEmission * multiple);
			}
			else
			{
				GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
			}
		}
		if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			if (x)
			{
				GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", colorEmission * multiple);
			}
			else
			{
				GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", Color.black);
			}
		}
	}

	public void SetColor(int _index)
	{
		if (GetComponent<MeshRenderer>() != null)
		{
			GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", colors[_index] * multiple);
		}
		if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", colors[_index] * multiple);
		}
	}
}
