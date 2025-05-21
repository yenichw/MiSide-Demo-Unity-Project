using UnityEngine;

public class Material_ColorVariablesSimple : MonoBehaviour
{
	public string nameVariable;

	public Color color;

	[ContextMenu("Покрасить")]
	private void Start()
	{
		if (GetComponent<MeshRenderer>() != null)
		{
			GetComponent<MeshRenderer>().material.SetColor(nameVariable, color);
		}
		if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			GetComponent<SkinnedMeshRenderer>().material.SetColor(nameVariable, color);
		}
	}
}
