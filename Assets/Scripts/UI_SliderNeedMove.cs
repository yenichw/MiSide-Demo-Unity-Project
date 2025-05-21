using UnityEngine;
using UnityEngine.UI;

public class UI_SliderNeedMove : MonoBehaviour
{
	private bool mouseUp;

	[HideInInspector]
	public bool back;

	[HideInInspector]
	public float value;

	public RectTransform rectPoint;

	private Slider slider;

	private float move;

	private void Start()
	{
		slider = GetComponent<Slider>();
		move = slider.value;
	}

	private void Update()
	{
		if (!back)
		{
			if (slider.value > move)
			{
				move = slider.value;
			}
			value = Mathf.Lerp(value, move, Time.deltaTime * (5f * (1f + value * 1.5f)));
			rectPoint.anchoredPosition = new Vector2(0f, (0f - GetComponent<RectTransform>().sizeDelta.y) * value);
			return;
		}
		if (Input.GetMouseButtonUp(0))
		{
			mouseUp = false;
		}
		if (!mouseUp)
		{
			back = true;
			value -= Time.deltaTime * 10f;
			if (value <= 0f)
			{
				value = 0f;
			}
			slider.value = value;
			rectPoint.anchoredPosition = new Vector2(0f, (0f - GetComponent<RectTransform>().sizeDelta.y) * value);
			if (value == 0f)
			{
				move = 0f;
				back = false;
				slider.interactable = true;
			}
		}
	}

	public void ResetMove()
	{
		if (Input.GetMouseButton(0))
		{
			mouseUp = true;
		}
		back = true;
		slider.interactable = false;
	}
}
