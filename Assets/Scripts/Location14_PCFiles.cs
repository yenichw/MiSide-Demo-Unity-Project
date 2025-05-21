using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Location14_PCFiles : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	private bool mouseHold;

	private Image img;

	private RectTransform rect;

	private Vector2 positionOrigin;

	private Transform parentOrigin;

	[HideInInspector]
	public int indexColor;

	public RectTransform mouse;

	public Location14_PCGames main;

	public void OnPointerDown(PointerEventData eventData)
	{
		mouseHold = true;
		base.transform.parent = mouse;
		img.raycastTarget = false;
		main.FileHold(base.gameObject);
		main.SoundPlay(main.soundFileTake);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!mouseHold)
		{
			img.color = new Color(img.color.r, img.color.g, img.color.b, 0.7f);
		}
	}

	public void StartComponent(int _indexColor)
	{
		rect = GetComponent<RectTransform>();
		img = GetComponent<Image>();
		indexColor = _indexColor;
		if (indexColor == 0)
		{
			img.color = new Color(1f, 0f, 0f, 0.7f);
		}
		if (indexColor == 1)
		{
			img.color = new Color(0f, 1f, 0f, 0.7f);
		}
		if (indexColor == 2)
		{
			img.color = new Color(0f, 0f, 1f, 0.7f);
		}
		if (indexColor == 3)
		{
			img.color = new Color(1f, 1f, 0f, 0.7f);
		}
	}

	public void SetPosition(Transform _parent, Vector2 _position)
	{
		base.transform.parent = _parent;
		parentOrigin = _parent;
		positionOrigin = _position;
	}

	private void Update()
	{
		if (mouseHold)
		{
			rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, Vector2.zero, Time.deltaTime * 10f);
			if (Input.GetMouseButtonUp(0))
			{
				mouseHold = false;
				img.color = new Color(img.color.r, img.color.g, img.color.b, 0.7f);
				base.transform.parent = parentOrigin;
				main.FileUnhold();
				img.raycastTarget = true;
				main.SoundPlay(main.soundFileDrop);
			}
		}
		else
		{
			rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, positionOrigin, Time.deltaTime * 10f);
		}
	}
}
