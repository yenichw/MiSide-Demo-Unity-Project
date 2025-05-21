using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tamagotchi_Main : MonoBehaviour
{
	public int energy;

	public int money;

	private GameObject castObject;

	private Camera cmr;

	[Header("Чиби Мита")]
	public LayerMask layerChibi;

	public GameObject chibiMita;

	public ParticleSystem heartChibi;

	[Header("Дом")]
	public GameObject objectHouse;

	public Tamagotchi_Room[] rooms;

	public int indexRoomShow;

	public Color colorAmbientReal;

	public Color colorAmbientTamagotchi;

	[Header("Интерфейс")]
	public bool showInterface;

	public bool showButtonShop;

	public bool showButtonGames;

	public bool sprBlackScreenHide;

	public GameObject objectDialogue;

	public RectTransform rectLife;

	public Text textMoney;

	public ButtonMouseClickBubble buttonRight;

	public ButtonMouseClickBubble buttonLeft;

	public RectTransform blackLeft;

	public RectTransform blackRight;

	public RectTransform buttonShop;

	public RectTransform buttonGame;

	public RectTransform rectCoins;

	public GameObject buttonCloseWindows;

	public GameObject buttonCloseMiniGame;

	public Image imageScreen;

	public GameObject frameAddMoney;

	public SpriteRenderer spriteBlackScreen;

	public Text textEnergy;

	public AnimationCurve aniamtionEnergyAlpha;

	private float timeEnergyAlpha;

	public RectTransform lineHunger;

	public RectTransform lineMood;

	public Tamagotchi_BuyCase[] casesBuy;

	public RectTransform backgroundGame;

	public RectTransform backgroundShop;

	private bool addoneWindowActive;

	[Header("Дополнительные окна")]
	public Tamagotchi_WindowButtons windowShop;

	public Tamagotchi_WindowButtons windowMiniGames;

	private Transform cameraTransform;

	private bool play;

	private Tamagotchi_MiniGame minigameNow;

	private bool playMinigame;

	[Header("Сюжетная часть")]
	public Location1Main worldLocation1;

	private bool animationCameraZoom;

	private float timeCameraAnimation;

	public AnimationCurve animationCurveCameraZoom;

	public UnityEvent eventAnimationZoomBack;

	private bool zoomBack;

	public UnityEvent eventAnimationZoomStop;

	[Header("Звуки")]
	public AudioSource audioOpenShop;

	public AudioSource audioOpenMinigame;

	private void Start()
	{
		cmr = GlobalTag.cameraPlayer.GetComponent<Camera>();
		timeEnergyAlpha = 1f;
	}

	private void Update()
	{
		if (play)
		{
			if (!playMinigame)
			{
				cameraTransform.transform.localPosition = Vector3.Lerp(cameraTransform.transform.localPosition, new Vector3(rooms[indexRoomShow].cameraPositionX, 12f, 15f), Time.deltaTime * 15f);
			}
			if (showInterface && !objectDialogue.activeSelf)
			{
				rectLife.offsetMax = Vector2.Lerp(rectLife.offsetMax, Vector2.zero, Time.deltaTime * 5f);
				rectLife.offsetMin = Vector2.Lerp(rectLife.offsetMin, Vector2.zero, Time.deltaTime * 5f);
				if (addoneWindowActive || playMinigame || !showInterface)
				{
					buttonRight.SetAnchoredLerp(new Vector2(100f, 0f), Time.deltaTime * 15f);
					buttonLeft.SetAnchoredLerp(new Vector2(-100f, 0f), Time.deltaTime * 15f);
					blackRight.anchoredPosition = Vector2.Lerp(blackRight.anchoredPosition, new Vector2(150f, 0f), Time.deltaTime * 15f);
					blackLeft.anchoredPosition = Vector2.Lerp(blackLeft.anchoredPosition, new Vector2(-150f, 0f), Time.deltaTime * 15f);
					buttonGame.anchoredPosition = Vector2.Lerp(buttonGame.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
					buttonShop.anchoredPosition = Vector2.Lerp(buttonShop.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
					if ((minigameNow != null && minigameNow.dontExit) || (!playMinigame && !addoneWindowActive))
					{
						rectCoins.anchoredPosition = Vector2.Lerp(rectCoins.anchoredPosition, new Vector2(-25f, -35f), Time.deltaTime * 15f);
					}
					else
					{
						rectCoins.anchoredPosition = Vector2.Lerp(rectCoins.anchoredPosition, new Vector2(-120f, -35f), Time.deltaTime * 15f);
					}
				}
				else
				{
					buttonRight.SetAnchoredLerp(new Vector2(-25f, 0f), Time.deltaTime * 15f);
					buttonLeft.SetAnchoredLerp(new Vector2(25f, 0f), Time.deltaTime * 15f);
					blackRight.anchoredPosition = Vector2.Lerp(blackRight.anchoredPosition, Vector2.zero, Time.deltaTime * 15f);
					blackLeft.anchoredPosition = Vector2.Lerp(blackLeft.anchoredPosition, Vector2.zero, Time.deltaTime * 15f);
					if (Input.GetButtonDown("Right"))
					{
						buttonRight.GetComponent<ButtonMouseClick>().PointerDown();
					}
					if (Input.GetButtonDown("Left"))
					{
						buttonLeft.GetComponent<ButtonMouseClick>().PointerDown();
					}
					int num = -25;
					if (showButtonGames)
					{
						buttonGame.anchoredPosition = Vector2.Lerp(buttonGame.anchoredPosition, new Vector2(num, -25f), Time.deltaTime * 15f);
						num -= 95;
					}
					else
					{
						buttonGame.anchoredPosition = Vector2.Lerp(buttonGame.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
					}
					if (showButtonShop)
					{
						buttonShop.anchoredPosition = Vector2.Lerp(buttonShop.anchoredPosition, new Vector2(num, -25f), Time.deltaTime * 15f);
						num -= 95;
					}
					else
					{
						buttonShop.anchoredPosition = Vector2.Lerp(buttonShop.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
					}
					rectCoins.anchoredPosition = Vector2.Lerp(rectCoins.anchoredPosition, new Vector2(num, -35f), Time.deltaTime * 15f);
				}
			}
			else
			{
				rectLife.offsetMax = Vector2.Lerp(rectLife.offsetMax, new Vector2(250f, 250f), Time.deltaTime * 5f);
				rectLife.offsetMin = Vector2.Lerp(rectLife.offsetMin, new Vector2(-250f, -250f), Time.deltaTime * 5f);
				buttonShop.anchoredPosition = Vector2.Lerp(buttonShop.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
				buttonGame.anchoredPosition = Vector2.Lerp(buttonGame.anchoredPosition, new Vector2(90f, -25f), Time.deltaTime * 15f);
				rectCoins.anchoredPosition = Vector2.Lerp(rectCoins.anchoredPosition, new Vector2(-25f, -35f), Time.deltaTime * 15f);
			}
			if (sprBlackScreenHide)
			{
				spriteBlackScreen.color = Color.Lerp(spriteBlackScreen.color, new Color(spriteBlackScreen.color.r, spriteBlackScreen.color.g, spriteBlackScreen.color.b, 1f), Time.deltaTime * 3f);
			}
			else if (!objectDialogue.activeSelf)
			{
				spriteBlackScreen.color = Color.Lerp(spriteBlackScreen.color, new Color(spriteBlackScreen.color.r, spriteBlackScreen.color.g, spriteBlackScreen.color.b, 0f), Time.deltaTime * 3f);
			}
			else
			{
				spriteBlackScreen.color = Color.Lerp(spriteBlackScreen.color, new Color(spriteBlackScreen.color.r, spriteBlackScreen.color.g, spriteBlackScreen.color.b, 0.7f), Time.deltaTime * 3f);
			}
			if (Input.GetMouseButtonDown(0) && !objectDialogue.activeSelf && !addoneWindowActive && !sprBlackScreenHide && castObject == chibiMita)
			{
				heartChibi.Play();
				heartChibi.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
				heartChibi.GetComponent<AudioSource>().Play();
			}
			backgroundGame.sizeDelta += new Vector2(Time.deltaTime * 128f, 0f);
			if (backgroundGame.sizeDelta.x > 512f)
			{
				backgroundGame.sizeDelta = new Vector2(0f, 0f);
			}
			backgroundShop.sizeDelta = backgroundGame.sizeDelta;
			if (animationCameraZoom)
			{
				timeCameraAnimation += Time.deltaTime * 0.1f;
				cameraTransform.GetComponent<Camera>().orthographicSize = 5f - animationCurveCameraZoom.Evaluate(timeCameraAnimation);
				if (!zoomBack && timeCameraAnimation > 0.9f)
				{
					zoomBack = true;
					eventAnimationZoomBack.Invoke();
				}
				if (timeCameraAnimation >= 1f)
				{
					timeCameraAnimation = 1f;
					animationCameraZoom = false;
					eventAnimationZoomStop.Invoke();
				}
			}
			if (timeEnergyAlpha < 1f)
			{
				timeEnergyAlpha += Time.deltaTime;
				if (timeEnergyAlpha > 1f)
				{
					timeEnergyAlpha = 1f;
				}
				textEnergy.color = new Color(1f, 1f, 1f, aniamtionEnergyAlpha.Evaluate(timeEnergyAlpha));
			}
		}
		if (imageScreen.color.a > 0f)
		{
			imageScreen.color = new Color(0f, 0f, 0f, imageScreen.color.a - Time.deltaTime * 2f);
		}
	}

	private void FixedUpdate()
	{
		if (Physics.Raycast(cmr.ScreenPointToRay(Input.mousePosition), out var hitInfo, 100f, layerChibi))
		{
			castObject = hitInfo.collider.gameObject;
		}
		else
		{
			castObject = null;
		}
	}

	public void GameStart()
	{
		if (!play)
		{
			base.gameObject.SetActive(value: true);
			objectHouse.SetActive(value: true);
			play = true;
			rectLife.offsetMin = new Vector2(-250f, -250f);
			rectLife.offsetMax = new Vector2(250f, 250f);
			NewDay();
			GlobalTag.player.SetActive(value: false);
			GlobalTag.gameController.GetComponent<GameController>().HideCrosshair(x: true);
			GlobalTag.gameController.GetComponent<GameController>().FirstPersonPlay(x: false);
			cameraTransform = GlobalTag.cameraPlayer.transform;
			GlobalTag.cameraPlayer.transform.parent = objectHouse.transform;
			CameraFix();
			RenderSettings.ambientSkyColor = colorAmbientTamagotchi;
		}
	}

	public void GameStop()
	{
		play = false;
		GlobalTag.player.SetActive(value: true);
		GlobalTag.gameController.GetComponent<GameController>().HideCrosshair(x: false);
		GlobalTag.cameraPlayer.transform.parent = GlobalTag.player.transform.Find("HeadPlayer");
		Camera component = GlobalTag.cameraPlayer.GetComponent<Camera>();
		component.orthographic = false;
		component.fieldOfView = 50f;
		component.transform.localPosition = Vector3.zero;
		component.transform.localRotation = Quaternion.identity;
		component.backgroundColor = Color.black;
		component.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
		base.gameObject.SetActive(value: false);
		RenderSettings.ambientSkyColor = colorAmbientReal;
		objectDialogue.GetComponent<Tamagotchi_Dialogue>().FastCloseDialoue();
	}

	public void RightRoom()
	{
		indexRoomShow++;
		if (indexRoomShow > rooms.Length - 1)
		{
			indexRoomShow = 0;
		}
	}

	public void LeftRoom()
	{
		indexRoomShow--;
		if (indexRoomShow < 0)
		{
			indexRoomShow = rooms.Length - 1;
		}
	}

	public void ButtonShop()
	{
		if (showButtonShop)
		{
			windowShop.Open();
			windowMiniGames.Close();
			addoneWindowActive = true;
			buttonCloseWindows.SetActive(value: true);
			audioOpenShop.Play();
		}
	}

	public void ButtonMiniGames()
	{
		if (showButtonGames)
		{
			windowShop.Close();
			windowMiniGames.Open();
			addoneWindowActive = true;
			buttonCloseWindows.SetActive(value: true);
			audioOpenMinigame.Play();
		}
	}

	public void ButtonCloseWindowButtons()
	{
		windowShop.Close();
		windowMiniGames.Close();
		addoneWindowActive = false;
		buttonCloseWindows.SetActive(value: false);
	}

	public void MoneyAdd(int x)
	{
		money += x;
		textMoney.text = money.ToString() ?? "";
		textEnergy.text = energy.ToString() ?? "";
	}

	public void MiniGamePlay(Tamagotchi_MiniGame _game)
	{
		playMinigame = true;
		minigameNow = _game;
		_game.StartGame(cameraTransform);
		ButtonCloseWindowButtons();
		if (_game.dontExit)
		{
			buttonCloseMiniGame.SetActive(value: false);
		}
		else
		{
			buttonCloseMiniGame.SetActive(value: true);
		}
	}

	public void MiniGameStop()
	{
		if (minigameNow != null && showInterface)
		{
			frameAddMoney.SetActive(value: false);
			minigameNow.StopGame();
			playMinigame = false;
			CameraFix();
			imageScreen.color = new Color(0f, 0f, 0f, 1f);
			buttonCloseMiniGame.SetActive(value: false);
			minigameNow = null;
		}
	}

	public void EnergyAlphaAnimation()
	{
		timeEnergyAlpha = 0f;
	}

	public void NewDay()
	{
		energy = Random.Range(15, 30);
		lineHunger.sizeDelta = new Vector2(Random.Range(80, 115), 20f);
		lineMood.sizeDelta = new Vector2(Random.Range(80, 115), 20f);
		MoneyAdd(0);
	}

	public void DayMiniGame()
	{
		energy = 20;
		lineHunger.sizeDelta = new Vector2(Random.Range(80, 115), 20f);
		lineMood.sizeDelta = new Vector2(Random.Range(80, 115), 20f);
		MoneyAdd(0);
	}

	public void ShowInterface(bool x)
	{
		showInterface = x;
	}

	public void ShowButtonGames(bool x)
	{
		showButtonGames = x;
		buttonGame.GetComponent<ButtonMouseClick>().LockClick(!x);
	}

	public void ShowButtonShop(bool x)
	{
		showButtonShop = x;
		buttonShop.GetComponent<ButtonMouseClick>().LockClick(!x);
	}

	public void HideFastBlackScreen(bool x)
	{
		sprBlackScreenHide = x;
		if (sprBlackScreenHide)
		{
			spriteBlackScreen.color = new Color(spriteBlackScreen.color.r, spriteBlackScreen.color.g, spriteBlackScreen.color.b, 1f);
		}
		else
		{
			spriteBlackScreen.color = new Color(spriteBlackScreen.color.r, spriteBlackScreen.color.g, spriteBlackScreen.color.b, 0f);
		}
	}

	public void HideBlackScreen(bool x)
	{
		sprBlackScreenHide = x;
	}

	public void UpdateCasesBuy()
	{
		for (int i = 0; i < casesBuy.Length; i++)
		{
			casesBuy[i].UpdateColorBuy();
		}
	}

	public void CameraChangeRoomFast(int _index)
	{
		indexRoomShow = _index;
		CameraFix();
	}

	public void AnimationCameraZoom()
	{
		animationCameraZoom = true;
	}

	private void CameraFix()
	{
		cameraTransform.GetComponent<Camera>().orthographic = true;
		cameraTransform.GetComponent<Camera>().orthographicSize = 5f;
		cameraTransform.GetComponent<Camera>().backgroundColor = new Color(0.21f, 0.15f, 0.22f);
		cameraTransform.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
		cameraTransform.localRotation = Quaternion.Euler(new Vector3(35f, 180f, 0f));
		cameraTransform.transform.localPosition = new Vector3(rooms[indexRoomShow].cameraPositionX, 12f, 15f);
	}
}
