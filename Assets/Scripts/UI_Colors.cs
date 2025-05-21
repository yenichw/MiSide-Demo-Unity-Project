using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Colors : MonoBehaviour
{
	public bool startInvisible;

	public bool onEnableInvisible;

	public bool deactive;

	public float speed = 20f;

	public UnityEvent eventDestroy;

	public bool autoFindUI = true;

	private bool hide;

	private bool fs;

	private float share;

	private bool destroyMe;

	[Header("Информация")]
	public List<Image> ui_images;

	public List<Color> ui_imagesColor;

	public List<Text> ui_text;

	public List<Color> ui_textColor;

	private void Start()
	{
		if (fs || deactive)
		{
			return;
		}
		fs = true;
		share = 1f;
		if (autoFindUI)
		{
			Component[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
			Component[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				ui_text.Add(array[i].GetComponent<Text>());
				if (array[i].GetComponent<UI_ColorsSave>() == null)
				{
					ui_textColor.Add(array[i].GetComponent<Text>().color);
					if (startInvisible)
					{
						array[i].GetComponent<Text>().color = new Color(array[i].GetComponent<Text>().color.r, array[i].GetComponent<Text>().color.g, array[i].GetComponent<Text>().color.b, 0f);
					}
				}
				else
				{
					ui_textColor.Add(array[i].GetComponent<UI_ColorsSave>().GetColor());
				}
			}
			componentsInChildren = base.transform.GetComponentsInChildren<Image>();
			array = componentsInChildren;
			for (int j = 0; j < array.Length; j++)
			{
				ui_images.Add(array[j].GetComponent<Image>());
				if (array[j].GetComponent<UI_ColorsSave>() == null)
				{
					ui_imagesColor.Add(array[j].GetComponent<Image>().color);
					if (startInvisible)
					{
						array[j].GetComponent<Image>().color = new Color(array[j].GetComponent<Image>().color.r, array[j].GetComponent<Image>().color.g, array[j].GetComponent<Image>().color.b, 0f);
					}
				}
				else
				{
					ui_imagesColor.Add(array[j].GetComponent<UI_ColorsSave>().GetColor());
				}
			}
			return;
		}
		for (int k = 0; k < ui_text.Count; k++)
		{
			if (startInvisible)
			{
				ui_text[k].color = new Color(ui_text[k].color.r, ui_text[k].color.g, ui_text[k].color.b, 0f);
			}
		}
		for (int l = 0; l < ui_images.Count; l++)
		{
			if (startInvisible)
			{
				ui_images[l].color = new Color(ui_images[l].color.r, ui_images[l].color.g, ui_images[l].color.b, 0f);
			}
		}
	}

	private void Update()
	{
		if (deactive)
		{
			return;
		}
		if (hide)
		{
			bool flag = true;
			for (int i = 0; i < ui_text.Count; i++)
			{
				if (ui_text[i] != null)
				{
					ui_text[i].color = Color.Lerp(ui_text[i].color, new Color(ui_textColor[i].r, ui_textColor[i].g, ui_textColor[i].b, 0f), Time.unscaledDeltaTime * speed);
					if ((double)ui_text[i].color.a > 0.01)
					{
						flag = false;
					}
				}
			}
			for (int j = 0; j < ui_images.Count; j++)
			{
				if (ui_images[j] != null)
				{
					ui_images[j].color = Color.Lerp(ui_images[j].color, new Color(ui_imagesColor[j].r, ui_imagesColor[j].g, ui_imagesColor[j].b, 0f), Time.unscaledDeltaTime * speed);
					if ((double)ui_images[j].color.a > 0.01)
					{
						flag = false;
					}
				}
			}
			if (destroyMe && flag)
			{
				eventDestroy.Invoke();
				Object.Destroy(base.gameObject);
			}
			return;
		}
		for (int k = 0; k < ui_text.Count; k++)
		{
			if (ui_text[k] != null)
			{
				ui_text[k].color = Color.Lerp(ui_text[k].color, new Color(ui_textColor[k].r, ui_textColor[k].g, ui_textColor[k].b, ui_textColor[k].a / share), Time.unscaledDeltaTime * speed);
			}
		}
		for (int l = 0; l < ui_images.Count; l++)
		{
			if (ui_images[l] != null)
			{
				ui_images[l].color = Color.Lerp(ui_images[l].color, new Color(ui_imagesColor[l].r, ui_imagesColor[l].g, ui_imagesColor[l].b, ui_imagesColor[l].a / share), Time.unscaledDeltaTime * speed);
			}
		}
	}

	private void OnDisable()
	{
		if (destroyMe)
		{
			eventDestroy.Invoke();
			Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (onEnableInvisible)
		{
			for (int i = 0; i < ui_text.Count; i++)
			{
				ui_text[i].color = new Color(ui_text[i].color.r, ui_text[i].color.g, ui_text[i].color.b, 0f);
			}
			for (int j = 0; j < ui_images.Count; j++)
			{
				ui_images[j].color = new Color(ui_images[j].color.r, ui_images[j].color.g, ui_images[j].color.b, 0f);
			}
		}
	}

	public void Hide(bool x, bool _fast)
	{
		if (deactive)
		{
			return;
		}
		if (!fs)
		{
			Start();
		}
		hide = x;
		if (!_fast)
		{
			return;
		}
		if (x)
		{
			for (int i = 0; i < ui_text.Count; i++)
			{
				if (ui_text[i] != null)
				{
					ui_text[i].color = new Color(ui_textColor[i].r, ui_textColor[i].g, ui_textColor[i].b, 0f);
				}
			}
			for (int j = 0; j < ui_images.Count; j++)
			{
				if (ui_images[j] != null)
				{
					ui_images[j].color = new Color(ui_imagesColor[j].r, ui_imagesColor[j].g, ui_imagesColor[j].b, 0f);
				}
			}
			return;
		}
		for (int k = 0; k < ui_text.Count; k++)
		{
			if (ui_text[k] != null)
			{
				ui_text[k].color = ui_textColor[k];
			}
		}
		for (int l = 0; l < ui_images.Count; l++)
		{
			if (ui_images[l] != null)
			{
				ui_images[l].color = ui_imagesColor[l];
			}
		}
	}

	public void Hide(bool x)
	{
		if (!deactive)
		{
			if (!fs)
			{
				Start();
			}
			hide = x;
		}
	}

	public void SetColorImage(int _index, Color _color)
	{
		if (!deactive)
		{
			if (!fs)
			{
				Start();
			}
			ui_imagesColor[_index] = _color;
		}
	}

	public void SetColorImage(GameObject _object, Color _color)
	{
		if (deactive)
		{
			return;
		}
		if (!fs)
		{
			Start();
		}
		for (int i = 0; i < ui_images.Count; i++)
		{
			if (ui_images[i].gameObject == _object)
			{
				ui_imagesColor[i] = _color;
				break;
			}
		}
	}

	public void ShareAlpha(float x)
	{
		if (!deactive)
		{
			share = x;
		}
	}

	public void DestroyHide()
	{
		Hide(x: true, _fast: false);
		destroyMe = true;
	}

	public void Reload()
	{
		if (ui_text != null)
		{
			ui_text.Clear();
		}
		if (ui_textColor != null)
		{
			ui_textColor.Clear();
		}
		if (ui_images != null)
		{
			ui_images.Clear();
		}
		if (ui_imagesColor != null)
		{
			ui_imagesColor.Clear();
		}
		fs = false;
		Start();
	}
}
