using UnityEngine;

public class FlashLight : MonoBehaviour
{
	public Light lightSpot;

	public Light lightSphere;

	private WorldPlayer _scrWP;

	private void Start()
	{
		if (GlobalTag.cameraPlayer.transform.Find("LightSphere") != null)
		{
			Object.Destroy(lightSphere.gameObject);
			lightSphere = GlobalTag.cameraPlayer.transform.Find("LightSphere").gameObject.GetComponent<Light>();
			lightSphere.transform.parent = base.transform;
			lightSphere.GetComponent<FlashLightPoint>().lightPlayer = false;
		}
		_scrWP = GlobalTag.world.GetComponent<WorldPlayer>();
		lightSpot.spotAngle = _scrWP.flashLightSpotAngle;
		lightSpot.intensity = _scrWP.flashLightIntensity;
		lightSpot.range = _scrWP.flashLightRange;
		lightSphere.intensity = _scrWP.flashLightIntensitySphere;
		lightSphere.range = _scrWP.flashLightRangeSphere;
	}

	private void Update()
	{
		lightSpot.spotAngle = Mathf.Lerp(lightSpot.spotAngle, _scrWP.flashLightSpotAngle, Time.deltaTime * _scrWP.speedLerpFlashLight);
		lightSpot.intensity = Mathf.Lerp(lightSpot.intensity, _scrWP.flashLightIntensity, Time.deltaTime * _scrWP.speedLerpFlashLight);
		lightSpot.range = Mathf.Lerp(lightSpot.range, _scrWP.flashLightRange, Time.deltaTime * _scrWP.speedLerpFlashLight);
		lightSphere.intensity = Mathf.Lerp(lightSphere.intensity, _scrWP.flashLightIntensitySphere, Time.deltaTime * _scrWP.speedLerpFlashLight);
		lightSphere.range = Mathf.Lerp(lightSphere.range, _scrWP.flashLightRangeSphere, Time.deltaTime * _scrWP.speedLerpFlashLight);
	}

	public void TakeLightPlayer()
	{
		lightSphere.transform.parent = GlobalTag.cameraPlayer.transform;
		lightSphere.name = "LightSphere";
		lightSphere.GetComponent<FlashLightPoint>().lightPlayer = true;
	}
}
