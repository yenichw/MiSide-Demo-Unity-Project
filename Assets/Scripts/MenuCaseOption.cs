using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuCaseOption : MonoBehaviour
{
	public enum TypeCaseOption
	{
		Volume = 0,
		PostProcessing = 1,
		ColorEffect = 2,
		AO = 3,
		VSync = 4,
		Antialiasing = 5,
		Resolution = 6,
		WindowMode = 7,
		Language = 8,
		SpeedMouse = 9,
		SpeedMouseLerp = 10,
		EverSub = 11,
		Bright = 12,
		HeadMove = 13,
		HintScreen = 14,
		VoicePlayer = 15,
		Shadow = 16,
		Particles = 17,
		Bloom = 18,
		QualityWorld = 19,
		Loadgame = 20,
		FOV = 21,
		ExitGame = 22
	}

	public TypeCaseOption option;

	[Header("Addone")]
	public GameObject[] objectsAddone;

	public GameObject locationChangeOption;

	public List<Interface_ChangeScreenButton_Class_ButtonInfo> scrIccb;

	public Sprite spriteToggleY;

	public Sprite spriteToggleN;

	public float defaultSlide;

	private int intiResolution;

	private Resolution[] resolutions;

	private string[] languages;

	private int iLanguage;

	private float secondF;

	private int frames;

	private bool inputPosition;

	private float timeStartSec;

	private int timeStart;

	private Slider slider;

	private bool sliderFloat;

	private float sliderInt;

	private GameObject toggleImg;

	private int toggleActive;

	private ButtonMouseClick scrbmc;

	private bool firstStart;

	private void OnEnable()
	{
		timeStart = 3;
		timeStartSec = 1f;
		if (!firstStart)
		{
			firstStart = true;
			if (base.transform.Find("Slider") != null)
			{
				slider = base.transform.Find("Slider").GetComponent<Slider>();
			}
			if (base.transform.Find("Case/CaseCheck") != null)
			{
				toggleImg = base.transform.Find("Case/CaseCheck").gameObject;
			}
			if (GetComponent<ButtonMouseClick>() != null)
			{
				scrbmc = GetComponent<ButtonMouseClick>();
				scrbmc.Start();
			}
			if (option == TypeCaseOption.Resolution)
			{
				int num = 0;
				int num2 = 0;
				resolutions = Screen.resolutions;
				for (int i = 0; i < resolutions.Length; i++)
				{
					if (resolutions[i].width == PlayerPrefs.GetInt("XScreen", resolutions[resolutions.Length - 1].width) && resolutions[i].height == PlayerPrefs.GetInt("XScreen", resolutions[resolutions.Length - 1].height))
					{
						intiResolution = i;
					}
					if (num != resolutions[i].width && num2 != resolutions[i].height)
					{
						num = resolutions[i].width;
						num2 = resolutions[i].height;
						Interface_ChangeScreenButton_Class_ButtonInfo item = new Interface_ChangeScreenButton_Class_ButtonInfo
						{
							buttonText = resolutions[i].width + ":" + resolutions[i].height,
							value_int = i
						};
						scrIccb.Add(item);
					}
				}
				base.transform.Find("Text Resolution").GetComponent<Text>().text = PlayerPrefs.GetInt("XScreen", resolutions[intiResolution].width) + ":" + PlayerPrefs.GetInt("YScreen", resolutions[intiResolution].height);
			}
			if (option == TypeCaseOption.WindowMode)
			{
				toggleActive = PlayerPrefs.GetInt("WindowMode", 0);
			}
			if (option == TypeCaseOption.VSync)
			{
				toggleActive = PlayerPrefs.GetInt("VSync", 0);
			}
			if (option == TypeCaseOption.PostProcessing)
			{
				toggleActive = PlayerPrefs.GetInt("PostProcessing", 1);
			}
			if (option == TypeCaseOption.Bright)
			{
				sliderInt = PlayerPrefs.GetInt("Bright", 0);
			}
			if (option == TypeCaseOption.ColorEffect)
			{
				toggleActive = PlayerPrefs.GetInt("ColorEffects", 1);
			}
			if (option == TypeCaseOption.AO)
			{
				toggleActive = PlayerPrefs.GetInt("AO", 1);
			}
			if (option == TypeCaseOption.Bloom)
			{
				toggleActive = PlayerPrefs.GetInt("Bloom", 1);
			}
			if (option == TypeCaseOption.Antialiasing)
			{
				sliderInt = PlayerPrefs.GetInt("Antialiasing", 1);
			}
			if (option == TypeCaseOption.Shadow)
			{
				sliderInt = PlayerPrefs.GetInt("Shadow", 2);
			}
			if (option == TypeCaseOption.QualityWorld)
			{
				sliderInt = PlayerPrefs.GetInt("QualityWorld", 2);
			}
			if (option == TypeCaseOption.Volume)
			{
				sliderFloat = true;
				sliderInt = PlayerPrefs.GetFloat("Volume", 1f);
			}
			if (option == TypeCaseOption.Language)
			{
				languages = Directory.GetDirectories("Data/Languages");
				for (int j = 0; j < languages.Length; j++)
				{
					languages[j] = languages[j].Replace("Data/Languages" + Path.DirectorySeparatorChar, "");
				}
				base.transform.Find("Text Language").GetComponent<Text>().text = PlayerPrefs.GetString("Language", "English");
				for (int k = 0; k < languages.Length; k++)
				{
					Interface_ChangeScreenButton_Class_ButtonInfo item2 = new Interface_ChangeScreenButton_Class_ButtonInfo
					{
						buttonText = languages[k],
						value_int = k
					};
					scrIccb.Add(item2);
				}
			}
			if (option == TypeCaseOption.SpeedMouse)
			{
				sliderFloat = true;
				sliderInt = 1f + PlayerPrefs.GetFloat("MouseSpeed", 0f);
			}
			if (option == TypeCaseOption.SpeedMouseLerp)
			{
				toggleActive = PlayerPrefs.GetInt("MouseLerp", 0);
			}
			if (option == TypeCaseOption.EverSub)
			{
				toggleActive = PlayerPrefs.GetInt("EverSub", 0);
			}
			if (option == TypeCaseOption.HintScreen)
			{
				toggleActive = PlayerPrefs.GetInt("HintScreen", 1);
			}
			if (option == TypeCaseOption.HeadMove)
			{
				toggleActive = PlayerPrefs.GetInt("HeadMove", 1);
			}
			if (option == TypeCaseOption.VoicePlayer)
			{
				sliderFloat = true;
				sliderInt = PlayerPrefs.GetInt("VoicePlayer", 0);
			}
			if (option == TypeCaseOption.FOV)
			{
				sliderFloat = true;
				sliderInt = PlayerPrefs.GetFloat("FOV", 60f);
			}
			UpdateCase();
		}
		if (option == TypeCaseOption.VSync)
		{
			frames = 0;
			secondF = 0f;
			objectsAddone[0].GetComponent<Text>().text = "FPS";
		}
	}

	private void Update()
	{
		if (scrbmc != null && scrbmc.changeNow)
		{
			if (Input.GetAxis("Horizontal") == 0f)
			{
				inputPosition = false;
			}
			if (!inputPosition)
			{
				if ((double)Input.GetAxis("Horizontal") < -0.5)
				{
					inputPosition = true;
					if (slider != null)
					{
						if (sliderInt >= 0f)
						{
							slider.value -= 1f;
						}
						else
						{
							slider.value -= 0.1f;
						}
					}
					if (toggleImg != null)
					{
						Click();
					}
				}
				if ((double)Input.GetAxis("Horizontal") > 0.5)
				{
					inputPosition = true;
					if (slider != null)
					{
						if (sliderInt >= 0f)
						{
							slider.value += 1f;
						}
						else
						{
							slider.value += 0.1f;
						}
					}
					if (toggleImg != null)
					{
						Click();
					}
				}
			}
		}
		if (timeStart > 0)
		{
			timeStart--;
		}
		if (timeStartSec > 0f)
		{
			timeStartSec -= Time.fixedDeltaTime;
			if (timeStartSec <= 0f)
			{
				timeStartSec = 0f;
				if (option == TypeCaseOption.PostProcessing)
				{
					toggleActive = PlayerPrefs.GetInt("PostProcessing", 1);
					if (toggleActive == 0)
					{
						for (int i = 0; i < objectsAddone.Length; i++)
						{
							objectsAddone[i].GetComponent<ButtonMouseClick>().ActivationInteractive(x: false);
						}
					}
					else
					{
						for (int j = 0; j < objectsAddone.Length; j++)
						{
							objectsAddone[j].GetComponent<ButtonMouseClick>().ActivationInteractive(x: true);
						}
					}
				}
			}
		}
		if (option == TypeCaseOption.VSync)
		{
			secondF += Time.unscaledDeltaTime;
			frames++;
			if (secondF >= 1f)
			{
				objectsAddone[0].GetComponent<Text>().text = "FPS [" + frames + "]";
				frames = 0;
				secondF = 0f;
			}
		}
	}

	public void Click()
	{
		bool flag = false;
		if (slider != null)
		{
			if (!sliderFloat)
			{
				sliderInt = Mathf.FloorToInt(slider.value);
			}
			else
			{
				sliderInt = slider.value;
			}
		}
		if (toggleImg != null)
		{
			if (toggleActive == 1)
			{
				toggleActive = 0;
			}
			else
			{
				toggleActive = 1;
			}
		}
		if (option == TypeCaseOption.Resolution)
		{
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().eventReturn = new UnityEvent();
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().eventReturn.AddListener(ChangeResoulution);
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().Create(base.transform.Find("Text").GetComponent<Text>().text, scrIccb, Interface_ChangeScreenButton.TypeChangeScreenButton.ReturnInt, base.transform.parent.gameObject, base.gameObject);
		}
		if (option == TypeCaseOption.WindowMode)
		{
			flag = true;
			PlayerPrefs.SetInt("WindowMode", toggleActive);
		}
		if (option == TypeCaseOption.VSync)
		{
			flag = true;
			PlayerPrefs.SetInt("VSync", toggleActive);
			frames = 0;
			secondF = 0f;
			objectsAddone[0].GetComponent<Text>().text = "FPS";
		}
		if (option == TypeCaseOption.PostProcessing)
		{
			flag = true;
			PlayerPrefs.SetInt("PostProcessing", toggleActive);
			if (toggleActive == 0)
			{
				for (int i = 0; i < objectsAddone.Length; i++)
				{
					objectsAddone[i].GetComponent<ButtonMouseClick>().ActivationInteractive(x: false);
				}
			}
			else
			{
				for (int j = 0; j < objectsAddone.Length; j++)
				{
					objectsAddone[j].GetComponent<ButtonMouseClick>().ActivationInteractive(x: true);
				}
			}
		}
		if (option == TypeCaseOption.Bright)
		{
			flag = true;
			PlayerPrefs.SetInt("Bright", (int)sliderInt);
		}
		if (option == TypeCaseOption.ColorEffect)
		{
			flag = true;
			PlayerPrefs.SetInt("ColorEffects", toggleActive);
		}
		if (option == TypeCaseOption.AO)
		{
			flag = true;
			PlayerPrefs.SetInt("AO", toggleActive);
		}
		if (option == TypeCaseOption.Bloom)
		{
			flag = true;
			PlayerPrefs.SetInt("Bloom", toggleActive);
		}
		if (option == TypeCaseOption.Antialiasing)
		{
			flag = true;
			PlayerPrefs.SetInt("Antialiasing", (int)sliderInt);
		}
		if (option == TypeCaseOption.Shadow)
		{
			flag = true;
			PlayerPrefs.SetInt("Shadow", (int)sliderInt);
		}
		if (option == TypeCaseOption.QualityWorld)
		{
			flag = true;
			PlayerPrefs.SetInt("QualityWorld", (int)sliderInt);
		}
		if (option == TypeCaseOption.Volume)
		{
			GlobalGame.VolumeGame = slider.value;
			AudioListener.volume = GlobalGame.VolumeGame;
			PlayerPrefs.SetFloat("Volume", GlobalGame.VolumeGame);
		}
		if (option == TypeCaseOption.Language)
		{
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().eventReturn = new UnityEvent();
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().eventReturn.AddListener(ChangeLanguage);
			locationChangeOption.GetComponent<Interface_ChangeScreenButton>().Create(base.transform.Find("Text").GetComponent<Text>().text, scrIccb, Interface_ChangeScreenButton.TypeChangeScreenButton.ReturnInt, base.transform.parent.gameObject, base.gameObject);
		}
		if (option == TypeCaseOption.SpeedMouse)
		{
			GlobalGame.mouseSpeed = slider.value - 1f;
			PlayerPrefs.SetFloat("MouseSpeed", GlobalGame.mouseSpeed);
		}
		if (option == TypeCaseOption.SpeedMouseLerp)
		{
			if (toggleActive == 0)
			{
				GlobalGame.mouseSpeedLerp = false;
			}
			else
			{
				GlobalGame.mouseSpeedLerp = true;
			}
			PlayerPrefs.SetInt("MouseLerp", toggleActive);
		}
		if (option == TypeCaseOption.EverSub)
		{
			if (toggleActive == 0)
			{
				GlobalGame.everSub = false;
			}
			else
			{
				GlobalGame.everSub = true;
			}
			PlayerPrefs.SetInt("EverSub", toggleActive);
		}
		if (option == TypeCaseOption.HintScreen)
		{
			if (toggleActive == 0)
			{
				GlobalGame.hintScreen = false;
			}
			else
			{
				GlobalGame.hintScreen = true;
			}
			PlayerPrefs.SetInt("HintScreen", toggleActive);
		}
		if (option == TypeCaseOption.HeadMove)
		{
			if (toggleActive == 0)
			{
				GlobalGame.headMove = false;
			}
			else
			{
				GlobalGame.headMove = true;
			}
			PlayerPrefs.SetInt("HeadMove", toggleActive);
		}
		if (option == TypeCaseOption.VoicePlayer)
		{
			flag = true;
			GlobalGame.voicePlayer = Mathf.FloorToInt(slider.value);
			PlayerPrefs.SetInt("VoicePlayer", GlobalGame.voicePlayer);
			if (timeStart == 0)
			{
				GetComponent<AudioSource>().clip = (Resources.Load("DataVoicePlayer") as GameObject).GetComponent<Audio_Data>().sounds[GlobalGame.voicePlayer];
				GetComponent<AudioSource>().Play();
			}
		}
		if (option == TypeCaseOption.FOV)
		{
			GlobalGame.playerFov = slider.value;
			PlayerPrefs.SetFloat("FOV", GlobalGame.playerFov);
			GlobalTag.player.GetComponent<PlayerMove>().UpdateSettingsCamera();
		}
		if (option == TypeCaseOption.ExitGame)
		{
			Application.Quit();
		}
		if (flag)
		{
			GameObject.FindWithTag("Game").GetComponent<OptionsGame>().ReloadOptions();
		}
		UpdateCase();
	}

	public void ChangeResoulution()
	{
		intiResolution = locationChangeOption.GetComponent<Interface_ChangeScreenButton>().returnInt;
		base.transform.Find("Text Resolution").GetComponent<Text>().text = resolutions[intiResolution].width + ":" + resolutions[intiResolution].height;
		PlayerPrefs.SetInt("XScreen", resolutions[intiResolution].width);
		PlayerPrefs.SetInt("YScreen", resolutions[intiResolution].height);
		Screen.SetResolution(resolutions[intiResolution].width, resolutions[intiResolution].height, Screen.fullScreen, 60);
	}

	public void ChangeLanguage()
	{
		iLanguage = locationChangeOption.GetComponent<Interface_ChangeScreenButton>().returnInt;
		base.transform.Find("Text Language").GetComponent<Text>().text = languages[iLanguage];
		GlobalGame.Language = languages[iLanguage];
		PlayerPrefs.SetString("Language", languages[iLanguage]);
		GameObject.FindWithTag("Game").GetComponent<OptionsGame>().ReloadLanguage();
		Component[] componentsInChildren = GameObject.FindWithTag("World").GetComponentsInChildren(typeof(Localization_UIText), includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetComponent<Localization_UIText>().TextTranslate();
		}
		componentsInChildren = GameObject.FindWithTag("World").GetComponentsInChildren(typeof(UI_TextFontLanguage), includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].GetComponent<UI_TextFontLanguage>().FontUpdate();
		}
	}

	public void ResetDefault()
	{
		sliderInt = defaultSlide;
		slider.value = sliderInt;
		Click();
	}

	public void MouseEnter()
	{
		if (option == TypeCaseOption.Loadgame)
		{
			if (scrbmc.interactable)
			{
				objectsAddone[0].GetComponent<Image>().sprite = spriteToggleY;
			}
			else
			{
				objectsAddone[0].GetComponent<Image>().sprite = spriteToggleN;
			}
		}
	}

	private void UpdateCase()
	{
		if (slider != null)
		{
			slider.value = sliderInt;
		}
		if (!(toggleImg != null))
		{
			return;
		}
		if (toggleActive == 1)
		{
			toggleImg.GetComponent<Image>().sprite = spriteToggleY;
			toggleImg.GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 30f);
			if (GetComponent<UI_Colors>() != null)
			{
				GetComponent<UI_Colors>().SetColorImage(toggleImg, new Color(1f, 1f, 1f, 1f));
			}
			else
			{
				toggleImg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
			}
		}
		else
		{
			toggleImg.GetComponent<Image>().sprite = spriteToggleN;
			toggleImg.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
			if (GetComponent<UI_Colors>() != null)
			{
				GetComponent<UI_Colors>().SetColorImage(toggleImg, new Color(0.6f, 0f, 0.6f, 1f));
			}
			else
			{
				toggleImg.GetComponent<Image>().color = new Color(0.6f, 0f, 0.6f, 1f);
			}
		}
	}
}
