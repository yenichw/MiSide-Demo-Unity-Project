using System.Collections.Generic;
using System.IO;
using Colorful;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

public class OptionsGame : MonoBehaviour
{
	public Font fontRussian;

	public Font fontOther;

	public List<AsyncOperation> unloadScenes;

	public List<AddonSettingObject> eventsAddonSettings;

	[Header("Глобальные установки")]
	public Texture2D textureRGBShadowWorld;

	private void Awake()
	{
		if (!GlobalGame.optionGameLoad)
		{
			GlobalGame.optionGameLoad = true;
			Application.targetFrameRate = Mathf.Max(60, Mathf.RoundToInt(Screen.currentResolution.refreshRate));
			Time.fixedDeltaTime = 1f / (float)Application.targetFrameRate;
			Object.DontDestroyOnLoad(base.gameObject);
			GlobalGame.VolumeGame = PlayerPrefs.GetFloat("Volume", 0.7f);
			AudioListener.volume = GlobalGame.VolumeGame;
			GlobalGame.mouseSpeed = PlayerPrefs.GetFloat("MouseSpeed", 0f);
			GlobalGame.Language = PlayerPrefs.GetString("Language", "English");
			ReloadLanguage();
			if (PlayerPrefs.GetInt("MouseLerp", 0) == 0)
			{
				GlobalGame.mouseSpeedLerp = false;
			}
			else
			{
				GlobalGame.mouseSpeedLerp = true;
			}
			if (PlayerPrefs.GetInt("EverSub", 0) == 0)
			{
				GlobalGame.everSub = false;
			}
			else
			{
				GlobalGame.everSub = true;
			}
			if (PlayerPrefs.GetInt("HintScreen", 1) == 0)
			{
				GlobalGame.hintScreen = false;
			}
			else
			{
				GlobalGame.hintScreen = true;
			}
			if (PlayerPrefs.GetInt("HeadMove", 1) == 0)
			{
				GlobalGame.headMove = false;
			}
			else
			{
				GlobalGame.headMove = true;
			}
			GlobalGame.voicePlayer = PlayerPrefs.GetInt("VoicePlayer", 0);
			GetComponent<AchievementsController>().StartComponent();
			GlobalGame.playerFov = PlayerPrefs.GetFloat("FOV", 60f);
			GlobalGame.namePlayer = PlayerPrefs.GetString("NamePlayer", "???");
			GlobalTag.gameOptions = base.gameObject;
			ShaderSet();
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	private void Start()
	{
		ReloadOptions();
	}

	public void ReloadOptions()
	{
		Component[] array = Object.FindObjectsOfType<Camera>();
		Component[] array2 = array;
		if (array2.Length != 0)
		{
			for (int i = 0; i < array2.Length; i++)
			{
				if (!(array2[i].tag == "MainCamera") || !(array2[i].transform.Find("PP") != null))
				{
					continue;
				}
				//UpdateCameraPostProcessing(array2[i].transform.Find("PP").GetComponent<PostProcessVolume>());
				BrightnessContrastGamma brightnessContrastGamma = null;
				if (array2[i].GetComponent<BrightnessContrastGamma>() != null)
				{
					brightnessContrastGamma = array2[i].GetComponent<BrightnessContrastGamma>();
				}
				else if (array2[i].transform.Find("CameraPersons") != null)
				{
					brightnessContrastGamma = array2[i].transform.Find("CameraPersons").GetComponent<BrightnessContrastGamma>();
				}
				if (brightnessContrastGamma != null)
				{
					if (PlayerPrefs.GetInt("Bright", 0) != 0)
					{
						brightnessContrastGamma.enabled = true;
						brightnessContrastGamma.Brightness = PlayerPrefs.GetInt("Bright", 0);
					}
					else
					{
						brightnessContrastGamma.enabled = false;
					}
				}
			}
		}
		if (PlayerPrefs.GetInt("Shadow", 3) == 0)
		{
			GlobalGame.shadow = 0;
			QualitySettings.shadowResolution = ShadowResolution.Low;
		}
		if (PlayerPrefs.GetInt("Shadow", 3) == 1)
		{
			GlobalGame.shadow = 1;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
		}
		if (PlayerPrefs.GetInt("Shadow", 3) == 2)
		{
			GlobalGame.shadow = 2;
			QualitySettings.shadowResolution = ShadowResolution.High;
		}
		if (PlayerPrefs.GetInt("Shadow", 3) == 3)
		{
			GlobalGame.shadow = 3;
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
		}
		array = Object.FindObjectsOfType<Options_Light>();
		array2 = array;
		if (array2.Length != 0)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].GetComponent<Options_Light>().StartSeetings();
			}
		}
		if (PlayerPrefs.GetInt("QualityWorld", 2) == 0)
		{
			GlobalGame.qualityWorld = 0;
		}
		if (PlayerPrefs.GetInt("QualityWorld", 2) == 1)
		{
			GlobalGame.qualityWorld = 1;
		}
		if (PlayerPrefs.GetInt("QualityWorld", 2) == 2)
		{
			GlobalGame.qualityWorld = 2;
		}
		array = Object.FindObjectsOfType<Options_QualityWorld>();
		array2 = array;
		if (array2.Length != 0)
		{
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k].GetComponent<Options_QualityWorld>().StartSeetings();
			}
		}
		if (PlayerPrefs.GetInt("Antialiasing", 1) == 3)
		{
			QualitySettings.antiAliasing = 8;
		}
		if (PlayerPrefs.GetInt("Antialiasing", 1) == 2)
		{
			QualitySettings.antiAliasing = 4;
		}
		if (PlayerPrefs.GetInt("Antialiasing", 1) == 1)
		{
			QualitySettings.antiAliasing = 2;
		}
		if (PlayerPrefs.GetInt("Antialiasing", 1) == 0)
		{
			QualitySettings.antiAliasing = 0;
		}
		QualitySettings.masterTextureLimit = 0;
		if (PlayerPrefs.GetInt("WindowMode", 0) == 1)
		{
			Screen.fullScreen = false;
		}
		else
		{
			Screen.fullScreen = true;
		}
		if (PlayerPrefs.GetInt("VSync", 0) == 1)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
		array = Object.FindObjectsOfType<AudioVoicePlayer>();
		array2 = array;
		if (array2.Length != 0)
		{
			for (int l = 0; l < array2.Length; l++)
			{
				array2[l].GetComponent<AudioVoicePlayer>().ResetVoice();
			}
		}
		if (eventsAddonSettings == null || eventsAddonSettings.Count <= 0)
		{
			return;
		}
		for (int m = 0; m < eventsAddonSettings.Count; m++)
		{
			if (eventsAddonSettings[m].objectTarget != null)
			{
				eventsAddonSettings[m].eventReset.Invoke();
			}
		}
	}

	public void ReloadLanguage()
	{
		string text = "Data/Languages/" + GlobalGame.Language;
		string[] files = Directory.GetFiles(text, "*.txt");
		List<LanguageFilesText> list = new List<LanguageFilesText>();
		for (int i = 0; i < files.Length; i++)
		{
			files[i] = files[i].Remove(files[i].Length - 4, 4);
			files[i] = files[i].Remove(0, text.Length + 1);
			list.Add(new LanguageFilesText());
			list[i].name = files[i];
			list[i].strings = File.ReadAllLines(text + "/" + files[i] + ".txt");
			for (int j = 0; j < list[i].strings.Length; j++)
			{
				if (list[i].strings[j].IndexOf(" //") >= 0)
				{
					list[i].strings[j] = list[i].strings[j].Substring(0, list[i].strings[j].IndexOf(" //"));
				}
				else if (list[i].strings[j].IndexOf("//") >= 0)
				{
					list[i].strings[j] = list[i].strings[j].Substring(0, list[i].strings[j].IndexOf("//"));
				}
			}
		}
		GlobalLanguage.languageText = list;
		GlobalGame.fontUse = fontOther;
		if (GlobalGame.Language == "Russian")
		{
			GlobalGame.fontUse = fontRussian;
		}
		if (GlobalGame.Language == "English")
		{
			GlobalGame.fontUse = fontRussian;
		}
		if (GlobalGame.Language == "Romanian")
		{
			GlobalGame.fontUse = fontRussian;
		}
	}

	public void AddResetSetting(UnityEvent _eventReset, GameObject _object)
	{
		AddonSettingObject addonSettingObject = new AddonSettingObject();
		addonSettingObject.eventReset = _eventReset;
		addonSettingObject.objectTarget = _object;
		List<AddonSettingObject> list = new List<AddonSettingObject>();
		list.Add(addonSettingObject);
		for (int i = 0; i < eventsAddonSettings.Count; i++)
		{
			if (eventsAddonSettings[i].objectTarget != null)
			{
				list.Add(eventsAddonSettings[i]);
			}
		}
		eventsAddonSettings = list;
	}

	private void UpdateCameraPostProcessing(PostProcessVolume _ppv)
	{
		if (PlayerPrefs.GetInt("PostProcessing", 1) == 1)
		{
			_ppv.enabled = true;
		}
		else
		{
			_ppv.enabled = false;
		}
		if (PlayerPrefs.GetInt("ColorEffects", 1) == 1)
		{
			_ppv.profile.GetSetting<ColorGrading>().lift.value = new Vector4(0.97f, 0.97f, 1f, 0f);
		}
		else
		{
			_ppv.profile.GetSetting<ColorGrading>().lift.value = new Vector4(1f, 1f, 1f, 0f);
		}
		if (PlayerPrefs.GetInt("Bloom", 1) == 1)
		{
			_ppv.profile.GetSetting<Bloom>().active = true;
		}
		else
		{
			_ppv.profile.GetSetting<Bloom>().active = false;
		}
		if (PlayerPrefs.GetInt("AO", 1) == 1)
		{
			_ppv.profile.GetSetting<AmbientOcclusion>().active = true;
		}
		else
		{
			_ppv.profile.GetSetting<AmbientOcclusion>().active = false;
		}
	}

	[ContextMenu("Установить глобальные шейдеры")]
	public void ShaderSet()
	{
		Shader.SetGlobalTexture("TextureDraw", textureRGBShadowWorld);
		ConsoleMain.ConsolePrintGame("GlobalShader.");
	}
}
