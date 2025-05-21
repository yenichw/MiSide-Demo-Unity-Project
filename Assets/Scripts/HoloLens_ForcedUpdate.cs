using UnityEngine;

public class HoloLens_ForcedUpdate : MonoBehaviour
{
	public PIDI_PlanarReflection[] reflections = new PIDI_PlanarReflection[0];

	public void OnPreRender()
	{
		for (int i = 0; i < reflections.Length; i++)
		{
			reflections[i].OnWillRenderObject(GetComponent<Camera>());
		}
	}
}
