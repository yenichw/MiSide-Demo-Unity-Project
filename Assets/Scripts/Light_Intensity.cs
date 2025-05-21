using UnityEngine;

public class Light_Intensity : MonoBehaviour
{
	private Light lg;

	[Header("Свет")]
	public float intensity;

	public AnimationCurve animationLerp;

	private float animationLerpTime;

	private float intensityWas;

	public float speedLerp = 1f;

	private float animationNoiseTime;

	[Header("Шум")]
	public AnimationCurve animationNoise;

	public bool playNoise;

	private int indexColorNeed;

	private float timeColor;

	private Color colorWas;

	[Header("Цвета")]
	public Color[] colorsLight;

	private bool fs;

	private void Start()
	{
		if (!fs)
		{
			fs = true;
			lg = GetComponent<Light>();
			animationLerpTime = 1f;
		}
	}

	private void Update()
	{
		if (playNoise)
		{
			animationNoiseTime += Time.deltaTime;
			if (animationNoiseTime > 1f)
			{
				animationNoiseTime -= 1f;
			}
			lg.intensity = intensity * animationNoise.Evaluate(animationNoiseTime);
		}
		if (animationLerpTime < 1f)
		{
			animationLerpTime += Time.deltaTime * speedLerp;
			if (animationLerpTime > 1f)
			{
				animationLerpTime = 1f;
			}
			lg.intensity = Mathf.Lerp(intensityWas, intensity, animationLerp.Evaluate(animationLerpTime));
			if (lg.intensity <= 0f)
			{
				lg.enabled = false;
			}
		}
		if (timeColor > 0f)
		{
			timeColor -= Time.deltaTime;
			if (timeColor < 0f)
			{
				timeColor = 0f;
			}
			lg.color = Color.Lerp(colorWas, colorsLight[indexColorNeed], 1f - timeColor);
		}
	}

	public void NoisePlay(bool _x)
	{
		if (!fs)
		{
			Start();
		}
		playNoise = _x;
		if (!playNoise)
		{
			lg.intensity = intensity;
		}
		else
		{
			animationNoiseTime = Random.Range(0f, 1f);
		}
	}

	public void Intensity(float _x)
	{
		intensity = _x;
		GetComponent<Light>().intensity = intensity;
		if (_x == 0f)
		{
			GetComponent<Light>().enabled = false;
		}
		else
		{
			GetComponent<Light>().enabled = true;
		}
	}

	public void IntenityLerp(float _x)
	{
		if (!fs)
		{
			Start();
		}
		animationLerpTime = 0f;
		intensityWas = lg.intensity;
		intensity = _x;
		if (_x > 0f)
		{
			GetComponent<Light>().enabled = true;
		}
	}

	public void ColorLerpStart(int _indexColor)
	{
		if (lg == null)
		{
			Start();
		}
		colorWas = lg.color;
		timeColor = 1f;
		indexColorNeed = _indexColor;
	}

	public void ColorSharp(int _indexColor)
	{
		timeColor = 0f;
		lg.color = colorsLight[indexColorNeed];
	}
}
