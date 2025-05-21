using UnityEngine;

public class Material_ColorAnimationLoop : MonoBehaviour
{
	public GameObject[] meshes;

	public string nameVariable;

	private float timeAnimation;

	[Header("Анимация")]
	public float speed = 1f;

	public Color colorAnimationA;

	public Color colorAnimationB;

	public AnimationCurve animationColor;

	public bool order;

	[Header("Стартовая анимация")]
	public AnimationCurve animationCurveStart;

	public Color colorAnimationStart;

	public bool colorStart;

	private int indexOrder;

	private Color colorAnim;

	private Color colorAnimEnd;

	private AnimationCurve curveAnim;

	private void Update()
	{
		timeAnimation += Time.deltaTime * speed;
		if (timeAnimation > 1f)
		{
			timeAnimation = 0f;
			if (order)
			{
				NextOrder();
			}
			else if (colorStart)
			{
				colorStart = false;
			}
		}
		ColorChange();
	}

	private void NextOrder()
	{
		indexOrder++;
		if (indexOrder > meshes.Length - 1)
		{
			indexOrder = 0;
			if (colorStart)
			{
				colorStart = false;
			}
		}
		int num = meshes.Length;
		while (meshes[indexOrder] == null)
		{
			indexOrder++;
			if (indexOrder > meshes.Length - 1)
			{
				indexOrder = 0;
				if (colorStart)
				{
					colorStart = false;
				}
			}
			num--;
			if (num == 0)
			{
				Object.Destroy(this);
				break;
			}
		}
	}

	private void OnEnable()
	{
		if (order)
		{
			order = false;
			ColorChange();
			order = true;
		}
		else
		{
			ColorChange();
		}
	}

	public void ColorChange()
	{
		if (colorStart)
		{
			colorAnim = colorAnimationStart;
			colorAnimEnd = colorAnimationA;
			curveAnim = animationCurveStart;
		}
		else
		{
			colorAnim = colorAnimationA;
			colorAnimEnd = colorAnimationB;
			curveAnim = animationColor;
		}
		if (!order)
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
						meshes[i].GetComponent<SkinnedMeshRenderer>().materials[j].SetColor(nameVariable, Color.Lerp(colorAnim, colorAnimEnd, curveAnim.Evaluate(timeAnimation)));
					}
				}
				if (meshes[i].GetComponent<MeshRenderer>() != null)
				{
					for (int k = 0; k < meshes[i].GetComponent<MeshRenderer>().materials.Length; k++)
					{
						meshes[i].GetComponent<MeshRenderer>().materials[k].SetColor(nameVariable, Color.Lerp(colorAnim, colorAnimEnd, curveAnim.Evaluate(timeAnimation)));
					}
				}
			}
		}
		else
		{
			if (!(meshes[indexOrder] != null))
			{
				return;
			}
			if (meshes[indexOrder].GetComponent<SkinnedMeshRenderer>() != null)
			{
				for (int l = 0; l < meshes[indexOrder].GetComponent<SkinnedMeshRenderer>().materials.Length; l++)
				{
					meshes[indexOrder].GetComponent<SkinnedMeshRenderer>().materials[l].SetColor(nameVariable, Color.Lerp(colorAnim, colorAnimEnd, curveAnim.Evaluate(timeAnimation)));
				}
			}
			if (meshes[indexOrder].GetComponent<MeshRenderer>() != null)
			{
				for (int m = 0; m < meshes[indexOrder].GetComponent<MeshRenderer>().materials.Length; m++)
				{
					meshes[indexOrder].GetComponent<MeshRenderer>().materials[m].SetColor(nameVariable, Color.Lerp(colorAnim, colorAnimEnd, curveAnim.Evaluate(timeAnimation)));
				}
			}
		}
	}
}
