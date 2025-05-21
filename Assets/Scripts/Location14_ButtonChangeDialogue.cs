using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Location14_ButtonChangeDialogue : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	public Location14_GameQuestPlayer main;

	public bool sideChange;

	public Text text;

	private Image img;

	private bool mouse;

	private bool active;

	private void Update()
	{
		if (active)
		{
			if (text.color.a < 1f)
			{
				text.color = new Color(1f, 1f, 1f, text.color.a + Time.deltaTime * 4f);
				if (text.color.a > 1f)
				{
					text.color = new Color(1f, 1f, 1f, 1f);
				}
			}
			if (!mouse)
			{
				if ((double)img.color.a < 0.7)
				{
					img.color = new Color(1f, 1f, 1f, img.color.a + Time.deltaTime * 4f);
					if ((double)img.color.a > 0.7)
					{
						img.color = new Color(1f, 1f, 1f, 0.7f);
					}
				}
				if ((double)img.color.a > 0.7)
				{
					img.color = new Color(1f, 1f, 1f, img.color.a - Time.deltaTime * 4f);
					if ((double)img.color.a < 0.7)
					{
						img.color = new Color(1f, 1f, 1f, 0.7f);
					}
				}
			}
			else if ((double)img.color.a > 0.9)
			{
				img.color = new Color(1f, 1f, 1f, img.color.a - Time.deltaTime);
				if ((double)img.color.a < 0.9)
				{
					img.color = new Color(1f, 1f, 1f, 0.9f);
				}
			}
			return;
		}
		if (text.color.a > 0f)
		{
			text.color = new Color(1f, 1f, 1f, text.color.a - Time.deltaTime * 8f);
			if (text.color.a < 0f)
			{
				text.color = new Color(1f, 1f, 1f, 0f);
			}
		}
		img.color = new Color(1f, 1f, 1f, img.color.a - Time.deltaTime * 4f);
		if (img.color.a < 0f)
		{
			img.color = new Color(1f, 1f, 1f, 0f);
			base.gameObject.SetActive(value: false);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		main.ChangeDialogue(sideChange);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (active)
		{
			img.color = new Color(1f, 1f, 1f, 1f);
			mouse = true;
			main.au.clip = main.soundEnterChange;
			main.au.pitch = Random.Range(0.95f, 1.05f);
			main.au.Play();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (active)
		{
			img.color = new Color(1f, 1f, 1f, 0.9f);
			mouse = false;
		}
	}

	public void Activation(string _text)
	{
		base.gameObject.SetActive(value: true);
		text.text = _text;
		active = true;
		img = GetComponent<Image>();
		img.color = new Color(1f, 1f, 1f, 0f);
		text.color = new Color(1f, 1f, 1f, 0f);
	}

	public void Deactivation()
	{
		active = false;
	}
}
