using System.Collections;
using Coffee.UIEffects;
using Colorful;
using Kino;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public bool demo;

	[Space(20f)]
	public GameObject firstLocation;

	public RectTransform rectMenu;

	public AudioSource audioMusicMenu;

	[Header("Игра")]
	public string nameSceneNewGame;

	[Header("Достижения")]
	public Text textAchievementProgress;

	public Sprite spriteAchievementNoComplete;

	[Header("Загрузка игры")]
	public CaseLoadGame[] casesLoad;

	public ButtonMouseClick[] casesContinue;

	private bool newGame;

	[Header("Новая игра")]
	public UnityEvent eventStartNewGame;

	private float timeDatamoshGlitch;

	private bool alternative;

	[Header("Редкая альтернатива")]
	public Animator cameraAnimator;

	public RuntimeAnimatorController alternativeAnimatorCamera;

	public RuntimeAnimatorController alternativeAnimator;

	public AudioClip alternativeMusic;

	public ParticleSystem particleBackground;

	public MenuMita menuMita;

	public Datamosh datamosh;

	[Header("Финал игры")]
	public Animator animMita;

	public GameObject cutSceneEnding;

	public Transform cameraOrigin;

	public Transform cameraCutscene;

	public Transform scene;

	public UnityEvent eventStartEnding;

	public GameObject firstLocationAuthors;

	public GameObject startCase;

	public Text textNameCartridge;

	public Text textTimeGameplay;

	public Image screenCapture;

	[HideInInspector]
	public GameObject objectLocationLast;

	private bool firstOpenAchievement;

	private void Start()
	{
		GlobalGame.demo = demo;
		if (GlobalGame.gameEndingMenu)
		{
			if (!demo)
			{
				animMita.Play("Mita EndingFace", 0, 0f);
				cameraOrigin.GetComponent<Animator>().enabled = false;
				cameraOrigin.parent = cameraCutscene;
				cameraOrigin.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
				cutSceneEnding.SetActive(value: true);
				GetComponent<Time_Events>().EventsOnTime[0].time += 7f;
				for (int i = 1; i < GetComponent<Time_Events>().EventsOnTime.Length; i++)
				{
					GetComponent<Time_Events>().EventsOnTime[i].time += 8.5f;
				}
				textNameCartridge.text = GlobalGame.namePlayer;
				if (GlobalGame.screenCapture != null)
				{
					screenCapture.sprite = Sprite.Create(GlobalGame.screenCapture, new Rect(0f, 0f, GlobalGame.screenCapture.width, GlobalGame.screenCapture.height), new Vector2(0.5f, 0.5f), 100f);
				}
			}
			GlobalGame.gameEndingMenu = false;
			eventStartEnding.Invoke();
			firstLocation = firstLocationAuthors;
			firstLocation = firstLocationAuthors;
			GetComponent<ButtonMouseMenu>().startCase = startCase;
			if (GlobalGame.timeGameplay)
			{
				textTimeGameplay.GetComponent<Localization_UIText>().TextTranslate();
				string text = "";
				text = ((GlobalGame.timeH >= 10) ? (text + GlobalGame.timeH) : (text + "0" + GlobalGame.timeH));
				text = ((GlobalGame.timeM >= 10) ? (text + ":" + GlobalGame.timeM) : (text + ":0" + GlobalGame.timeM));
				text = ((!(GlobalGame.timeS < 10f)) ? (text + ":" + Mathf.Round(GlobalGame.timeS)) : (text + ":0" + Mathf.Round(GlobalGame.timeS)));
				textTimeGameplay.text += text;
			}
			else
			{
				Object.Destroy(textTimeGameplay.gameObject);
			}
		}
		else if (Random.Range(0, 450) == 100)
		{
			Alternative();
		}
		GetComponent<Time_Events>().YieldRestart();
		for (int j = 0; j < casesLoad.Length; j++)
		{
			if (!GlobalAM.ExistsData(casesLoad[j].nameSave))
			{
				casesLoad[j].caseButton.interactable = false;
				casesLoad[j].caseButton.transform.Find("Text").GetComponent<Text>().text = "?????????";
				Object.Destroy(casesLoad[j].caseButton.transform.Find("Text").GetComponent<Localization_UIText>());
			}
		}
		if (!GlobalAM.ExistsData("Continue"))
		{
			for (int k = 0; k < casesContinue.Length; k++)
			{
				casesContinue[k].interactable = false;
			}
		}
		textAchievementProgress.text = GlobalTag.gameOptions.GetComponent<AchievementsController>().ProgressAchievement() + "%";
		firstLocation.GetComponent<MenuLocation>().Active(x: true);
		objectLocationLast = firstLocation;
	}

	private void Update()
	{
		if (alternative)
		{
			timeDatamoshGlitch -= Time.deltaTime;
			if (timeDatamoshGlitch < 0f)
			{
				timeDatamoshGlitch = Random.Range(2f, 12f);
				datamosh.Glitch();
			}
		}
		if (newGame)
		{
			audioMusicMenu.volume -= Time.deltaTime * 0.5f;
		}
	}

	public void OpenNextLocation(GameObject objectLocation)
	{
		if (objectLocationLast != null)
		{
			objectLocationLast.GetComponent<MenuLocation>().Active(x: false);
		}
		if (objectLocation != null)
		{
			objectLocationLast = objectLocation;
			objectLocationLast.GetComponent<MenuLocation>().Active(x: true);
			objectLocationLast.gameObject.SetActive(value: true);
		}
	}

	public void StartAchievement()
	{
		if (firstOpenAchievement)
		{
			return;
		}
		firstOpenAchievement = true;
		DataAchievementsValues[] dataAchievements = GlobalTag.gameOptions.GetComponent<AchievementsController>().dataAchievements;
		GameObject gameObject = base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Ach Simple").gameObject;
		gameObject.SetActive(value: true);
		int num = -15;
		GameObject gameObject2 = null;
		for (int i = 0; i < dataAchievements.Length; i++)
		{
			if (!GlobalGame.demo || (GlobalGame.demo && dataAchievements[i].demo))
			{
				GameObject gameObject3 = Object.Instantiate(gameObject, gameObject.transform.parent);
				gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, num);
				gameObject3.transform.Find("TextName").GetComponent<Localization_UIText>().StringNumber = i + 1;
				if (dataAchievements[i].intNow == dataAchievements[i].intMax)
				{
					gameObject3.transform.Find("Icon/Image").GetComponent<Image>().sprite = dataAchievements[i].icon;
				}
				if (dataAchievements[i].intNow != dataAchievements[i].intMax)
				{
					gameObject3.transform.Find("TextName").GetComponent<Text>().color = gameObject3.transform.Find("TextName").GetComponent<Text>().color - new Color(0f, 0f, 0f, 0.5f);
					gameObject3.transform.Find("TextName").GetComponent<UIShiny>().enabled = false;
				}
				if (i == 0)
				{
					gameObject3.GetComponent<ButtonMouseClick>().changeUp = base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Button Back").GetComponent<ButtonMouseClick>();
					base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Button Back").GetComponent<ButtonMouseClick>().changeDown = gameObject3.GetComponent<ButtonMouseClick>();
				}
				if (i == dataAchievements.Length - 1)
				{
					gameObject3.GetComponent<ButtonMouseClick>().changeDown = base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Button Back").GetComponent<ButtonMouseClick>();
					base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Button Back").GetComponent<ButtonMouseClick>().changeUp = gameObject3.GetComponent<ButtonMouseClick>();
				}
				if (i > 0)
				{
					gameObject2.GetComponent<ButtonMouseClick>().changeDown = gameObject3.GetComponent<ButtonMouseClick>();
					gameObject3.GetComponent<ButtonMouseClick>().changeUp = gameObject2.GetComponent<ButtonMouseClick>();
				}
				gameObject2 = gameObject3;
				num -= 90;
			}
		}
		base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View").GetComponent<RectTransform>().sizeDelta = new Vector2(500f, Mathf.Clamp(-num + 65, 170, 650));
		Object.Destroy(gameObject);
		base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -num + 50);
		base.transform.Find("Canvas/FrameMenu/Location Achievements/Scroll View/Viewport/Content/Button Back").GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, num);
	}

	private void Alternative()
	{
		cameraAnimator.GetComponent<Glitch>().enabled = true;
		cameraAnimator.runtimeAnimatorController = alternativeAnimatorCamera;
		animMita.Play("CameraMenu Start", -1, 0f);
		animMita.runtimeAnimatorController = alternativeAnimator;
		animMita.Play("MitaStartIdle", -1, 0f);
		audioMusicMenu.clip = alternativeMusic;
		audioMusicMenu.Play();
		GetComponent<Material_ColorVariables>().colorAnimation = new Color(1f, 0f, 0f);
		particleBackground.startColor = new Color(0.5f, 0f, 0.1f);
		menuMita.Alternative();
		datamosh.enabled = true;
		timeDatamoshGlitch = 0.1f;
		alternative = true;
	}

	public void CutSceneEnd()
	{
		cameraOrigin.parent = scene;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void ButtonURL(string _url)
	{
		Application.OpenURL(_url);
	}

	public void ButtonNewGame()
	{
		newGame = true;
		eventStartNewGame.Invoke();
		GlobalAM.DeleteData("Continue");
		GlobalGame.timeGameplay = true;
		StartCoroutine(TimeButton("newgame"));
	}

	private IEnumerator TimeButton(string _button)
	{
		yield return new WaitForSeconds(1f);
		if (_button == "newgame")
		{
			GameObject.FindWithTag("Game").transform.Find("Interface/BlackScreen").GetComponent<BlackScreen>().NextLevel(nameSceneNewGame);
		}
	}

	public void ButtonLoadScene(string _nameScene)
	{
		GlobalGame.timeGameplay = false;
		StartCoroutine(TimeButtonLoadScene(_nameScene));
	}

	private IEnumerator TimeButtonLoadScene(string _nameScene)
	{
		yield return new WaitForSeconds(1f);
		GameObject.FindWithTag("Game").transform.Find("Interface/BlackScreen").GetComponent<BlackScreen>().NextLevel(_nameScene);
	}

	public void ButtonContinue()
	{
		GlobalGame.timeGameplay = true;
		StartCoroutine(TimeButtonLoadScene(GlobalAM.LoadData("Continue")[0]));
	}
}
