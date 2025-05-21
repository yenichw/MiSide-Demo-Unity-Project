using UnityEngine;

public class Tamagotchi_WindowButtons : MonoBehaviour
{
	public AnimationCurve animationOpen;

	private float timeAnimationOpen;

	private RectTransform rect;

	private bool open;

	private void Update()
	{
		if (open)
		{
			if (timeAnimationOpen < 1f)
			{
				timeAnimationOpen += Time.deltaTime * 3f;
				if (timeAnimationOpen > 1f)
				{
					timeAnimationOpen = 1f;
				}
				rect.offsetMax = new Vector2(0f, 0f - (float)Screen.height * (1f - animationOpen.Evaluate(timeAnimationOpen)));
				rect.offsetMin = new Vector2(0f, 0f);
			}
		}
		else if (timeAnimationOpen < 1f)
		{
			timeAnimationOpen += Time.deltaTime * 3f;
			if (timeAnimationOpen > 1f)
			{
				timeAnimationOpen = 1f;
				base.gameObject.SetActive(value: false);
			}
			rect.offsetMax = new Vector2(0f, 0f - (float)Screen.height * animationOpen.Evaluate(timeAnimationOpen));
			rect.offsetMin = new Vector2(0f, 0f);
		}
	}

	public void Open()
	{
		base.gameObject.SetActive(value: true);
		open = true;
		timeAnimationOpen = 0f;
		rect = GetComponent<RectTransform>();
		rect.offsetMax = new Vector2(0f, -Screen.height);
		rect.offsetMin = new Vector2(0f, 0f);
	}

	public void Close()
	{
		if (open)
		{
			rect = GetComponent<RectTransform>();
			open = false;
			timeAnimationOpen = 0f;
		}
	}
}
