using UnityEngine;
using UnityEngine.UI;

public class ButtonMouseClickBubble : MonoBehaviour
{
	public AnimationCurve animationClick;

	private Shadow imageShadow;

	private Vector2 positionOrigin;

	private float positionShadowY;

	private RectTransform rect;

	private float timeAnimation;

	public void Start()
	{
		imageShadow = GetComponent<Shadow>();
		rect = GetComponent<RectTransform>();
		positionOrigin = rect.anchoredPosition;
		positionShadowY = imageShadow.effectDistance.y;
		timeAnimation = 1f;
	}

	private void Update()
	{
		if (timeAnimation < 1f)
		{
			timeAnimation += Time.unscaledDeltaTime * 10f;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			rect.anchoredPosition = positionOrigin + new Vector2(0f, animationClick.Evaluate(timeAnimation) * positionShadowY);
			imageShadow.effectDistance = new Vector2(imageShadow.effectDistance.x, positionShadowY - animationClick.Evaluate(timeAnimation) * positionShadowY);
		}
	}

	public void Click()
	{
		timeAnimation = 0f;
	}

	public void SetAnchored(Vector2 _pos)
	{
		rect.anchoredPosition = _pos;
		positionOrigin = _pos;
	}

	public void SetAnchoredLerp(Vector2 _pos, float _time)
	{
		rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, _pos, _time);
		positionOrigin = Vector2.Lerp(positionOrigin, _pos, _time);
	}

	public Vector2 GetAnchored()
	{
		return rect.anchoredPosition;
	}
}
