using System.Collections;
using System.Collections.Generic;
using System.IO;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConsoleInterface : MonoBehaviour
{
	public GameObject inputTextObject;

	public RectTransform textContent;

	public Text textInfo;

	public Text textString;

	public Text textCountChar;

	public string versionText;

	public GameObject[] buttonsDrop;

	public ConsoleCheats[] cheats;

	private Color colorConsole;

	private string typeColor = "red";

	private Text textTimer;

	private GameObject settingsObject;

	private int iLastCodeNow;

	private InputField inputField;

	[HideInInspector]
	public GameObject objectCallMe;

	private GameObject[] resourcesCasesObject;

	[HideInInspector]
	public int resourcesTypeCreate;

	[HideInInspector]
	public List<ConsoleHierarchyCase> hierarchyObjects;

	[HideInInspector]
	public List<ConsoleHierarchyCase> hierarchyObjectsNormal;

	private InputField hierarchyFind;

	private GameObject cameraFlyObject;

	private GameObject cameraOrigin;

	private AudioSource aud;

	private bool autoDown;

	private float timeStartConsoleInterface;

	[HideInInspector]
	public UnityEvent eventEnter;

	[HideInInspector]
	public UnityEvent eventEnterHelp;

	[HideInInspector]
	public GameObject[] objectWindow = new GameObject[100];

	private GameObject panelCloseAddoneWindows;

	private bool functionsCreate;

	private bool levelsCreate;

	[HideInInspector]
	public string codeEnter;

	private void Start()
	{
		settingsObject = base.transform.Find("MainPanel/Settings").gameObject;
		inputField = inputTextObject.GetComponent<InputField>();
		StartCoroutine(ChangeInput());
		panelCloseAddoneWindows = base.transform.Find("MainPanel/ConsoleCloseAddoneWindow").gameObject;
		textTimer = base.transform.Find("MainPanel/UpPanelInfoConsole/TimerGame").GetComponent<Text>();
		base.transform.Find("MainPanel/UpPanelInfoConsole/VersionConsole").GetComponent<Text>().text = versionText;
		base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").GetComponent<Scrollbar>().value = 0f;
		base.transform.Find("MainPanel/UpPanelInfoConsole/NameGame").GetComponent<Text>().text = "AIHASTO: " + Application.productName;
		base.transform.Find("MainPanel/UpPanelInfoConsole/NameLevel").GetComponent<Text>().text = "LEVEL: " + SceneManager.GetActiveScene().name;
		hierarchyFind = base.transform.Find("MainPanel/Hierarchy/InputFound").GetComponent<InputField>();
		aud = GetComponent<AudioSource>();
		autoDown = true;
		if (!Directory.Exists(Application.persistentDataPath + "/Save"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/Save");
		}
		if (!Directory.Exists(Application.persistentDataPath + "/Save/Console"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/Save/Console");
		}
		if (File.Exists(Application.persistentDataPath + "/Save/Console/Settings"))
		{
			string[] array = File.ReadAllLines(Application.persistentDataPath + "/Save/Console/Settings");
			if (array.Length > 2 && array[2] == "0")
			{
				autoDown = false;
			}
			if (array.Length > 3 && array[3] == "1")
			{
				ConsoleMain.debugUnity = true;
			}
			Recolor(array[1]);
		}
		else
		{
			Recolor("silver");
		}
		if (File.Exists(Application.persistentDataPath + "/Save/Console/Dev"))
		{
			ConsoleMain.dev = true;
		}
		base.transform.Find("MainPanel/Settings/ButtonAutoDown/Check/Image").gameObject.SetActive(autoDown);
		Object.Destroy(base.transform.Find("MainPanel/UpPanelInfoConsole/Unity Debug").gameObject);
	}

	private IEnumerator ChangeInput()
	{
		yield return new WaitForSeconds(0.1f);
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(inputTextObject);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.BackQuote))
		{
			Close();
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			EnterConsole();
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			iLastCodeNow--;
			if (iLastCodeNow < 0)
			{
				iLastCodeNow = ConsoleMain.console_iLastCode;
			}
			inputField.text = ConsoleMain.console_lastCodes[iLastCodeNow];
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			iLastCodeNow++;
			if (iLastCodeNow > ConsoleMain.console_iLastCode)
			{
				iLastCodeNow = 0;
			}
			inputField.text = ConsoleMain.console_lastCodes[iLastCodeNow];
		}
		textInfo.text = ConsoleMain.consoleText2 + ConsoleMain.consoleText;
		textContent.anchoredPosition -= new Vector2(0f, Input.GetAxis("Mouse ScrollWheel") * 200f);
		if (textContent.anchoredPosition.y < 0f)
		{
			textContent.anchoredPosition = new Vector2(0f, 0f);
		}
		if (textContent.anchoredPosition.y > 5000f)
		{
			textContent.anchoredPosition = new Vector2(0f, 5000f);
		}
		textString.text = "STRINGS: " + iLastCodeNow + "/" + ConsoleMain.console_iLastCode;
		textCountChar.text = "# " + (ConsoleMain.consoleText.Length + ConsoleMain.consoleText2.Length);
		string text = "TIME [";
		text = ((!(ConsoleCall.timeSceneHours < 10f)) ? (text + ConsoleCall.timeSceneHours) : (text + "0" + ConsoleCall.timeSceneHours));
		text = ((!(ConsoleCall.timeSceneMinute < 10f)) ? (text + ":" + ConsoleCall.timeSceneMinute) : (text + ":0" + ConsoleCall.timeSceneMinute));
		text = ((!(Mathf.Round(ConsoleCall.timeSceneSecond) < 10f)) ? (text + ":" + Mathf.Round(ConsoleCall.timeSceneSecond)) : (text + ":0" + Mathf.Round(ConsoleCall.timeSceneSecond)));
		textTimer.text = text + "]";
		timeStartConsoleInterface += Time.deltaTime;
		if (timeStartConsoleInterface < 0.5f && autoDown)
		{
			base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").GetComponent<Scrollbar>().value = 0f;
		}
	}

	private void OnDestroy()
	{
		ConsoleMain.active = false;
	}

	public void EnterConsole()
	{
		string text = "";
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(inputTextObject);
		if (inputField.text != "")
		{
			text = inputField.text.ToLower();
			string text2 = inputField.text;
			ConsoleMain.console_lastCodes[ConsoleMain.console_iLastCode] = text;
			ConsoleMain.console_iLastCode++;
			if (ConsoleMain.console_iLastCode > 99)
			{
				ConsoleMain.console_iLastCode = 0;
			}
			iLastCodeNow = ConsoleMain.console_iLastCode;
			ConsoleMain.ConsolePrint(inputField.text ?? "");
			inputField.text = "";
			if (text == "colorprint")
			{
				SoundPlay(0);
				ConsoleMain.ConsolePrintGame("GAME | ");
				ConsoleMain.ConsolePrintWarningAdd(" WARNING | ");
				ConsoleMain.ConsolePrintCheatAdd(" CHEAT | ");
				ConsoleMain.ConsolePrintAdd(" CLASSIC |");
				ConsoleMain.ConsolePrintAdd("< color =#02E8E0> SYSTEM |</color>");
			}
			if (text == "qiut")
			{
				Qiut();
			}
			if (text == "showhierarchy")
			{
				ShowHierarchy();
			}
			if (text == "mousehide")
			{
				MouseHide();
			}
			if (text == "help")
			{
				Help();
				eventEnterHelp.Invoke();
			}
			if (text == "clear")
			{
				Clear();
			}
			if (text == "trailer")
			{
				SoundPlay(0);
				GlobalGame.trailer = !GlobalGame.trailer;
				ConsoleMain.ConsolePrintGame("Trailer = " + GlobalGame.hideDialogue);
			}
			if (text == "hidedialogue")
			{
				SoundPlay(0);
				GlobalGame.hideDialogue = !GlobalGame.hideDialogue;
				ConsoleMain.ConsolePrintGame("Hide dialogue = " + GlobalGame.hideDialogue);
			}
			if (text == "sv")
			{
				SaveFile();
			}
			if (text == "camerafly")
			{
				CameraFly();
			}
			if (text == "playerprefsclear")
			{
				ConsoleMain.ConsolePrintCheat("Playerprefs clear.");
				PlayerPrefs.DeleteAll();
			}
			if (text == "showinfo")
			{
				ShowInformation();
			}
			if (text == "deletefilesave")
			{
				DeleteFileSave();
			}
			if (text == "unitydebug")
			{
				UnityDebug();
			}
			if (text == "hideui")
			{
				HideUI();
			}
			if (text == "light")
			{
				LightCam();
			}
			if (text == "dev")
			{
				ConsoleMain.dev = !ConsoleMain.dev;
				ConsoleMain.ConsolePrintCheat("Developer = " + ConsoleMain.dev + ".");
			}
			if (text == "devsave")
			{
				ConsoleMain.dev = true;
				SaveDev();
				ConsoleMain.ConsolePrintCheat("Developer is saved.");
			}
			if (text.Length > 5 && text.Substring(0, 5) == "time ")
			{
				SoundPlay(0);
				Time.timeScale = float.Parse(text.Substring(5, text.Length - 5));
				text = ((Time.timeScale != 1f) ? "time unknown" : "time 1");
			}
			if (text.Length > 10 && text.Substring(0, 10) == "levelload ")
			{
				SoundPlay(0);
				SceneManager.LoadScene(text.Substring(10, text.Length - 10) ?? "", LoadSceneMode.Single);
			}
			if (text.Length > 5 && text.Substring(0, 5) == "find ")
			{
				string @string = text.Substring(5, text.Length - 5);
				FindObject(@string);
			}
			if (text.Length > 8 && text.Substring(0, 8) == "findtag ")
			{
				string string2 = text2.Substring(8, text2.Length - 8);
				FindObjectTag(string2);
			}
			if (text.Length > 12 && text.Substring(0, 12) == "skinnedfind ")
			{
				string string3 = text2.Substring(12, text2.Length - 12);
				FindSkinnedMesh(string3);
			}
			if (ConsoleCommandsGame.Command(text))
			{
				SoundPlay(0);
			}
		}
		if (autoDown)
		{
			base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").GetComponent<Scrollbar>().value = 0f;
		}
		codeEnter = text;
		eventEnter.Invoke();
	}

	public void Clear()
	{
		SoundPlay(0);
		ConsoleMain.consoleText = "";
		ConsoleMain.consoleText2 = "";
		inputField.text = "";
		base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").gameObject.GetComponent<Scrollbar>().value = 0f;
	}

	public void Close()
	{
		Object.Destroy(base.gameObject);
	}

	public void MouseHide()
	{
		SoundPlay(0);
		CursorLockMode lockState;
		if (Cursor.visible)
		{
			lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		Cursor.lockState = lockState;
	}

	private void Qiut()
	{
		Application.Quit();
		Debug.Log("Quit");
	}

	public void ShowHierarchy()
	{
		SoundPlay(0);
		Component[] array = Object.FindObjectsOfType<Transform>();
		Component[] array2 = array;
		ConsoleMain.ConsolePrintGame("GameObject Length: " + array2.Length);
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].transform.parent == null)
			{
				ConsoleMain.ConsolePrint("<color=silver>" + array2[i].gameObject.name + "</color>");
			}
			else
			{
				ConsoleMain.ConsolePrintGame(array2[i].gameObject.name ?? "");
			}
		}
	}

	private void FindObject(string _string)
	{
		SoundPlay(0);
		Component[] array = Object.FindObjectsOfType<Transform>();
		Component[] array2 = array;
		bool flag = false;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].gameObject.name.ToLower() == _string)
			{
				ConsoleMain.ConsolePrint("<color=green>Object found.</color>");
				flag = true;
			}
		}
		if (!flag)
		{
			ConsoleMain.ConsolePrint("<color=red>Object not found.</color>");
		}
	}

	private void FindObjectTag(string _string)
	{
		SoundPlay(0);
		if (GameObject.FindWithTag(_string) != null)
		{
			ConsoleMain.ConsolePrint("<color=green>Object found.</color>");
		}
		else
		{
			ConsoleMain.ConsolePrint("<color=red>Object not found.</color>");
		}
	}

	public void Help()
	{
		SoundPlay(0);
		ConsoleMain.ConsolePrintCheat("HELP!!!");
		ConsoleMain.ConsolePrintCheat("<color=#02E8E0>System-------------------------------------------</color>\nhelp = показать все команды\nmousehide = включение и выключение курсора\nqiut = выключить игру\nplayerprefsclear = очистить реестр\nlevelload * = перейти на уровень *\ntime * = скорость *\nshowhierarchy = показать иерархию\nfind *= существует ли объект *\nfindtag *= существует ли объект по тегу *\ntrailer = режим трейлера\nskinnedfind * = показать все свойства меша\nshowinfo * = показать информацию\nresources = показать возможные ресурсы\nsv = сохранить записи консоли в файл\ndeletefilesave = удалить папку сохранений\ncamerafly = полёт камеры\nhideui = скрыть UI\n(отключено) playerprefssave * * = сохранить реестр * со значением *\n(отключено) listenervolume * = громкость *\n(отключено) destroyallscene = очистить всю сцену\n(для разработчика) colorprint = показать раскраску консольных команд\nlight = включить свет в камере\n" + ConsoleCommandsGame.Help());
		base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").gameObject.GetComponent<Scrollbar>().value = 0f;
	}

	private void FindSkinnedMesh(string _string)
	{
		SoundPlay(0);
		Component[] array = Object.FindObjectsOfType<SkinnedMeshRenderer>();
		Component[] array2 = array;
		ConsoleMain.ConsolePrint("Objects:" + array2.Length);
		GameObject[] array3 = new GameObject[10];
		int num = 0;
		bool flag = false;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].name.ToLower() == _string)
			{
				flag = true;
				array3[num] = array2[i].gameObject;
				num++;
			}
		}
		if (!flag)
		{
			ConsoleMain.ConsolePrint("<color=red>Object not found.</color>");
			return;
		}
		ConsoleMain.ConsolePrint("<color=green>Objects found:" + num + ". </color>");
		for (int j = 0; j < num; j++)
		{
			ConsoleMain.ConsolePrint("Name object: " + array3[j].name);
			ConsoleMain.ConsolePrint("Bones:");
			for (int k = 0; k < array3[j].GetComponent<SkinnedMeshRenderer>().bones.Length; k++)
			{
				ConsoleMain.ConsolePrint("Bones:" + array3[j].GetComponent<SkinnedMeshRenderer>().bones[k].name);
			}
		}
	}

	public void ShowInformation()
	{
		SoundPlay(0);
		objectCallMe.GetComponent<ConsoleCall>().ShowInformation();
	}

	private void DeleteFileSave()
	{
		if (Directory.Exists("Data") && Directory.Exists("Data/Save"))
		{
			Directory.Delete("Data/Save", recursive: true);
		}
		if (Directory.Exists(Application.persistentDataPath) && Directory.Exists(Application.persistentDataPath + "/Save"))
		{
			Directory.Delete(Application.persistentDataPath + "/Save", recursive: true);
		}
	}

	public void UnityDebug()
	{
		Debug.Log("Game unity test.");
		Debug.LogError("Game unity test.");
		Debug.LogWarning("Game unity test.");
	}

	private void HideUI()
	{
		SoundPlay(0);
		Component[] array = Object.FindObjectsOfType<Canvas>();
		Component[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.GetComponent<Canvas>().enabled = !array2[i].gameObject.GetComponent<Canvas>().enabled;
		}
		if (GameObject.FindWithTag("MainCamera") != null)
		{
			GameObject.FindWithTag("MainCamera").GetComponent<Outliner>().enabled = false;
		}
	}

	public void LightCam()
	{
		SoundPlay(0);
		cameraOrigin = Camera.main.gameObject;
		if (cameraOrigin.GetComponent<Light>() == null)
		{
			(cameraOrigin.AddComponent(typeof(Light)) as Light).type = LightType.Directional;
		}
		else
		{
			Object.Destroy(cameraOrigin.GetComponent<Light>());
		}
	}

	public void ClickFunction(int _indexCheat)
	{
		inputField.text = cheats[_indexCheat].cheat;
		EnterConsole();
	}

	private void HierarchyLoad()
	{
		Component[] array = Object.FindObjectsOfType<Transform>(includeInactive: true);
		Component[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (!HierarchyCheck(array2[i].gameObject))
			{
				ConsoleHierarchyCase consoleHierarchyCase = new ConsoleHierarchyCase();
				consoleHierarchyCase.objectOrigin = array2[i].gameObject;
				if (array2[i].gameObject.transform.parent != null)
				{
					consoleHierarchyCase.objectParent = array2[i].gameObject.transform.parent.gameObject;
				}
				hierarchyObjects.Add(consoleHierarchyCase);
			}
		}
		for (int j = 0; j < hierarchyObjects.Count; j++)
		{
			if (hierarchyObjects[j].objectParent == null)
			{
				hierarchyObjectsNormal.Add(hierarchyObjects[j]);
				if (hierarchyObjects[j].objectOrigin.transform.childCount > 0)
				{
					HierarchyChild(j);
				}
			}
		}
		for (int k = 0; k < hierarchyObjectsNormal.Count; k++)
		{
			for (int l = 0; l < hierarchyObjectsNormal.Count; l++)
			{
				if (hierarchyObjectsNormal[k].objectOrigin.transform.parent != null && hierarchyObjectsNormal[k].objectOrigin.transform.parent == hierarchyObjectsNormal[l].objectOrigin.transform)
				{
					hierarchyObjectsNormal[k].indexParent = l;
				}
			}
		}
		HierarchyUpdate();
	}

	private void HierarchyChild(int _index)
	{
		for (int i = 0; i < hierarchyObjects.Count; i++)
		{
			if (i != _index && hierarchyObjects[i].objectParent == hierarchyObjects[_index].objectOrigin)
			{
				hierarchyObjectsNormal.Add(hierarchyObjects[i]);
				if (hierarchyObjects[i].objectOrigin.transform.childCount > 0)
				{
					HierarchyChild(i);
				}
			}
		}
	}

	private bool HierarchyCheck(GameObject _object)
	{
		bool result = false;
		if (_object.GetComponent<ConsoleInterface>() == null)
		{
			while (_object != null)
			{
				if (_object.transform.parent == null)
				{
					_object = null;
				}
				else if (_object.transform.parent.GetComponent<ConsoleInterface>() != null)
				{
					result = true;
					_object = null;
				}
				else
				{
					_object = _object.transform.parent.gameObject;
				}
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	public void HierarchyFind()
	{
		string text = hierarchyFind.text.ToLower();
		for (int i = 0; i < hierarchyObjects.Count; i++)
		{
			if (text != "")
			{
				if (hierarchyObjects[i].objectOrigin.name.Length > text.Length)
				{
					hierarchyObjects[i].obejctCase.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
					hierarchyObjects[i].obejctCase.transform.Find("Text").GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
					for (int j = 0; j < text.Length; j++)
					{
						if (text[j] != hierarchyObjects[i].objectOrigin.name.ToLower()[j])
						{
							hierarchyObjects[i].obejctCase.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
							hierarchyObjects[i].obejctCase.transform.Find("Text").GetComponent<Text>().color = new Color(0f, 0f, 0f, 0.3f);
							break;
						}
					}
				}
				else
				{
					hierarchyObjects[i].obejctCase.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
					hierarchyObjects[i].obejctCase.transform.Find("Text").GetComponent<Text>().color = new Color(0f, 0f, 0f, 0.3f);
				}
			}
			else
			{
				hierarchyObjects[i].obejctCase.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
				hierarchyObjects[i].obejctCase.transform.Find("Text").GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
			}
		}
	}

	public void HierarchyUpdate()
	{
		for (int i = 0; i < hierarchyObjectsNormal.Count; i++)
		{
			if (hierarchyObjectsNormal[i].obejctCase != null)
			{
				Object.Destroy(hierarchyObjectsNormal[i].obejctCase);
			}
		}
		GameObject gameObject = base.transform.Find("MainPanel/Hierarchy/Scroll View/Viewport/Content/Button").gameObject;
		gameObject.gameObject.SetActive(value: true);
		for (int j = 0; j < hierarchyObjectsNormal.Count; j++)
		{
			if (hierarchyObjectsNormal[j].objectOrigin.transform.parent == null)
			{
				hierarchyObjectsNormal[j].obejctCase = Object.Instantiate(gameObject, gameObject.transform.parent);
				hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<Text>().text = hierarchyObjectsNormal[j].objectOrigin.name;
				if (hierarchyObjectsNormal[j].objectOrigin.transform.childCount == 0)
				{
					Object.Destroy(hierarchyObjectsNormal[j].obejctCase.transform.Find("BD").gameObject);
					hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, 0f);
					hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 15f);
				}
				hierarchyObjectsNormal[j].obejctCase.GetComponent<ConsoleHierarchyButton>().StartComponent(this, j);
			}
			else if (j > 0 && hierarchyObjectsNormal[hierarchyObjectsNormal[j].indexParent].open)
			{
				hierarchyObjectsNormal[j].obejctCase = Object.Instantiate(gameObject, gameObject.transform.parent);
				hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<Text>().text = hierarchyObjectsNormal[j].objectOrigin.name;
				if (hierarchyObjectsNormal[j].objectOrigin.transform.childCount == 0)
				{
					Object.Destroy(hierarchyObjectsNormal[j].obejctCase.transform.Find("BD").gameObject);
					hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, 0f);
					hierarchyObjectsNormal[j].obejctCase.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 15f);
				}
				hierarchyObjectsNormal[j].obejctCase.GetComponent<ConsoleHierarchyButton>().StartComponent(this, j);
			}
		}
		int num = -5;
		for (int k = 0; k < hierarchyObjectsNormal.Count; k++)
		{
			if (!(hierarchyObjectsNormal[k].obejctCase != null))
			{
				continue;
			}
			hierarchyObjectsNormal[k].obejctCase.GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, num);
			if (k > 0)
			{
				if (hierarchyObjectsNormal[k - 1].obejctCase != null && hierarchyObjectsNormal[k].objectParent == hierarchyObjectsNormal[k - 1].objectOrigin)
				{
					hierarchyObjectsNormal[k].obejctCase.GetComponent<RectTransform>().anchoredPosition = new Vector2(hierarchyObjectsNormal[k - 1].obejctCase.GetComponent<RectTransform>().anchoredPosition.x + 20f, num);
				}
				else
				{
					for (int l = 0; l < k; l++)
					{
						if (k != l && hierarchyObjectsNormal[k].objectParent == hierarchyObjectsNormal[l].objectParent)
						{
							hierarchyObjectsNormal[k].obejctCase.GetComponent<RectTransform>().anchoredPosition = new Vector2(hierarchyObjectsNormal[l].obejctCase.GetComponent<RectTransform>().anchoredPosition.x, num);
						}
					}
				}
			}
			num -= 17;
		}
		gameObject.SetActive(value: false);
	}

	public void ResourcesType(int x)
	{
		resourcesTypeCreate = x;
		base.transform.Find("MainPanel/Resources/FrameName/Button Camera").GetComponent<Image>().color = new Color(0.764151f, 0.764151f, 0.764151f, 0f);
		base.transform.Find("MainPanel/Resources/FrameName/Button Player").GetComponent<Image>().color = new Color(0.764151f, 0.764151f, 0.764151f, 0f);
		if (x == 0)
		{
			base.transform.Find("MainPanel/Resources/FrameName/Button Camera").GetComponent<Image>().color = new Color(0.764151f, 0.764151f, 0.764151f, 1f);
		}
		if (x == 1)
		{
			base.transform.Find("MainPanel/Resources/FrameName/Button Player").GetComponent<Image>().color = new Color(0.764151f, 0.764151f, 0.764151f, 1f);
		}
	}

	public void Recolor(string colorSetting)
	{
		if (colorSetting == "silver")
		{
			colorConsole = new Color(0.2f, 0.2f, 0.2f);
		}
		if (colorSetting == "red")
		{
			colorConsole = new Color(1f, 0.2f, 0.2f);
		}
		if (colorSetting == "green")
		{
			colorConsole = new Color(0.2f, 0.5f, 0.3f);
		}
		if (colorSetting == "blue")
		{
			colorConsole = new Color(0.2f, 0.6f, 0.6f);
		}
		if (colorSetting == "purple")
		{
			colorConsole = new Color(0.5f, 0.2f, 0.6f);
		}
		base.transform.Find("MainPanel/UpPanel").GetComponent<Image>().color = colorConsole;
		base.transform.Find("MainPanel/UpPanelInfoConsole").GetComponent<Image>().color = new Color(colorConsole.r / 8f, colorConsole.g / 8f, colorConsole.b / 8f);
		base.transform.Find("MainPanel/Scroll View Information").GetComponent<Image>().color = colorConsole - new Color(0.3f, 0.3f, 0.3f, 0.2f);
		base.transform.Find("MainPanel/InputField").GetComponent<Image>().color = colorConsole;
		base.transform.Find("MainPanel/Scroll View Information/Scrollbar Vertical").GetComponent<Image>().color = colorConsole - new Color(0.3f, 0.3f, 0.3f, 0f);
		typeColor = colorSetting;
		SaveSettings();
	}

	public void SaveFile()
	{
		SoundPlay(0);
		if (!Directory.Exists("Data/Console"))
		{
			Directory.CreateDirectory("Data/Console");
		}
		int num = 1;
		while (File.Exists("Data/Console/Console Save " + num + ".txt"))
		{
			num++;
		}
		string consoleText = ConsoleMain.consoleText;
		consoleText = consoleText.Replace("</color>", "");
		consoleText = consoleText.Replace("<color=#abff50>", "");
		consoleText = consoleText.Replace("<color=#ff2030>", "");
		consoleText = consoleText.Replace("<color=#fb9920>", "");
		consoleText = consoleText.Replace("<color=#02E8E0>", "");
		using (StreamWriter streamWriter = new StreamWriter("Data/Console/Console Save " + num + ".txt"))
		{
			streamWriter.Write(consoleText);
		}
		ConsoleMain.ConsolePrintGame("File save: 'Console Save " + num + ".txt'");
	}

	public void CameraFly()
	{
		SoundPlay(0);
		if (cameraFlyObject != null)
		{
			Object.Destroy(cameraFlyObject);
			cameraOrigin.AddComponent<AudioListener>();
			return;
		}
		cameraOrigin = Camera.main.gameObject;
		cameraFlyObject = Object.Instantiate(Camera.main.gameObject);
		cameraFlyObject.gameObject.AddComponent<ConsoleCameraFly>();
		cameraFlyObject.GetComponent<ConsoleCameraFly>().xRot = cameraOrigin.transform.rotation.eulerAngles.y;
		cameraFlyObject.GetComponent<ConsoleCameraFly>().yRot = 0f - cameraOrigin.transform.rotation.eulerAngles.x;
		cameraFlyObject.transform.position = cameraOrigin.transform.position;
		Object.Destroy(cameraOrigin.GetComponent<AudioListener>());
	}

	public void AutoDown()
	{
		autoDown = !autoDown;
		base.transform.Find("MainPanel/Settings/ButtonAutoDown/Check/Image").gameObject.SetActive(autoDown);
		SaveSettings();
	}

	public void OpenSettings()
	{
		AddoneWindowCloseAll();
		AddoneWindowAdd(settingsObject);
		settingsObject.SetActive(value: true);
	}

	public void OpenFunctions()
	{
		AddoneWindowCloseAll();
		GameObject gameObject = base.transform.Find("MainPanel/Functions").gameObject;
		AddoneWindowAdd(gameObject);
		gameObject.SetActive(value: true);
		if (!functionsCreate)
		{
			functionsCreate = true;
			GameObject gameObject2 = base.transform.Find("MainPanel/Functions/Scroll View/Viewport/Content/Button").gameObject;
			for (int i = 0; i < cheats.Length; i++)
			{
				GameObject gameObject3 = Object.Instantiate(gameObject2, gameObject2.transform.parent);
				gameObject3.GetComponent<ConsoleFuctionsButton>().StartComponent(this, i);
				gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(gameObject3.GetComponent<RectTransform>().anchoredPosition.x, -5 + -35 * i);
				gameObject3.transform.Find("Text").GetComponent<Text>().text = cheats[i].nameCheat;
			}
			Object.Destroy(gameObject2);
		}
	}

	public void OpenAddons()
	{
		AddoneWindowCloseAll();
		GameObject gameObject = base.transform.Find("MainPanel/Addons").gameObject;
		AddoneWindowAdd(gameObject);
		gameObject.SetActive(value: true);
	}

	public void OpenResources()
	{
		AddoneWindowCloseAll();
		GameObject gameObject = base.transform.Find("MainPanel/Resources").gameObject;
		AddoneWindowAdd(gameObject);
		gameObject.SetActive(value: true);
		Object[] array = Resources.LoadAll("", typeof(GameObject));
		GameObject[] array2 = new GameObject[array.Length];
		resourcesCasesObject = array2;
		int num = -5;
		for (int i = 0; i < array.Length; i++)
		{
			resourcesCasesObject[i] = Object.Instantiate(base.transform.Find("MainPanel/Resources/Scroll View/Viewport/Content/TableObject").gameObject, base.transform.Find("MainPanel/Resources/Scroll View/Viewport/Content"));
			resourcesCasesObject[i].SetActive(value: true);
			resourcesCasesObject[i].transform.Find("Text").GetComponent<Text>().text = array[i].name;
			resourcesCasesObject[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num);
			resourcesCasesObject[i].GetComponent<ConsoleResourcesCase>().objectResource = array[i] as GameObject;
			resourcesCasesObject[i].GetComponent<ConsoleResourcesCase>().console = this;
			num -= 35;
		}
		base.transform.Find("MainPanel/Resources/Scroll View/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -num);
	}

	public void OpenHierarchy()
	{
		AddoneWindowCloseAll();
		GameObject gameObject = base.transform.Find("MainPanel/Hierarchy").gameObject;
		AddoneWindowAdd(gameObject);
		gameObject.SetActive(value: true);
		HierarchyLoad();
	}

	public void OpenLevels()
	{
		AddoneWindowCloseAll();
		GameObject gameObject = base.transform.Find("MainPanel/Levels").gameObject;
		AddoneWindowAdd(gameObject);
		gameObject.SetActive(value: true);
		if (!levelsCreate)
		{
			levelsCreate = true;
			GameObject gameObject2 = base.transform.Find("MainPanel/Levels/Scroll View/Viewport/Content/Button").gameObject;
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				GameObject gameObject3 = Object.Instantiate(gameObject2, gameObject2.transform.parent);
				gameObject3.transform.Find("ButtonLoad").GetComponent<ConsoleLevelsButton>().StartComponent(i);
				gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(gameObject3.GetComponent<RectTransform>().anchoredPosition.x, -5 + -35 * i);
			}
			Object.Destroy(gameObject2);
		}
	}

	public void ToggleDebugUnity()
	{
		ConsoleMain.debugUnity = !ConsoleMain.debugUnity;
		base.transform.Find("MainPanel/UpPanelInfoConsole/Unity Debug/Active").gameObject.SetActive(ConsoleMain.debugUnity);
		SaveSettings();
	}

	public void CheckLocalization()
	{
		ConsoleMain.ConsolePrintGame("Start check Localization, originial [Russian]...");
		List<LanguageFilesText> list = new List<LanguageFilesText>();
		list = LocalizatioLoad("Data/Languages/Russian");
		string[] directories = Directory.GetDirectories("Data/Languages");
		for (int i = 0; i < directories.Length; i++)
		{
			directories[i] = directories[i].Replace("Data/Languages" + Path.DirectorySeparatorChar, "");
			if (!(directories[i] != "Russian"))
			{
				continue;
			}
			List<LanguageFilesText> list2 = LocalizatioLoad("Data/Languages/" + directories[i]);
			ConsoleMain.ConsolePrintGame("load " + directories[i] + "... files [" + list2.Count + "/" + list.Count + "]...");
			bool flag = true;
			for (int j = 0; j < list2.Count; j++)
			{
				bool flag2 = true;
				for (int k = 0; k < list.Count; k++)
				{
					if (k < list.Count && list2[j].name == list[k].name)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					flag = false;
					ConsoleMain.ConsolePrintWarning("file unnecessary: " + list2[j].name);
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				int num = -1;
				int count = list.Count;
				if (list2.Count > list.Count)
				{
					count = list2.Count;
				}
				for (int m = 0; m < count; m++)
				{
					if (m >= list2.Count || !(list[l].name == list2[m].name))
					{
						continue;
					}
					num = m;
					if (list[l].strings.Length != list2[m].strings.Length)
					{
						flag = false;
						ConsoleMain.ConsolePrintWarning("the number of lines does not match [" + list2[m].strings.Length + "/" + list[l].strings.Length + "] " + list[l].name);
					}
					for (int n = 0; n < list[l].strings.Length; n++)
					{
						if (n < list2[m].strings.Length)
						{
							if (list[l].strings[n] != "" && list2[m].strings[n] == "")
							{
								flag = false;
								ConsoleMain.ConsolePrintWarning("string error [" + (n + 1) + "] " + list2[m].name);
							}
							if (list[l].strings[n] == "" && list2[m].strings[n] != "")
							{
								flag = false;
								ConsoleMain.ConsolePrintWarning("string error [" + (n + 1) + "] " + list2[m].name);
							}
						}
						else
						{
							flag = false;
							ConsoleMain.ConsolePrintWarning("string error [" + (n + 1) + "] " + list2[m].name);
						}
					}
					break;
				}
				if (num == -1)
				{
					flag = false;
					ConsoleMain.ConsolePrintWarning("file not found: " + list[l].name);
				}
			}
			if (flag)
			{
				ConsoleMain.ConsolePrintGameAdd(" perfect!");
			}
		}
		ConsoleMain.ConsolePrintGame("Ready check Localization.");
	}

	private string[] LocalizationClearText(string[] _text)
	{
		for (int i = 0; i < _text.Length; i++)
		{
			if (_text[i].IndexOf(" //") >= 0)
			{
				_text[i] = _text[i].Substring(0, _text[i].IndexOf(" //"));
			}
			else if (_text[i].IndexOf("//") >= 0)
			{
				_text[i] = _text[i].Substring(0, _text[i].IndexOf("//"));
			}
		}
		return _text;
	}

	private List<LanguageFilesText> LocalizatioLoad(string _path)
	{
		string[] files = Directory.GetFiles(_path, "*.txt");
		List<LanguageFilesText> list = new List<LanguageFilesText>();
		for (int i = 0; i < files.Length; i++)
		{
			files[i] = files[i].Remove(files[i].Length - 4, 4);
			files[i] = files[i].Remove(0, _path.Length + 1);
			list.Add(new LanguageFilesText());
			list[i].name = files[i];
			list[i].strings = File.ReadAllLines(_path + "/" + files[i] + ".txt");
			list[i].strings = LocalizationClearText(list[i].strings);
		}
		return list;
	}

	public void SaveSettings()
	{
		string text = "[Console settings]";
		text = text + "\n" + typeColor;
		text = (autoDown ? (text + "\n1") : (text + "\n0"));
		text = (ConsoleMain.debugUnity ? (text + "\n1") : (text + "\n0"));
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/Save/Console/Settings");
		streamWriter.Write(text);
	}

	private void SaveDev()
	{
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/Save/Console/Dev");
		streamWriter.Close();
	}

	public void SoundPlay(int soundInt)
	{
		aud.Play();
	}

	public void AddoneWindowAdd(GameObject _object)
	{
		for (int i = 0; i < objectWindow.Length; i++)
		{
			if (objectWindow[i] == null)
			{
				objectWindow[i] = _object;
				break;
			}
		}
		panelCloseAddoneWindows.SetActive(value: true);
	}

	public void AddoneWindowCloseAll()
	{
		bool flag = false;
		for (int i = 0; i < objectWindow.Length; i++)
		{
			if (objectWindow[i] != null)
			{
				objectWindow[i].GetComponent<ConsoleAddoneWindow>().CloseWindow();
				objectWindow[i] = null;
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		panelCloseAddoneWindows.SetActive(value: false);
		ChangeButtonOpen(null);
		if (resourcesCasesObject != null)
		{
			for (int j = 0; j < resourcesCasesObject.Length; j++)
			{
				if (resourcesCasesObject[j] != null)
				{
					Object.Destroy(resourcesCasesObject[j]);
				}
			}
			resourcesCasesObject = null;
		}
		if (hierarchyObjects.Count <= 0)
		{
			return;
		}
		for (int k = 0; k < hierarchyObjects.Count; k++)
		{
			if (hierarchyObjects[k].obejctCase != null)
			{
				Object.Destroy(hierarchyObjects[k].obejctCase);
			}
		}
		hierarchyObjects.Clear();
		for (int l = 0; l < hierarchyObjectsNormal.Count; l++)
		{
			if (hierarchyObjectsNormal[l].obejctCase != null)
			{
				Object.Destroy(hierarchyObjectsNormal[l].obejctCase);
			}
		}
		hierarchyObjectsNormal.Clear();
	}

	public void ChangeButtonOpen(GameObject _object)
	{
		for (int i = 0; i < buttonsDrop.Length; i++)
		{
			if (buttonsDrop[i] != null)
			{
				if (buttonsDrop[i] != _object)
				{
					buttonsDrop[i].GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
					buttonsDrop[i].transform.Find("Text").GetComponent<Text>().color = new Color(1f, 1f, 1f);
				}
				else
				{
					buttonsDrop[i].GetComponent<Image>().color = new Color(1f, 1f, 1f);
					buttonsDrop[i].transform.Find("Text").GetComponent<Text>().color = new Color(0f, 0f, 0f);
				}
			}
		}
	}
}
