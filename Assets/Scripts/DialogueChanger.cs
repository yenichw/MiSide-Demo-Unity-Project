using UnityEngine;
using UnityEngine.Events;

public class DialogueChanger : MonoBehaviour
{
	[Header("Интерфейс")]
	public DialogueChangerCase[] buttons;

	[Header("Методы")]
	public UnityEvent eventStart;

	public UnityEvent eventClose;

	public UnityEvent eventExit;

	public UnityEvent eventLastStop;

	[Header("Настройки")]
	public bool lookOnCamera = true;

	private GameObject exampleButton;

	private Transform cameraT;

	private float timeClose;

	private void Start()
	{
		cameraT = GlobalTag.cameraPlayer.transform;
		exampleButton = base.transform.Find("Canvas/Case").gameObject;
		int num = 0;
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].button = Object.Instantiate(exampleButton, exampleButton.transform.parent);
			buttons[i].button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num);
			buttons[i].button.GetComponent<DialogueChange_Button>().StartComponent(this, i, buttons[i].stringFile, buttons[i].iconButton);
			num -= 40;
		}
		Object.Destroy(exampleButton);
	}

	private void Update()
	{
		if (lookOnCamera)
		{
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - cameraT.position, Vector3.up);
		}
		if (timeClose > 0f)
		{
			timeClose -= Time.deltaTime;
			if (timeClose <= 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void ClickButton(int _indexButton)
	{
		GlobalTag.player.GetComponent<PlayerMove>().BlockInteractive();
		if (!buttons[_indexButton].oneTimeUse)
		{
			buttons[_indexButton].eventClick.Invoke();
			if (buttons[_indexButton].oneTime)
			{
				buttons[_indexButton].button.GetComponent<DialogueChange_Button>().DestroyAlpha();
				buttons[_indexButton].oneTimeUse = true;
			}
			if (buttons[_indexButton].closeClick)
			{
				Close();
			}
			if (buttons[_indexButton].exitButton)
			{
				eventExit.Invoke();
				Close();
			}
		}
	}

	public void Close()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].button != null)
			{
				buttons[i].button.GetComponent<DialogueChange_Button>().HideButton(x: true);
			}
		}
		timeClose = 1f;
		GlobalTag.player.GetComponent<PlayerMove>().stopMouseMove = false;
		GlobalTag.player.GetComponent<PlayerMove>().dontMove = false;
		GlobalTag.gameController.GetComponent<GameController>().ShowCursor(x: false);
		GlobalTag.gameController.GetComponent<GameController>().DialogueChangerStart(null);
		bool flag = true;
		for (int j = 0; j < buttons.Length; j++)
		{
			if (!buttons[j].exitButton && !buttons[j].oneTimeUse)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			eventLastStop.Invoke();
		}
		eventClose.Invoke();
	}

	public void Reposition()
	{
		int num = 0;
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].button != null)
			{
				buttons[i].button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num);
				num -= 40;
			}
		}
	}

	public void OnEnable()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].button != null)
			{
				buttons[i].button.GetComponent<DialogueChange_Button>().HideButton(x: false);
			}
		}
		Reposition();
	}

	public void EscapeClick()
	{
		bool flag = false;
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].exitButton)
			{
				flag = true;
				ClickButton(i);
				break;
			}
		}
		if (!flag)
		{
			ClickButton(0);
		}
	}

	[ContextMenu("Play")]
	public void Play()
	{
		if (timeClose > 0f)
		{
			timeClose = 0f;
			OnEnable();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		GlobalTag.player.GetComponent<PlayerMove>().stopMouseMove = true;
		GlobalTag.player.GetComponent<PlayerMove>().dontMove = true;
		GlobalTag.gameController.GetComponent<GameController>().DialogueChangerStart(this);
		eventStart.Invoke();
	}
}
