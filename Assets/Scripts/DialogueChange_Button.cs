using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueChange_Button : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	public Image img;

	public Text txt;

	public Image imgFrameIcon;

	public Image imgIcon;

	private DialogueChanger dialogueChanger;

	private int indexButton;

	private bool click;

	private float timeAnimation;

	private bool destroyMe;

	private Color colorImgWas;

	private Color colorImgNeed;

	private Color colorTxtWas;

	private Color colorTxtNeed;

	private bool hide;

	private void Update()
	{
		if (!(timeAnimation < 1f))
		{
			return;
		}
		timeAnimation += Time.deltaTime * 4f;
		if (timeAnimation > 1f)
		{
			timeAnimation = 1f;
			if (destroyMe)
			{
				Object.Destroy(base.gameObject);
			}
		}
		img.color = Color.Lerp(colorImgWas, colorImgNeed, timeAnimation);
		txt.color = Color.Lerp(colorTxtWas, colorTxtNeed, timeAnimation);
		if (imgFrameIcon != null)
		{
			imgFrameIcon.color = img.color;
			imgIcon.color = txt.color;
		}
		if (GlobalGame.fontUse != null)
		{
			txt.font = GlobalGame.fontUse;
		}
	}

	public void StartComponent(DialogueChanger _main, int _index, int _stringFile, Sprite _icon)
	{
		dialogueChanger = _main;
		indexButton = _index;
		txt.text = GlobalLanguage.GetString("LocationDialogue " + GlobalTag.world.GetComponent<World>().nameLocation, _stringFile - 1);
		if (_icon == null)
		{
			Object.Destroy(imgFrameIcon.gameObject);
		}
		else
		{
			imgIcon.sprite = _icon;
		}
	}

	public void DestroyAlpha()
	{
		destroyMe = true;
		LerpColor(new Color(0f, 0f, 0f, 0f), new Color(1f, 1f, 1f, 0f));
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!click && !hide)
		{
			dialogueChanger.ClickButton(indexButton);
			click = true;
			img.color = new Color(0f, 0f, 0f, 1f);
			txt.color = new Color(1f, 1f, 1f, 1f);
			if (imgFrameIcon != null)
			{
				imgFrameIcon.color = img.color;
				imgIcon.color = txt.color;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!hide)
		{
			img.color = new Color(0.8f, 0.8f, 0.8f, 1f);
			txt.color = new Color(0.4f, 0.4f, 0.4f, 1f);
			if (imgFrameIcon != null)
			{
				imgFrameIcon.color = img.color;
				imgIcon.color = txt.color;
			}
			LerpColor(new Color(1f, 1f, 1f, 1f), new Color(0.1f, 0.1f, 0.1f, 1f));
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		LerpColor(new Color(0.1f, 0.1f, 0.1f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
	}

	private void OnEnable()
	{
		click = false;
		img.color = new Color(0.1f, 0.1f, 0.1f, 0f);
		txt.color = new Color(0.5f, 0.5f, 0.5f, 0f);
		if (imgFrameIcon != null)
		{
			imgFrameIcon.color = img.color;
			imgIcon.color = txt.color;
		}
		LerpColor(new Color(0.1f, 0.1f, 0.1f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
	}

	private void OnDisable()
	{
		if (destroyMe)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void LerpColor(Color _colorImgNeed, Color _colorTxtNeed)
	{
		if (!hide)
		{
			colorImgWas = img.color;
			colorImgNeed = _colorImgNeed;
			colorTxtWas = txt.color;
			colorTxtNeed = _colorTxtNeed;
			timeAnimation = 0f;
		}
	}

	private void LerpColorIgnoryHide(Color _colorImgNeed, Color _colorTxtNeed)
	{
		colorImgWas = img.color;
		colorImgNeed = _colorImgNeed;
		colorTxtWas = txt.color;
		colorTxtNeed = _colorTxtNeed;
		timeAnimation = 0f;
	}

	public void HideButton(bool x)
	{
		if (x)
		{
			LerpColor(new Color(0f, 0f, 0f, 0f), new Color(1f, 1f, 1f, 0f));
		}
		else if (hide)
		{
			click = false;
			LerpColorIgnoryHide(new Color(0.1f, 0.1f, 0.1f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
		}
		hide = x;
	}
}
