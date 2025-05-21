using UnityEngine;

public class SpriteRenderer_SmoothDestroy : MonoBehaviour
{
	public float speedAlpha = 0.1f;

	[Header("Alpha start")]
	public bool alphaStart;

	[Range(0f, 1f)]
	public float alphaStartIntensity = 1f;

	public float alphaStartSpeed = 3f;

	private SpriteRenderer sr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		if (alphaStart)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
		}
	}

	private void Update()
	{
		if (!alphaStart)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a - Time.deltaTime * speedAlpha);
			if (sr.color.a <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			return;
		}
		sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a + Time.deltaTime * alphaStartSpeed);
		if (sr.color.a >= alphaStartIntensity || sr.color.a >= 1f)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alphaStartIntensity);
			alphaStart = false;
		}
	}
}
