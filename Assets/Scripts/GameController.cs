using System.Collections.Generic;
using Coffee.UIEffects;
using Colorful;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	private PlayerMove scrpm;

	private PlayerPersonIK scrppik;

	[HideInInspector]
	public float crosshairSize;

	private bool crosshairHide;

	private Camera cameraPlayer;

	private bool showCursor;

	private DialogueChanger dialogueChanger;

	[Header("Интерфейс")]
	public bool canChangeItemForHands;

	public bool showInventoryForHands;

	public Image imgMouseCast;

	public Image imgMouse;

	public GameObject prefabFastMenu;

	private GameObject fastMenuObject;

	public GameObject prefabHintScreen;

	private AsyncOperation asyncLoadNextScene;

	[Header("Интерфейс загрузки")]
	public Image lineLoadingAsync;

	public Image imgLoadingAsync;

	private float timePrint;

	[HideInInspector]
	public List<TextDialogueMemory> dialoguesMemory;

	[HideInInspector]
	public float timePrintWas;

	[Header("Интерфейс диалог")]
	public Image imageFrameDialogue;

	public Text textDialogue;

	[HideInInspector]
	public bool isPaused;

	private Component[] soundsPauseA;

	private Component[] soundsPauseB;

	private Component[] audioReverbs;

	private Component[] canvasesPause;

	private bool pauseWasBlur;

	[HideInInspector]
	public CursorLockMode pauseWasLockModeCursor;

	[Header("Пауза")]
	public GaussianBlur blurCamera;

	private RaycastHit hit;

	[HideInInspector]
	public int timeLightCast;

	[Header("Слои")]
	public LayerMask layerPlayer;

	public LayerMask layerWall;

	public LayerMask layerMouse;

	public LayerMask layerPhysic;

	public LayerMask layerPhysicSound;

	[HideInInspector]
	public List<GameObject> inventoryKeysCase;

	[Header("Инвентарь")]
	public GameObject inventoryKeysCaseExample;

	[HideInInspector]
	public List<GameObject> keyItems;

	[HideInInspector]
	public UnityEvent eventItemKeyAdd;

	[HideInInspector]
	public UnityEvent eventItemKeyRemove;

	[HideInInspector]
	public bool cutsceneRun;

	[HideInInspector]
	public GameObject objectCast;

	[HideInInspector]
	public bool firstPersonPlay;

	private float timeAFK;

	[Header("Бездействует")]
	public TetrisGame tetrisComponent;

	public GameObject tetrisObject;

	public ObjectAnimationPlayer animationPlayerTetris;

	private void Awake()
	{
		if (GlobalTag.gameController == null && GlobalGame.play)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			GlobalTag.gameController = base.gameObject;
			firstPersonPlay = true;
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public void StartComponent()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		scrppik = scrpm.transform.Find("Person").GetComponent<PlayerPersonIK>();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		crosshairSize = 1f;
		UpdateInterfaceInventoryKeys();
		cameraPlayer = GlobalTag.cameraPlayer.GetComponent<Camera>();
		GlobalTag.gameOptions.GetComponent<OptionsGame>().ReloadOptions();
	}

	private void Update()
	{
		if (!scrpm.animationRun)
		{
			bool flag = false;
			if (scrpm.objectCastInteractive != null && scrpm.objectCastInteractive.GetComponent<ObjectInteractive>() != null && !scrpm.objectCastInteractive.GetComponent<ObjectInteractive>().interactiveIK && scrpm.objectCastInteractive.GetComponent<ObjectInteractive>().active && (!scrpm.animationHandRun || (scrpm.animationHandRun && scrpm.objectCastInteractive.GetComponent<ObjectInteractive>().ignoryAnimationHand)))
			{
				flag = true;
			}
			if (scrpm.objectCastHandInteractive != null && scrpm.objectCastHandInteractive.GetComponent<ObjectInteractive>() != null && scrpm.objectCastHandInteractive.GetComponent<ObjectInteractive>().active)
			{
				flag = true;
			}
			if (timeLightCast > 0)
			{
				flag = true;
			}
			if (flag)
			{
				imgMouseCast.color = Color.Lerp(imgMouseCast.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 20f);
			}
			else
			{
				imgMouseCast.color = Color.Lerp(imgMouseCast.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 20f);
			}
			if ((scrpm.objectCast != null && scrpm.objectCast.layer == 8) || GlobalGame.trailer)
			{
				imgMouse.color = Color.Lerp(imgMouse.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 20f);
			}
			else
			{
				imgMouse.color = Color.Lerp(imgMouse.color, new Color(1f, 1f, 1f, 0.5f), Time.deltaTime * 20f);
			}
		}
		else
		{
			imgMouseCast.color = Color.Lerp(imgMouseCast.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 20f);
			imgMouse.color = Color.Lerp(imgMouse.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 20f);
		}
		if (timeLightCast > 0)
		{
			timeLightCast--;
		}
		if (imgMouse.GetComponent<RectTransform>().localScale.x != crosshairSize)
		{
			imgMouse.GetComponent<RectTransform>().localScale = Vector3.Lerp(imgMouse.GetComponent<RectTransform>().localScale, Vector3.one * crosshairSize, Time.deltaTime * 5f);
		}
		if (Input.GetButtonDown("Cancel"))
		{
			if (timeAFK != -1.1f)
			{
				if (dialogueChanger == null)
				{
					scrpm.BlockInteractive();
					if (!isPaused)
					{
						FastMenuOpen();
					}
					else if (fastMenuObject != null)
					{
						fastMenuObject.GetComponent<InterfaceFastMenu>().CloseMenu();
					}
				}
				else
				{
					dialogueChanger.EscapeClick();
					dialogueChanger = null;
				}
			}
			else if (tetrisObject.activeSelf)
			{
				animationPlayerTetris.AnimationStop();
				timeAFK = 5f;
			}
		}
		if (timePrintWas > 0f)
		{
			timePrintWas -= Time.deltaTime;
			if (timePrintWas < 0f)
			{
				timePrintWas = 0f;
			}
		}
		if (timePrint > 0f)
		{
			timePrint -= Time.deltaTime;
			if (timePrint <= 0f)
			{
				timePrint = 0f;
			}
			if (imageFrameDialogue.color.a < 0.6f)
			{
				imageFrameDialogue.color = new Color(0.3f, 0f, 0.3f, imageFrameDialogue.color.a + Time.deltaTime * 2f);
				if (imageFrameDialogue.color.a > 0.6f)
				{
					imageFrameDialogue.color = new Color(0.3f, 0f, 0.3f, 0.6f);
				}
			}
			if (textDialogue.color.a < 1f)
			{
				textDialogue.color = new Color(1f, 1f, 1f, textDialogue.color.a + Time.deltaTime * 2f);
				if (textDialogue.color.a > 1f)
				{
					textDialogue.color = new Color(1f, 1f, 1f, 1f);
				}
			}
		}
		else
		{
			if (imageFrameDialogue.color.a > 0f)
			{
				imageFrameDialogue.color = new Color(0.3f, 0f, 0.3f, imageFrameDialogue.color.a - Time.deltaTime * 2f);
				if (imageFrameDialogue.color.a < 0f)
				{
					imageFrameDialogue.color = new Color(0.3f, 0f, 0.3f, 0f);
				}
			}
			if (textDialogue.color.a > 0f)
			{
				textDialogue.color = new Color(1f, 1f, 1f, textDialogue.color.a - Time.deltaTime * 2f);
				if (textDialogue.color.a < 0f)
				{
					textDialogue.color = new Color(1f, 1f, 1f, 0f);
				}
			}
			if (imageFrameDialogue.color.a <= 0f && textDialogue.color.a < 0f)
			{
				imageFrameDialogue.gameObject.SetActive(value: false);
			}
		}
		if (asyncLoadNextScene != null)
		{
			if (asyncLoadNextScene.progress < 0.9f)
			{
				if (!lineLoadingAsync.fillClockwise)
				{
					lineLoadingAsync.fillAmount += Time.unscaledDeltaTime;
					if (lineLoadingAsync.fillAmount >= 1f)
					{
						lineLoadingAsync.fillClockwise = true;
					}
				}
				else
				{
					lineLoadingAsync.fillAmount -= Time.unscaledDeltaTime;
					if (lineLoadingAsync.fillAmount <= 0f)
					{
						lineLoadingAsync.fillClockwise = false;
					}
				}
				if (lineLoadingAsync.color.a < 1f)
				{
					lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, lineLoadingAsync.color.a + Time.unscaledDeltaTime * 3f);
					if (lineLoadingAsync.color.a > 1f)
					{
						lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, 1f);
					}
					imgLoadingAsync.color = new Color(imgLoadingAsync.color.r, imgLoadingAsync.color.g, imgLoadingAsync.color.b, lineLoadingAsync.color.a);
				}
			}
			else
			{
				lineLoadingAsync.fillAmount = 1f;
				if (lineLoadingAsync.color.a > 0f)
				{
					lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, lineLoadingAsync.color.a - Time.unscaledDeltaTime * 0.2f);
					if (lineLoadingAsync.color.a < 0f)
					{
						lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, 0f);
						imgLoadingAsync.gameObject.SetActive(value: false);
					}
					imgLoadingAsync.color = new Color(imgLoadingAsync.color.r, imgLoadingAsync.color.g, imgLoadingAsync.color.b, lineLoadingAsync.color.a);
				}
			}
		}
		else if (lineLoadingAsync.color.a > 0f)
		{
			lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, lineLoadingAsync.color.a - Time.unscaledDeltaTime * 3f);
			if (lineLoadingAsync.color.a < 0f)
			{
				lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, 0f);
				imgLoadingAsync.gameObject.SetActive(value: false);
			}
			imgLoadingAsync.color = new Color(imgLoadingAsync.color.r, imgLoadingAsync.color.g, imgLoadingAsync.color.b, lineLoadingAsync.color.a);
		}
		if (timeAFK >= 0f)
		{
			timeAFK += Time.deltaTime;
			if (timeAFK > 120f)
			{
				timeAFK = -0.1f;
				tetrisObject.SetActive(value: true);
				tetrisComponent.StartRestart();
				animationPlayerTetris.AnimationPlay();
			}
			else if (!GlobalTag.player.gameObject.activeSelf || scrpm.animationRun || scrpm.dontMove || dialogueChanger != null || timePrintWas > 0f || Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f || Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
			{
				timeAFK = 0f;
			}
		}
		else if ((double)timeAFK > -1.1)
		{
			timeAFK -= Time.deltaTime;
			if ((double)timeAFK < -1.1)
			{
				timeAFK = -1.1f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (Physics.Raycast(cameraPlayer.ScreenPointToRay(Input.mousePosition), out hit, 100f, layerMouse))
		{
			objectCast = hit.collider.gameObject;
		}
		else
		{
			objectCast = null;
		}
	}

	public void CutscenePlay(Transform _cameraTarget)
	{
		cutsceneRun = true;
		imgMouseCast.transform.parent.gameObject.SetActive(value: false);
		scrpm.mainCamera.parent = _cameraTarget;
		scrpm.mainCamera.localPosition = Vector3.zero;
		scrpm.mainCamera.localRotation = Quaternion.Euler(Vector3.zero);
		scrpm.Hide(x: true);
	}

	public void CutsceneStop()
	{
		cutsceneRun = false;
		imgMouseCast.transform.parent.gameObject.SetActive(value: true);
		scrpm.Hide(x: false);
		scrpm.mainCamera.parent = scrpm.head;
		scrpm.mainCamera.localPosition = Vector3.zero;
		scrpm.mainCamera.localRotation = Quaternion.Euler(Vector3.zero);
	}

	public void FastMenuClose()
	{
		Pause(x: false, _isLoading: false);
		Object.Destroy(fastMenuObject);
	}

	public void FastMenuOpen()
	{
		Pause(x: true, _isLoading: false);
		fastMenuObject = Object.Instantiate(prefabFastMenu, base.transform.Find("Interface"));
		fastMenuObject.GetComponent<InterfaceFastMenu>().StartComponent(this);
	}

	public void Pause(bool x, bool _isLoading)
	{
		if (x == isPaused)
		{
			return;
		}
		isPaused = x;
		if (x)
		{
			pauseWasBlur = blurCamera.enabled;
			blurCamera.enabled = true;
			Time.timeScale = 0f;
			imageFrameDialogue.gameObject.SetActive(value: false);
			if (!_isLoading)
			{
				Cursor.visible = true;
				pauseWasLockModeCursor = Cursor.lockState;
				Cursor.lockState = CursorLockMode.None;
			}
			imgMouseCast.transform.parent.gameObject.SetActive(value: false);
			if (scrppik.objectInHand != null && scrppik.objectInHand.GetComponent<ItemInteractive>() != null)
			{
				scrppik.objectInHand.GetComponent<ItemInteractive>().Pause(_pause: true);
			}
			soundsPauseA = GameObject.FindWithTag("World").transform.GetComponentsInChildren(typeof(AudioSource), includeInactive: true);
			for (int i = 0; i < soundsPauseA.Length; i++)
			{
				if (soundsPauseA[i].GetComponent<AudioSource>().isPlaying)
				{
					soundsPauseA[i].GetComponent<AudioSource>().Pause();
				}
				else
				{
					soundsPauseA[i] = null;
				}
			}
			soundsPauseB = base.transform.GetComponentsInChildren(typeof(AudioSource), includeInactive: true);
			for (int j = 0; j < soundsPauseB.Length; j++)
			{
				if (soundsPauseB[j].GetComponent<AudioSource>().isPlaying)
				{
					soundsPauseB[j].GetComponent<AudioSource>().Pause();
				}
				else
				{
					soundsPauseB[j] = null;
				}
			}
			audioReverbs = GameObject.FindWithTag("World").transform.GetComponentsInChildren(typeof(AudioReverbZone), includeInactive: true);
			for (int k = 0; k < audioReverbs.Length; k++)
			{
				audioReverbs[k].GetComponent<AudioReverbZone>().enabled = false;
			}
			canvasesPause = GameObject.FindWithTag("World").transform.GetComponentsInChildren(typeof(Canvas), includeInactive: true);
			for (int l = 0; l < canvasesPause.Length; l++)
			{
				if (canvasesPause[l].GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay && canvasesPause[l].GetComponent<UI_AntiPause>() == null)
				{
					canvasesPause[l].GetComponent<Canvas>().enabled = false;
				}
			}
			return;
		}
		if (!pauseWasBlur)
		{
			blurCamera.enabled = false;
		}
		Time.timeScale = 1f;
		if (imageFrameDialogue.color.a > 0f)
		{
			imageFrameDialogue.gameObject.SetActive(value: true);
		}
		if (!crosshairHide)
		{
			imgMouseCast.transform.parent.gameObject.SetActive(value: true);
			if (!showCursor && !_isLoading)
			{
				Cursor.visible = false;
				Cursor.lockState = pauseWasLockModeCursor;
			}
		}
		if (scrppik.objectInHand != null && scrppik.objectInHand.GetComponent<ItemInteractive>() != null)
		{
			scrppik.objectInHand.GetComponent<ItemInteractive>().Pause(_pause: false);
		}
		for (int m = 0; m < soundsPauseA.Length; m++)
		{
			if (soundsPauseA[m] != null)
			{
				soundsPauseA[m].GetComponent<AudioSource>().Play();
			}
		}
		soundsPauseA = null;
		for (int n = 0; n < soundsPauseB.Length; n++)
		{
			if (soundsPauseB[n] != null)
			{
				soundsPauseB[n].GetComponent<AudioSource>().Play();
			}
		}
		soundsPauseB = null;
		for (int num = 0; num < audioReverbs.Length; num++)
		{
			audioReverbs[num].GetComponent<AudioReverbZone>().enabled = true;
		}
		audioReverbs = null;
		for (int num2 = 0; num2 < canvasesPause.Length; num2++)
		{
			if (canvasesPause[num2] != null && canvasesPause[num2].GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay && canvasesPause[num2].GetComponent<UI_AntiPause>() == null)
			{
				canvasesPause[num2].GetComponent<Canvas>().enabled = true;
			}
		}
		canvasesPause = null;
	}

	public void ExitGame()
	{
		GameObject.FindWithTag("Game").transform.Find("Interface/BlackScreen").GetComponent<BlackScreen>().NextLevel("SceneMenu");
	}

	public void HideCrosshair(bool x)
	{
		crosshairHide = x;
		if (crosshairHide)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			imgMouseCast.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			imgMouseCast.transform.parent.gameObject.SetActive(value: true);
		}
	}

	public void HideCrosshair(bool _crosshair, bool _cursor)
	{
		crosshairHide = _crosshair;
		if (crosshairHide)
		{
			if (_cursor)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			imgMouseCast.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			if (_cursor)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			imgMouseCast.transform.parent.gameObject.SetActive(value: true);
		}
	}

	public void ShowCursor(bool x)
	{
		if (x)
		{
			HideCrosshair(x: true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			HideCrosshair(x: false);
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		showCursor = x;
	}

	public void DialogueChangerStart(DialogueChanger _object)
	{
		if (_object != null)
		{
			dialogueChanger = _object;
			ShowCursor(x: true);
		}
		else
		{
			dialogueChanger = null;
		}
	}

	public void ShowHint(string _text, Vector2 _position)
	{
		GameObject obj = Object.Instantiate(prefabHintScreen, base.transform.Find("Interface"));
		obj.transform.Find("Text").GetComponent<Text>().text = _text;
		obj.GetComponent<Animator_OneTimeDestroy>().ActiveObject();
		obj.GetComponent<RectTransform>().anchoredPosition = _position;
	}

	public void PrintDialogue(Dialogue_3DText _componentDialogue, bool _boxVisible)
	{
		timePrintWas = 2f;
		if (!_boxVisible || GlobalGame.everSub)
		{
			timePrint = 1f;
		}
		textDialogue.text = _componentDialogue.textPrintNow;
		Color color = _componentDialogue.exampleSymbol.GetComponent<Text>().color;
		textDialogue.color = new Color(color.r, color.g, color.b, textDialogue.color.a);
		textDialogue.GetComponent<UIGradient>().offset = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().offset;
		textDialogue.GetComponent<UIGradient>().color1 = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().color1;
		textDialogue.GetComponent<UIGradient>().color2 = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().color2;
		if (!imageFrameDialogue.gameObject.activeSelf)
		{
			imageFrameDialogue.gameObject.SetActive(value: true);
			imageFrameDialogue.color = new Color(0f, 0f, 0f, 0f);
			textDialogue.color = new Color(1f, 1f, 1f, 0f);
		}
	}

	public void DialogueAdd(Dialogue_3DText _componentDialogue, string _text)
	{
		if (dialoguesMemory.Count > 30)
		{
			dialoguesMemory.RemoveAt(0);
		}
		TextDialogueMemory item = new TextDialogueMemory
		{
			text = _text,
			clr = _componentDialogue.exampleSymbol.GetComponent<Text>().color,
			offset = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().offset,
			clr1 = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().color1,
			clr2 = _componentDialogue.exampleSymbol.GetComponent<UIGradient>().color2
		};
		dialoguesMemory.Add(item);
	}

	public void AddKeyItem(GameObject _item)
	{
		keyItems.Add(_item);
		UpdateInterfaceInventoryKeys();
		eventItemKeyAdd.Invoke();
	}

	public void RemoveKeyItem(GameObject _item)
	{
		int keyItem = GetKeyItem(_item);
		if (keyItem != -1)
		{
			keyItems.RemoveAt(keyItem);
			UpdateInterfaceInventoryKeys();
			eventItemKeyRemove.Invoke();
		}
	}

	public int GetKeyItem(GameObject _item)
	{
		int result = -1;
		if (keyItems != null && keyItems.Count > 0)
		{
			for (int i = 0; i < keyItems.Count; i++)
			{
				if (keyItems[i] == _item)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private void UpdateInterfaceInventoryKeys()
	{
		for (int i = 0; i < inventoryKeysCase.Count; i++)
		{
			Object.Destroy(inventoryKeysCase[i]);
		}
		inventoryKeysCase.Clear();
		if (keyItems != null && keyItems.Count > 0)
		{
			for (int j = 0; j < keyItems.Count; j++)
			{
				GameObject gameObject = Object.Instantiate(inventoryKeysCaseExample, inventoryKeysCaseExample.transform.parent);
				gameObject.transform.Find("Icon").GetComponent<Image>().sprite = keyItems[j].GetComponent<ObjectItem>().icon;
				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(25 + 105 * j, 25f);
				gameObject.SetActive(value: true);
				inventoryKeysCase.Add(gameObject);
			}
		}
	}

	public void FirstPersonPlay(bool x)
	{
		firstPersonPlay = x;
	}

	public void StartSceneLoadAsync(AsyncOperation _async)
	{
		asyncLoadNextScene = _async;
		lineLoadingAsync.fillAmount = 0f;
		imgLoadingAsync.gameObject.SetActive(value: true);
		imgLoadingAsync.color = new Color(imgLoadingAsync.color.r, imgLoadingAsync.color.g, imgLoadingAsync.color.b, 0f);
		lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, 0f);
	}

	public void FinishSceneLoadAsync()
	{
		asyncLoadNextScene = null;
	}

	public void SceneLoadAsyncReady()
	{
		imgLoadingAsync.color = new Color(imgLoadingAsync.color.r, imgLoadingAsync.color.g, imgLoadingAsync.color.b, 1f);
		lineLoadingAsync.color = new Color(lineLoadingAsync.color.r, lineLoadingAsync.color.g, lineLoadingAsync.color.b, 1f);
	}
}
