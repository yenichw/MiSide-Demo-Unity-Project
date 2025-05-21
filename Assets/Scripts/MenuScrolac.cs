using UnityEngine;

public class MenuScrolac : MonoBehaviour
{
	public RectTransform rectChange;

	public RectTransform scrollView;

	private RectTransform rect;

	private float timeScrolac;

	public bool changeCopyRect;

	private void Update()
	{
		if (timeScrolac > 0f)
		{
			timeScrolac -= Time.fixedDeltaTime;
			if (timeScrolac < 0f)
			{
				timeScrolac = 0f;
			}
			if (rect.anchoredPosition.y > 0f - rectChange.anchoredPosition.y)
			{
				rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, new Vector2(0f, 0f - rectChange.anchoredPosition.y - 10f), Time.unscaledDeltaTime * 15f);
			}
			if (0f - rectChange.anchoredPosition.y > rect.anchoredPosition.y + scrollView.sizeDelta.y - 55f)
			{
				rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, new Vector2(0f, 0f - (scrollView.sizeDelta.y + rectChange.anchoredPosition.y - 80f)), Time.unscaledDeltaTime * 15f);
			}
		}
	}

	public void UpdateScrolac()
	{
		timeScrolac = 1f;
	}

	private void OnEnable()
	{
		rect = GetComponent<RectTransform>();
		UpdateScrolac();
	}
}
