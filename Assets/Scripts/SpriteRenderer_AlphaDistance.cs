using UnityEngine;

public class SpriteRenderer_AlphaDistance : MonoBehaviour
{
	public float distanceMin;

	public float distanceMax;

	public AnimationCurve alphaDistance;

	private SpriteRenderer spr;

	private Transform player;

	private bool destroySmooth;

	private float distanceNow;

	private void Start()
	{
		player = GlobalTag.cameraPlayer.transform;
		spr = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (!destroySmooth)
		{
			distanceNow = Vector3.Distance(player.position, base.transform.position);
			if (distanceNow > distanceMin && distanceNow < distanceMax)
			{
				distanceNow = alphaDistance.Evaluate((distanceNow - distanceMin) / (distanceMax - distanceMin));
			}
			else
			{
				distanceNow = 0f;
			}
			spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, distanceNow);
		}
		else
		{
			spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, spr.color.a - Time.deltaTime);
			if (spr.color.a <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void DestroySmooth()
	{
		destroySmooth = true;
	}
}
