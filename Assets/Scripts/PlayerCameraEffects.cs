using Colorful;
using Kino;
using UnityEngine;

public class PlayerCameraEffects : MonoBehaviour
{
	public Camera cameraPerson;

	[Header("Эффекты")]
	public GaussianBlur fxGaussian;

	public Glitch fxGlitch;

	public FastVignette fxFastVignette;

	public Noise fxNoise;

	public AnalogTV fxAnalogTV;

	public Datamosh datamosh;

	private int datamoshGlitch;

	private bool fxFV;

	private float fxNoiseInensity;

	private float timeUpdateCamera;

	private bool enableTV;

	private Camera cameraMe;

	private void Start()
	{
		cameraMe = GetComponent<Camera>();
	}

	private void Update()
	{
		if (fxFV)
		{
			if (fxFastVignette.Darkness < 75f)
			{
				fxFastVignette.Darkness += Time.deltaTime * 100f;
				if (fxFastVignette.Darkness > 75f)
				{
					fxFastVignette.Darkness = 75f;
				}
			}
		}
		else
		{
			fxFastVignette.Darkness -= Time.deltaTime * 100f;
			if (fxFastVignette.Darkness < 0f)
			{
				fxFastVignette.Darkness = 0f;
				fxFastVignette.enabled = false;
			}
		}
		if (fxNoise.Strength > fxNoiseInensity)
		{
			fxNoise.Strength -= Time.deltaTime;
			if (fxNoise.Strength < fxNoiseInensity)
			{
				fxNoise.Strength = fxNoiseInensity;
				if ((double)fxNoise.Strength < 0.01)
				{
					fxNoise.enabled = false;
				}
			}
		}
		if (fxNoise.Strength < fxNoiseInensity)
		{
			fxNoise.Strength += Time.deltaTime;
			if (fxNoise.Strength > fxNoiseInensity)
			{
				fxNoise.Strength = fxNoiseInensity;
			}
		}
		if (fxAnalogTV.enabled)
		{
			if (enableTV)
			{
				if ((double)fxAnalogTV.Scale > 0.85)
				{
					fxAnalogTV.Scale -= Time.deltaTime * 0.005f;
					if ((double)fxAnalogTV.Scale < 0.85)
					{
						fxAnalogTV.Scale = 0.85f;
					}
				}
				if ((double)fxAnalogTV.Distortion < 0.2)
				{
					fxAnalogTV.Distortion += Time.deltaTime * 0.005f;
					if ((double)fxAnalogTV.Distortion > 0.2)
					{
						fxAnalogTV.Distortion = 0.2f;
					}
					fxAnalogTV.CubicDistortion = fxAnalogTV.Distortion;
				}
			}
			else
			{
				if (fxAnalogTV.Scale < 1f)
				{
					fxAnalogTV.Scale += Time.deltaTime * 0.1f;
					if (fxAnalogTV.Scale > 1f)
					{
						fxAnalogTV.Scale = 1f;
					}
				}
				if (fxAnalogTV.Distortion > 0f)
				{
					fxAnalogTV.Distortion -= Time.deltaTime * 0.1f;
					if (fxAnalogTV.Distortion < 0f)
					{
						fxAnalogTV.Distortion = 0f;
					}
					fxAnalogTV.CubicDistortion = fxAnalogTV.Distortion;
				}
				if (fxAnalogTV.Distortion == 0f && fxAnalogTV.Scale == 1f)
				{
					fxAnalogTV.enabled = false;
				}
			}
		}
		timeUpdateCamera += Time.deltaTime;
		if (timeUpdateCamera > 1f)
		{
			timeUpdateCamera = 0f;
			UpdateCameraPerson();
		}
	}

	private void LateUpdate()
	{
		if (datamoshGlitch > 0)
		{
			datamoshGlitch--;
			if (datamoshGlitch == 0)
			{
				datamosh.Glitch();
			}
		}
	}

	public void UpdateCameraPerson()
	{
		cameraMe = GetComponent<Camera>();
		cameraPerson.orthographic = cameraMe.orthographic;
		cameraPerson.orthographicSize = cameraMe.orthographicSize;
		cameraPerson.fieldOfView = cameraMe.fieldOfView;
		cameraPerson.nearClipPlane = cameraMe.nearClipPlane;
		cameraPerson.farClipPlane = cameraMe.farClipPlane;
	}

	public void FastVegnetteActive(bool x)
	{
		fxFV = x;
		if (x && !fxFastVignette.enabled)
		{
			fxFastVignette.enabled = true;
		}
	}

	public void EffectNoise(float _intensity)
	{
		fxNoiseInensity = _intensity;
		if (fxNoiseInensity > 0f)
		{
			fxNoise.enabled = true;
		}
	}

	public void EffectFishEye(bool _active)
	{
		enableTV = _active;
		if (_active)
		{
			fxAnalogTV.enabled = true;
		}
	}

	public void EffectDatamosh(bool x)
	{
		datamosh.enabled = x;
		if (x)
		{
			datamoshGlitch = 2;
		}
	}
}
