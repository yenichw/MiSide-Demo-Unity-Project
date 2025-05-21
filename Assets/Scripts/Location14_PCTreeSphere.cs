using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Location14_PCTreeSphere : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public Location14_PCGames main;

	public Image imgSub;

	public Image mouseClick;

	public AnimationCurve animationAlphaClick;

	private float timeAnimationClick;

	[HideInInspector]
	public bool ready;

	private Image img;

	private float x;

	private float y;

	private float z;

	private RectTransform rect;

	public void StartComponent()
	{
		ready = false;
		rect = GetComponent<RectTransform>();
		img = GetComponent<Image>();
		x = Random.Range(0f, 1f);
		y = Random.Range(0f, 1f);
		z = Random.Range(0f, 1f);
		mouseClick.gameObject.SetActive(value: false);
	}

	public void UpdateCircle()
	{
		if (!ready)
		{
			float num = 0.2f;
			if (main.treeSliderComponentX.value > x + num || main.treeSliderComponentX.value < x - num)
			{
				if (main.treeSliderComponentX.value > x + num)
				{
					img.fillAmount = (1f - main.treeSliderComponentX.value) / (1f - (x + num));
				}
				if (main.treeSliderComponentX.value < x - num)
				{
					img.fillAmount = main.treeSliderComponentX.value / (x - num);
				}
			}
			else
			{
				img.fillAmount = 1f;
			}
			if (main.treeSliderComponentZ.value > z + num || main.treeSliderComponentZ.value < z - num)
			{
				if (main.treeSliderComponentZ.value > z + num)
				{
					rect.sizeDelta = Vector2.one * ((1f - main.treeSliderComponentZ.value) / (1f - (z + num)) * 100f);
				}
				if (main.treeSliderComponentZ.value < z - num)
				{
					rect.sizeDelta = Vector2.one * (main.treeSliderComponentZ.value / (z - num)) * 100f;
				}
			}
			else
			{
				rect.sizeDelta = Vector2.one * 100f;
			}
			if (main.treeSliderComponentY.value > y + num || main.treeSliderComponentY.value < y - num)
			{
				if (main.treeSliderComponentY.value > y + num)
				{
					img.color = new Color(0f, 1f, 0f, (1f - main.treeSliderComponentY.value) / (1f - (y + num)));
				}
				if (main.treeSliderComponentY.value < y - num)
				{
					img.color = new Color(0f, 1f, 0f, main.treeSliderComponentY.value / (y - num));
				}
			}
			else
			{
				img.color = new Color(0f, 1f, 0f, 1f);
			}
			if (img.color == new Color(0f, 1f, 0f, 1f) && rect.sizeDelta == Vector2.one * 100f && img.fillAmount == 1f)
			{
				imgSub.color = new Color(0f, 1f, 0f, 1f);
				if (!mouseClick.gameObject.activeSelf)
				{
					main.SoundPlay(main.soundCircleCan);
					mouseClick.gameObject.SetActive(value: true);
				}
			}
			else
			{
				imgSub.color = new Color(1f, 0f, 0f, 1f);
				if (mouseClick.gameObject.activeSelf)
				{
					main.SoundPlay(main.soundCircleNoCan);
					mouseClick.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			mouseClick.gameObject.SetActive(value: false);
			img.color = new Color(1f, 1f, 1f, 1f);
			img.fillAmount = 1f;
			imgSub.color = new Color(0f, 0f, 0f, 0f);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!ready)
		{
			if (imgSub.color == new Color(0f, 1f, 0f, 1f))
			{
				ready = true;
				main.SliderUpdate();
				main.SoundPlay(main.soundCircleApply);
			}
			else
			{
				main.SoundPlay(main.soundCircleNotApply);
			}
		}
	}

	private void Update()
	{
		if (mouseClick.gameObject.activeSelf)
		{
			timeAnimationClick += Time.deltaTime * 2f;
			if (timeAnimationClick > 1f)
			{
				timeAnimationClick = 0f;
			}
			mouseClick.color = new Color(0f, 0f, 0f, animationAlphaClick.Evaluate(timeAnimationClick));
		}
	}
}
