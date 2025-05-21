using UnityEngine;

public class FlashLight_World : MonoBehaviour
{
	public float speedLerp;

	public float flashLightIntensity = 0.7f;

	public float flashLightSpotAngle = 40f;

	public float flashLightRange = 30f;

	public float flashLightRangeSphere = 2f;

	public float flashLightIntensitySphere = 0.25f;

	public void Click()
	{
		GlobalTag.world.GetComponent<WorldPlayer>().FlashLightRechange(this);
	}
}
