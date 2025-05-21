using UnityEngine;

public class Material_ColorVariables : MonoBehaviour
{
	public GameObject[] meshes;

	public string nameVariable;

	public string nameVariableAddon;

	public Color colorChange;

	public bool onStart;

	public bool loop;

	private Color colorWas;

	private float timeAnimation;

	private float timeAnimationLoop;

	[Header("Анимация")]
	[ColorUsage(true, true)]
	public Color colorAnimation;

	[ColorUsage(true, true)]
	public Color colorAnimationLoop;

	public AnimationCurve animationColor;

	public AnimationCurve animationColorLoop;

	public float speedLoop = 1f;

	[Header("Дополнение")]
	[ColorUsage(true, true)]
	public Color[] otherColors;

	private void Start()
	{
		timeAnimation = 1f;
		if (onStart)
		{
			ColorChange();
		}
	}

	private void Update()
	{
		if (timeAnimation < 1f)
		{
			timeAnimation += Time.deltaTime;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			colorChange = Color.Lerp(colorWas, colorAnimation, animationColor.Evaluate(timeAnimation));
			ColorChange();
		}
		else if (loop)
		{
			timeAnimationLoop += Time.deltaTime * speedLoop;
			if (timeAnimationLoop > 1f)
			{
				timeAnimationLoop = 0f;
			}
			colorChange = Color.Lerp(colorAnimation, colorAnimationLoop, animationColorLoop.Evaluate(timeAnimationLoop));
			ColorChange();
		}
	}

	[ContextMenu("Установить цвет")]
	public void ColorChange()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			if (!(meshes[i] != null))
			{
				continue;
			}
			if (meshes[i].GetComponent<SkinnedMeshRenderer>() != null)
			{
				for (int j = 0; j < meshes[i].GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
				{
					meshes[i].GetComponent<SkinnedMeshRenderer>().materials[j].SetColor(nameVariable, colorChange);
					if (nameVariableAddon != null && nameVariableAddon != "")
					{
						meshes[i].GetComponent<SkinnedMeshRenderer>().materials[j].SetColor(nameVariableAddon, colorChange);
					}
				}
			}
			if (!(meshes[i].GetComponent<MeshRenderer>() != null))
			{
				continue;
			}
			for (int k = 0; k < meshes[i].GetComponent<MeshRenderer>().materials.Length; k++)
			{
				meshes[i].GetComponent<MeshRenderer>().materials[k].SetColor(nameVariable, colorChange);
				if (nameVariableAddon != null && nameVariableAddon != "")
				{
					meshes[i].GetComponent<MeshRenderer>().materials[k].SetColor(nameVariableAddon, colorChange);
				}
			}
		}
	}

	public void AnimationPlay()
	{
		colorWas = colorChange;
		timeAnimation = 0f;
	}

	public void ReColorChange(int _index)
	{
		colorAnimation = otherColors[_index];
		AnimationPlay();
	}

	public void LoopActive(bool x)
	{
		loop = x;
	}
}
