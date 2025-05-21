using UnityEngine;

public class Light_Addone : MonoBehaviour
{
	public Light lightMain;

	public float intensity = 1f;

	private Light lg;

	private void Start()
	{
		lg = GetComponent<Light>();
	}

	private void Update()
	{
		lg.intensity = Mathf.Lerp(lg.intensity, lightMain.intensity * intensity, Time.deltaTime * 5f);
		if (lg.intensity == 0f && lg.enabled)
		{
			lg.enabled = false;
		}
		if (!lg.enabled && lg.intensity > 0f)
		{
			lg.enabled = true;
		}
	}
}
