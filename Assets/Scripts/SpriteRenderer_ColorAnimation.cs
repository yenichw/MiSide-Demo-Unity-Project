using UnityEngine;

public class SpriteRenderer_ColorAnimation : MonoBehaviour
{
	private SpriteRenderer spr;

	private Color colorWas;

	private Color colorNeed;

	[Header("Анимация")]
	public AnimationCurve animationColor;

	private float timeAnimationColor;

	public float animationSpeed = 1f;

	public bool fastStart;

	public bool materialColor;

	private bool destroy;

	private void OnEnable()
	{
		spr = GetComponent<SpriteRenderer>();
		if (fastStart)
		{
			timeAnimationColor = 1f;
			return;
		}
		colorNeed = spr.color;
		colorWas = new Color(spr.color.r, spr.color.g, spr.color.b, 0f);
		spr.color = colorWas;
		if (materialColor)
		{
			spr.material.SetColor("_Color", spr.color);
		}
	}

	private void Update()
	{
		if (!(timeAnimationColor < 1f))
		{
			return;
		}
		timeAnimationColor += Time.deltaTime * animationSpeed;
		if (timeAnimationColor >= 1f)
		{
			timeAnimationColor = 1f;
			if (destroy)
			{
				Object.Destroy(base.gameObject);
			}
		}
		spr.color = Color.Lerp(colorWas, colorNeed, animationColor.Evaluate(timeAnimationColor));
		if (materialColor)
		{
			spr.material.SetColor("_Color", spr.color);
		}
	}

	public void ColorAnimationDestroy()
	{
		timeAnimationColor = 0f;
		colorWas = spr.color;
		if (materialColor)
		{
			spr.material.SetColor("_Color", spr.color);
		}
		colorNeed = new Color(spr.color.r, spr.color.g, spr.color.b, 0f);
		destroy = true;
	}

	public void DestroyObject()
	{
		Object.Destroy(base.gameObject);
	}
}
