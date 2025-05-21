using UnityEngine;
using UnityEngine.UI;

public class GamesCore_Sphere : MonoBehaviour
{
	public RectTransform line;

	public Image core;

	private bool isFinish;

	private float scaleOrigin;

	private float animationScaleTime;

	[Header("Animation")]
	public AnimationCurve animationScale;

	private void Start()
	{
		scaleOrigin = base.transform.localScale.x;
		base.transform.localScale = Vector3.zero;
		if (line != null)
		{
			line.sizeDelta = new Vector2(0f, 2f);
		}
	}

	private void Update()
	{
		if (animationScaleTime < 1f)
		{
			animationScaleTime += Time.deltaTime * 5f;
			if (animationScaleTime > 1f)
			{
				animationScaleTime = 1f;
			}
			base.transform.localScale = animationScale.Evaluate(animationScaleTime) * scaleOrigin * Vector3.one;
		}
		if (line != null && (double)base.transform.localScale.x > 0.98)
		{
			line.sizeDelta = Vector2.Lerp(line.sizeDelta, new Vector2(40f, 2f), Time.deltaTime * 10f);
		}
	}

	public void StartFinish()
	{
		core.GetComponent<Image>().color = Color.white;
		core.transform.localScale = Vector3.one * 1.3f;
		isFinish = true;
	}

	public void Click()
	{
		if (!isFinish)
		{
			GetComponent<Image>().color = new Color(1f, 0f, 0.65f, 1f);
		}
	}

	public void ResetColor()
	{
		if (!isFinish)
		{
			core.color = new Color(0.15f, 0.07f, 0.3f, 1f);
		}
	}
}
