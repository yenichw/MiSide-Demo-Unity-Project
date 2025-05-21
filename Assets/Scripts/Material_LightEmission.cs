using UnityEngine;

public class Material_LightEmission : MonoBehaviour
{
	public Light lightMain;

	public Vector3 colorMin;

	public Vector3 colorMax;

	public GameObject mesh;

	private Vector4 colorNow;

	private void Update()
	{
		if (mesh.GetComponent<MeshRenderer>() != null)
		{
			if (lightMain.gameObject.activeInHierarchy)
			{
				colorNow = new Color(colorMin.x + (colorMax.x - colorMin.x) * lightMain.intensity, colorMin.y + (colorMax.y - colorMin.y) * lightMain.intensity, colorMin.z + (colorMax.z - colorMin.z) * lightMain.intensity, 1f);
			}
			else
			{
				colorNow = Color.Lerp(colorNow, new Color(colorMin.x, colorMin.y, colorMin.z, 1f), Time.deltaTime * 5f);
			}
			mesh.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", colorNow);
		}
	}
}
