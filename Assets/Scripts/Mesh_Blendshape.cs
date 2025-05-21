using UnityEngine;

public class Mesh_Blendshape : MonoBehaviour
{
	public Mesh_BlendshapeSet[] shapes;

	public void SetBlendshape(int x)
	{
		GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(shapes[x].indexBlend, shapes[x].weight);
	}
}
