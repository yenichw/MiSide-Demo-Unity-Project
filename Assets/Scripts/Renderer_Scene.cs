using UnityEngine;

public class Renderer_Scene : MonoBehaviour
{
	[Range(0f, 100f)]
	public float speed = 1f;

	public bool active;

	[Header("Camera")]
	public Color colorCamera;

	public Camera cameraMain;

	[Header("Background")]
	public Color colorBackground;

	[Header("Fog")]
	public Color colorFog;

	public float fogStart;

	public float fogEnd;

	private void Update()
	{
		if (active)
		{
			cameraMain.backgroundColor = Color.Lerp(cameraMain.backgroundColor, colorCamera, Time.deltaTime * speed);
			RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, colorFog, Time.deltaTime * speed);
			RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, fogStart, Time.deltaTime * speed);
			RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, fogEnd, Time.deltaTime * speed);
			RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, colorBackground, Time.deltaTime * speed);
		}
	}

	public void Activation()
	{
		active = true;
	}
}
