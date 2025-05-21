using UnityEngine;

public class Light_ShadowIntensityDistancePlayer : MonoBehaviour
{
	public float distanceMin;

	public float distanceMax;

	public Light lightObject;

	public float shadowStrengthMax;

	public float shadowStrengthMin;

	private bool inside;

	private Transform playerT;

	private void Start()
	{
		playerT = GlobalTag.player.transform;
	}

	private void Update()
	{
		if (!inside)
		{
			if (GlobalAM.DistanceFloor(base.transform.position, playerT.position) < distanceMax)
			{
				inside = true;
			}
		}
		else if (GlobalAM.DistanceFloor(base.transform.position, playerT.position) > distanceMax)
		{
			inside = false;
			lightObject.shadowStrength = shadowStrengthMax;
		}
		else if (GlobalAM.DistanceFloor(base.transform.position, playerT.position) < distanceMin)
		{
			lightObject.shadowStrength = shadowStrengthMin;
		}
		else
		{
			lightObject.shadowStrength = Mathf.Lerp(shadowStrengthMax, shadowStrengthMin, (distanceMax - GlobalAM.DistanceFloor(base.transform.position, playerT.position)) / distanceMin);
		}
	}
}
