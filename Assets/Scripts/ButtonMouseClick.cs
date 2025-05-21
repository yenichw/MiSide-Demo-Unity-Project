using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonMouseClick : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public ButtonMouseMenu menuButton;

	public bool interactable = true;

	public GameObject[] elements;

	public bool toggleSound;

	public bool offSound;

	public UnityEvent eventClick;

	public UnityEvent eventEnter;

	public UnityEvent eventExit;

	public UnityEvent eventUp;

	[Header("Переключение")]
	public ButtonMouseClick changeUp;

	public ButtonMouseClick changeDown;

	public ButtonMouseClick changeLeft;

	public ButtonMouseClick changeRight;

	[Header("Цвета")]
	public Color colorEnter = new Color(0.8f, 0f, 1f, 0.3f);

	public Color colorExit = new Color(0.8f, 0f, 1f, 0f);

	private bool stopKey;

	private bool firstStart;

	[HideInInspector]
	public bool changeNow;

	private float timeAnimation;

	private bool lockButton;

	private RectTransform change;

	private bool inputPosition;

	private int timeStart;

	public void Start()
	{
		if (firstStart)
		{
			return;
		}
		if (!interactable)
		{
			ActivationInteractive(x: false);
		}
		firstStart = true;
		timeAnimation = 1f;
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].GetComponent<UIShadow>().style = ShadowStyle.Shadow;
			elements[i].GetComponent<UIShadow>().effectColor = new Color(0f, 0f, 0f, 0f);
			elements[i].GetComponent<UIShadow>().effectDistance = new Vector2(3f, -3f);
			elements[i].GetComponent<UIShiny>().effectFactor = 0f;
		}
		if (GetComponent<UI_Colors>() != null)
		{
			GetComponent<UI_Colors>().SetColorImage(0, colorExit);
		}
		if (base.transform.parent.Find("Change") != null)
		{
			change = base.transform.parent.Find("Change").GetComponent<RectTransform>();
		}
		if (GetComponent<UI_Colors>() != null)
		{
			if (!interactable)
			{
				GetComponent<UI_Colors>().ShareAlpha(4f);
			}
			else
			{
				GetComponent<UI_Colors>().ShareAlpha(1f);
			}
		}
	}

	private void OnDisable()
	{
		PointerExit();
	}

	private void OnEnable()
	{
		timeStart = 2;
	}

	private void Update()
	{
		if (timeAnimation < 1f)
		{
			timeAnimation += Time.unscaledDeltaTime * 3f;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			for (int i = 0; i < elements.Length; i++)
			{
				if (changeNow)
				{
					elements[i].GetComponent<UIShadow>().effectColor = new Color(elements[i].GetComponent<UIShadow>().effectColor.r, elements[i].GetComponent<UIShadow>().effectColor.g, elements[i].GetComponent<UIShadow>().effectColor.b, timeAnimation);
					if (elements[i].GetComponent<UIShiny>().effectFactor < 1f)
					{
						elements[i].GetComponent<UIShiny>().effectFactor += Time.unscaledDeltaTime * 2f;
					}
				}
				else
				{
					elements[i].GetComponent<UIShadow>().effectColor = new Color(elements[i].GetComponent<UIShadow>().effectColor.r, elements[i].GetComponent<UIShadow>().effectColor.g, elements[i].GetComponent<UIShadow>().effectColor.b, 1f - timeAnimation);
					if (elements[i].GetComponent<UIShiny>().effectFactor < 1f)
					{
						elements[i].GetComponent<UIShiny>().effectFactor += Time.unscaledDeltaTime * 2f;
					}
				}
				if (elements[i].GetComponent<UIShiny>().brightness > 0f)
				{
					elements[i].GetComponent<UIShiny>().brightness -= Time.unscaledDeltaTime * 3.5f;
					if (elements[i].GetComponent<UIShiny>().brightness < 0f)
					{
						elements[i].GetComponent<UIShiny>().brightness = 0f;
					}
				}
			}
		}
		if (timeStart > 0)
		{
			timeStart--;
		}
		if (!(menuButton == null) && (!(menuButton != null) || !menuButton.keyMove))
		{
			return;
		}
		if (inputPosition && (double)Input.GetAxis("Vertical") > -0.5 && (double)Input.GetAxis("Vertical") < 0.5 && (double)Input.GetAxis("Horizontal") < 0.5 && (double)Input.GetAxis("Horizontal") > -0.5)
		{
			inputPosition = false;
		}
		if (changeNow && !stopKey)
		{
			if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Interactive"))
			{
				PointerDown();
			}
			if (changeUp != null && !inputPosition && (double)Input.GetAxis("Vertical") > 0.5)
			{
				ButtonMouseClick buttonMouseClick = changeUp;
				while (!buttonMouseClick.interactable)
				{
					buttonMouseClick = buttonMouseClick.changeUp;
				}
				buttonMouseClick.keyDown();
				buttonMouseClick.PointerEnter();
			}
			if (changeDown != null && !inputPosition && (double)Input.GetAxis("Vertical") < -0.5)
			{
				ButtonMouseClick buttonMouseClick2 = changeDown;
				while (!buttonMouseClick2.interactable)
				{
					buttonMouseClick2 = buttonMouseClick2.changeDown;
				}
				buttonMouseClick2.keyDown();
				buttonMouseClick2.PointerEnter();
			}
			if (changeLeft != null && !inputPosition && (double)Input.GetAxis("Horizontal") < -0.5)
			{
				ButtonMouseClick buttonMouseClick3 = changeLeft;
				while (!buttonMouseClick3.interactable)
				{
					buttonMouseClick3 = buttonMouseClick3.changeLeft;
				}
				buttonMouseClick3.keyDown();
				buttonMouseClick3.PointerEnter();
			}
			if (changeRight != null && !inputPosition && (double)Input.GetAxis("Horizontal") > 0.5)
			{
				ButtonMouseClick buttonMouseClick4 = changeRight;
				while (!buttonMouseClick4.interactable)
				{
					buttonMouseClick4 = buttonMouseClick4.changeRight;
				}
				buttonMouseClick4.keyDown();
				buttonMouseClick4.PointerEnter();
			}
		}
		stopKey = false;
	}

	public void keyDown()
	{
		if (base.transform.parent.GetComponent<MenuScrolac>() != null)
		{
			base.transform.parent.GetComponent<MenuScrolac>().UpdateScrolac();
		}
	}

	public void LockClick(bool x)
	{
		lockButton = x;
	}

	public void ActivationInteractive(bool x)
	{
		interactable = x;
		if (!x)
		{
			changeNow = false;
			timeAnimation = 0f;
			if (GetComponent<UI_Colors>() != null)
			{
				GetComponent<UI_Colors>().ShareAlpha(4f);
			}
		}
		else if (GetComponent<UI_Colors>() != null)
		{
			GetComponent<UI_Colors>().ShareAlpha(1f);
		}
		if (GetComponent<Button>() != null)
		{
			GetComponent<Button>().enabled = x;
		}
	}

	public void PointerExit()
	{
		if (interactable)
		{
			eventExit.Invoke();
			changeNow = false;
			if (GetComponent<UI_Colors>() != null)
			{
				GetComponent<UI_Colors>().SetColorImage(0, colorExit);
			}
			timeAnimation = 0f;
		}
	}

	public void PointerEnter()
	{
		if (!firstStart)
		{
			Start();
		}
		if (interactable && !lockButton)
		{
			eventEnter.Invoke();
			changeNow = true;
			if (GetComponent<UI_Colors>() != null)
			{
				GetComponent<UI_Colors>().SetColorImage(0, colorEnter);
			}
			stopKey = true;
			timeAnimation = 0f;
			if (menuButton != null)
			{
				menuButton.EnterButton();
			}
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i].GetComponent<UIShiny>().effectFactor = 0f;
				elements[i].GetComponent<UIShiny>().brightness = 1f;
			}
			if (menuButton != null)
			{
				menuButton.ChangeCase(base.gameObject);
			}
			if (change != null)
			{
				change.anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
				if (base.transform.parent.GetComponent<MenuScrolac>() != null && base.transform.parent.GetComponent<MenuScrolac>().changeCopyRect)
				{
					change.sizeDelta = GetComponent<RectTransform>().sizeDelta;
				}
			}
		}
		if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f)
		{
			inputPosition = true;
		}
		if (GetComponent<MenuCaseOption>() != null)
		{
			GetComponent<MenuCaseOption>().MouseEnter();
		}
	}

	public void PointerDown()
	{
		if (lockButton || !interactable)
		{
			return;
		}
		eventClick.Invoke();
		if (GetComponent<MenuNextLocation>() != null)
		{
			GetComponent<MenuNextLocation>().Click();
		}
		if (GetComponent<MenuCaseOption>() != null)
		{
			GetComponent<MenuCaseOption>().Click();
		}
		if (menuButton != null && !offSound)
		{
			if (!toggleSound)
			{
				menuButton.ClickButton();
			}
			else
			{
				menuButton.ClickButtonToggle();
			}
		}
		if (GetComponent<ButtonMouseClickBubble>() != null)
		{
			GetComponent<ButtonMouseClickBubble>().Click();
		}
	}

	public void PointerUp()
	{
		eventUp.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerEnter();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (timeStart == 0)
		{
			PointerDown();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerExit();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		PointerUp();
	}
}
