using UnityEngine;

public class Material_Texture : MonoBehaviour
{
	public Texture2D[] textures;

	public int indexMaterial = -1;

	public string valueMaterial;

	public int startIndex = -1;

	private void Start()
	{
		if (startIndex > -1)
		{
			SetTexture(startIndex);
		}
	}

	public void SetTexture(int index)
	{
		if (GetComponent<MeshRenderer>() != null)
		{
			if (indexMaterial == -1)
			{
				GetComponent<MeshRenderer>().material.SetTexture(valueMaterial, textures[index]);
			}
			else
			{
				GetComponent<MeshRenderer>().materials[indexMaterial].SetTexture(valueMaterial, textures[index]);
			}
		}
		if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			if (indexMaterial == -1)
			{
				GetComponent<SkinnedMeshRenderer>().material.SetTexture(valueMaterial, textures[index]);
			}
			else
			{
				GetComponent<SkinnedMeshRenderer>().materials[indexMaterial].SetTexture(valueMaterial, textures[index]);
			}
		}
	}
}
