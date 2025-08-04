// based on the original game.Yen Chezky(yenichw)
using UnityEngine;

public class Light_LensFlare : MonoBehaviour
{
	private LensFlare lf;

	public Light lightComponent;

	public float brightness = 1f;

	private void Start()
	{
		lf = GetComponent<LensFlare>();
	}

	private void Update()
	{
		lf.brightness = lightComponent.intensity * brightness;
	}
}
