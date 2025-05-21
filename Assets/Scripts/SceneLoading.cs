using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoading : MonoBehaviour
{
	public AsyncOperation asyncLoad;

	public RectTransform lineLoad;

	public SceneLoading_Preloading preloading;

	[Header("первый запуск")]
	public GameObject objectInterfaceLoading;

	public GameObject objectChangeLanguage;

	public Text textExample;

	public GameObject[] casesLanguage;

	private bool go;

	private bool sceneAcyncGo;

	private Animator animButton;

	private string levelLoadName;

	private bool loadReady;

	private bool firstStartGame;

	private void Start()
	{
		Time.timeScale = 1f;
		animButton = GetComponent<Animator>();
		lineLoad.sizeDelta = new Vector2(0f, lineLoad.sizeDelta.y);
		if (GlobalGame.LoadingLevel == null)
		{
			levelLoadName = "SceneMenu";
		}
		else
		{
			levelLoadName = GlobalGame.LoadingLevel;
		}
		if (levelLoadName != "SceneMenu")
		{
			GlobalGame.play = true;
		}
		if (PlayerPrefs.GetInt("FirstStart", 0) == 0)
		{
			FirstStartGame();
			firstStartGame = true;
		}
		else
		{
			StartCoroutine(TimeStart());
		}
	}

	private void Update()
	{
		if (go && !sceneAcyncGo)
		{
			asyncLoad.allowSceneActivation = true;
			sceneAcyncGo = true;
			preloading.LoadingReady(0.5f);
		}
		if (asyncLoad != null)
		{
			lineLoad.sizeDelta = new Vector2((asyncLoad.progress + 0.1f) * 500f, lineLoad.sizeDelta.y);
			if (asyncLoad.progress >= 0.9f && !loadReady)
			{
				loadReady = true;
				go = true;
			}
		}
		if (firstStartGame && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Interactive")))
		{
			objectInterfaceLoading.SetActive(value: true);
			objectChangeLanguage.SetActive(value: false);
			PlayerPrefs.SetInt("FirstStart", 1);
			firstStartGame = false;
			StartCoroutine(TimeStart());
		}
	}

	private void LoadGo()
	{
		asyncLoad = SceneManager.LoadSceneAsync(levelLoadName);
		asyncLoad.allowSceneActivation = false;
	}

	private IEnumerator TimeStart()
	{
		yield return new WaitForSeconds(0.5f);
		LoadGo();
	}

	public void StopLoad()
	{
		if (asyncLoad != null)
		{
			asyncLoad = null;
		}
	}

	public void FirstStartGame()
	{
		objectInterfaceLoading.SetActive(value: false);
		objectChangeLanguage.SetActive(value: true);
		GameObject gameObject = objectChangeLanguage.transform.Find("Frame Flag/CaseFlag").gameObject;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string[] directories = Directory.GetDirectories("Data/Languages");
		for (int i = 0; i < directories.Length; i++)
		{
			directories[i] = directories[i].Replace("Data/Languages" + Path.DirectorySeparatorChar, "");
		}
		for (int j = 0; j < directories.Length; j++)
		{
			casesLanguage[j] = Object.Instantiate(gameObject, objectChangeLanguage.transform.Find("Frame Flag"));
			casesLanguage[j].SetActive(value: true);
			Texture2D texture2D = new Texture2D(20, 12);
			texture2D.LoadImage(File.ReadAllBytes("Data/Languages/" + directories[j] + "/F.png"));
			texture2D.mipMapBias = 0f;
			texture2D.requestedMipmapLevel = 0;
			texture2D.filterMode = FilterMode.Point;
			casesLanguage[j].GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			casesLanguage[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num3);
			casesLanguage[j].GetComponent<MenuCaseFlag>().index = j;
			num += 100;
			if (num == 500)
			{
				num = 0;
				num3 -= 54;
			}
			if (num2 < 500)
			{
				num2 += 100;
			}
		}
		objectChangeLanguage.transform.Find("Frame Flag").GetComponent<RectTransform>().sizeDelta = new Vector2(num2, 0f);
		Object.Destroy(gameObject);
		for (int k = 0; k < directories.Length; k++)
		{
			if (directories[k] == "English")
			{
				casesLanguage[k].GetComponent<MenuCaseFlag>().Click();
				break;
			}
		}
	}

	public void ClickFlag(int x)
	{
		string[] directories = Directory.GetDirectories("Data/Languages");
		for (int i = 0; i < directories.Length; i++)
		{
			directories[i] = directories[i].Replace("Data/Languages" + Path.DirectorySeparatorChar, "");
		}
		textExample.text = File.ReadAllLines("Data/Languages/" + directories[x] + "/Hello.txt")[0];
		GlobalGame.Language = directories[x];
		PlayerPrefs.SetString("Language", GlobalGame.Language);
		GlobalTag.gameOptions.GetComponent<OptionsGame>().ReloadLanguage();
	}
}
