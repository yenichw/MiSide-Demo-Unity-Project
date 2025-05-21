using UnityEngine;

public class Light_LightingColor : MonoBehaviour
{
	public AnimationCurve animColor;

	public Color[] colors;

	public float speed = 1f;

	private Color colorWas;

	private Color colorNeed;

	private float timeColor;

	private Color colorWasFog;

	private float timeColorFog;

	[Header("Туман")]
	public int indexColorFog = -1;

	public float endFogLine;

	public float startFogLine;

	private void Start()
	{
		timeColor = 1f;
		if (indexColorFog >= 0)
		{
			RenderSettings.fog = true;
			RenderSettings.fogColor = colors[indexColorFog];
			RenderSettings.fogStartDistance = startFogLine;
			RenderSettings.fogEndDistance = endFogLine;
		}
	}

	private void Update()
	{
		if (timeColor < 1f)
		{
			timeColor += Time.deltaTime;
			if (timeColor > 1f)
			{
				timeColor = 1f;
			}
			RenderSettings.ambientSkyColor = Color.Lerp(colorWas, colorNeed, animColor.Evaluate(timeColor));
		}
		if (!(timeColorFog > 0f))
		{
			return;
		}
		timeColorFog -= Time.deltaTime;
		if (timeColorFog < 0f)
		{
			timeColorFog = 0f;
		}
		if (indexColorFog == -1)
		{
			RenderSettings.fogStartDistance = Mathf.Lerp(startFogLine, 300f, animColor.Evaluate(1f - timeColorFog));
			RenderSettings.fogEndDistance = Mathf.Lerp(endFogLine, 300f, animColor.Evaluate(1f - timeColorFog));
			if (timeColorFog == 0f)
			{
				RenderSettings.fog = false;
			}
		}
		else
		{
			RenderSettings.fogStartDistance = Mathf.Lerp(300f, startFogLine, animColor.Evaluate(1f - timeColorFog));
			RenderSettings.fogEndDistance = Mathf.Lerp(300f, endFogLine, animColor.Evaluate(1f - timeColorFog));
			RenderSettings.fogColor = Color.Lerp(colorWasFog, colors[indexColorFog], animColor.Evaluate(1f - timeColorFog));
		}
	}

	public void SharplyColor(int _index)
	{
		timeColor = 1f;
		RenderSettings.ambientSkyColor = colors[_index];
	}

	public void LerpColor(int _index)
	{
		colorWas = RenderSettings.ambientSkyColor;
		colorNeed = colors[_index];
		timeColor = 0f;
	}

	public void LerpFogColor(int _index)
	{
		RenderSettings.fog = true;
		colorWasFog = RenderSettings.fogColor;
		indexColorFog = _index;
		timeColorFog = 1f;
	}
}
