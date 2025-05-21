using UnityEngine;

public class Functions_Light : MonoBehaviour
{
	public bool canShadow = true;

	public float noise;

	public float distanceDisable = 30f;

	public Color[] colorLightRandom;

	[Header("Region color")]
	public Color[] regionColorS;

	public float regionColorRadius;

	public bool regionColorGet;

	private Light lightComponent;

	private Transform cameraPosition;

	private Vector3 positionOriginal;

	private float intensityLight;

	public void Start()
	{
		lightComponent = GetComponent<Light>();
		if (regionColorRadius == 0f && colorLightRandom.Length != 0)
		{
			lightComponent.color = colorLightRandom[Random.Range(0, colorLightRandom.Length)];
		}
		intensityLight = lightComponent.intensity;
		cameraPosition = GameObject.FindWithTag("MainCamera").transform;
		positionOriginal = base.transform.localPosition;
		ReShadow();
		if (!(regionColorRadius > 0f))
		{
			return;
		}
		if (regionColorS.Length != 0)
		{
			lightComponent.color = regionColorS[Random.Range(0, regionColorS.Length)];
		}
		Component[] array = Object.FindObjectsOfType<Functions_Light>();
		Component[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (Vector3.Distance(array2[i].transform.position, base.transform.position) < regionColorRadius && array2[i].GetComponent<Functions_Light>().regionColorGet)
			{
				array2[i].GetComponent<Light>().color = lightComponent.color;
			}
		}
	}

	private void Update()
	{
		if (Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), new Vector3(cameraPosition.position.x, 0f, cameraPosition.position.z)) < distanceDisable)
		{
			if (!lightComponent.enabled)
			{
				lightComponent.enabled = true;
				lightComponent.intensity = 0f;
			}
			lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, intensityLight, Time.deltaTime * 10f);
		}
		else
		{
			lightComponent.enabled = false;
		}
		if (noise > 0f)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, positionOriginal + new Vector3(Random.Range(0f - noise, noise), Random.Range(0f - noise, noise), Random.Range(0f - noise, noise)), Time.deltaTime);
		}
	}

	public void ReShadow()
	{
		if (canShadow)
		{
			lightComponent = GetComponent<Light>();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, regionColorRadius);
	}
}
