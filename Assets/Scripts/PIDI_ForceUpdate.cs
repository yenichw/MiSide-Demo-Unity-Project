using UnityEngine;

public class PIDI_ForceUpdate : MonoBehaviour
{
	public bool affectAllReflections;

	public PIDI_PlanarReflection[] reflections = new PIDI_PlanarReflection[0];

	public void Start()
	{
		if (affectAllReflections)
		{
			reflections = Object.FindObjectsOfType<PIDI_PlanarReflection>();
		}
	}

	private void OnPreRender()
	{
		Camera component = GetComponent<Camera>();
		Matrix4x4 nonJitteredProjectionMatrix = component.nonJitteredProjectionMatrix;
		component.nonJitteredProjectionMatrix = component.projectionMatrix;
		for (int i = 0; i < reflections.Length; i++)
		{
			reflections[i].OnWillRenderObject(GetComponent<Camera>());
		}
		component.nonJitteredProjectionMatrix = nonJitteredProjectionMatrix;
	}
}
