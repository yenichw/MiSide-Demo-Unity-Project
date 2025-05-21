using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

public class TetrisFrog : MonoBehaviour
{
	public UIFlip frog;

	public AnimationCurve animationJump;

	public Sprite spriteJump;

	public Sprite spriteFall;

	private float timeJump;

	private float timeAnimationJump;

	private RectTransform frogRect;

	private RectTransform rect;

	private float speed;

	public void StartComponent(int _score)
	{
		timeAnimationJump = 1f;
		frogRect = frog.GetComponent<RectTransform>();
		rect = GetComponent<RectTransform>();
		speed = 1f + 0.1f * (float)_score;
	}

	private void Update()
	{
		timeJump += Time.deltaTime;
		if (timeJump > 2f)
		{
			timeJump = 0f;
			timeAnimationJump = 0f;
			frog.GetComponent<TetrisSpriteAnimation>().AnimationStop();
			frog.GetComponent<Image>().sprite = spriteJump;
		}
		if (timeAnimationJump < 1f)
		{
			timeAnimationJump += Time.deltaTime * 1.5f;
			if (timeAnimationJump > 1f)
			{
				timeAnimationJump = 1f;
				frog.GetComponent<TetrisSpriteAnimation>().AnimationPlay();
				frog.GetComponent<Image>().sprite = spriteFall;
			}
			frogRect.anchoredPosition = new Vector2(0f, animationJump.Evaluate(timeAnimationJump) * 15f);
			rect.anchoredPosition -= rect.anchoredPosition.normalized * (Time.deltaTime * 30f * speed);
		}
	}

	public void Flip(bool x)
	{
		frog.horizontal = x;
	}
}
