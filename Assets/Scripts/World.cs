using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
	[Header("Для перевода")]
	public string nameLocation;

	[HideInInspector]
	public List<LanguageFilesTexture> languageTexture;

	[HideInInspector]
	public bool texturesLoad;

	private PlayerMove scrpm;

	[Header("Игрок")]
	public Vector3 positionSpawn;

	public Vector2 rotationSpawn;

	[Header("Подсказки")]
	public bool showHint;

	public int indexHint = 1;

	public Vector2[] positionsHintScreen;

	private int indexPosHintScreen;

	[Header("Удаление лишнего")]
	public GameObject[] destroyObjects;

	public Collider[] destroyColliders;

	public GameObject[] deactiveObjects;

	[Header("Старт мира")]
	public UnityEvent eventStart;

	public UnityEvent eventContinueScene;

	public UnityEvent eventFirstStart;

	public Playable_Animation cutsceneStart;

	[Space(40f)]
	[Header("Информация")]
	public bool isContinue;

	private void Awake()
	{
		if (GlobalTag.world == null)
		{
			GlobalTag.world = base.gameObject;
			eventFirstStart.Invoke();
		}
		else
		{
			eventContinueScene.Invoke();
		}
		if (!GlobalGame.optionGameLoad)
		{
			GlobalGame.Language = PlayerPrefs.GetString("Language", "English");
		}
		if (Directory.Exists("Data/Languages/" + GlobalGame.Language + "/Textures") && Directory.Exists("Data/Languages/" + GlobalGame.Language + "/Textures/" + nameLocation))
		{
			string text = "Data/Languages/" + GlobalGame.Language + "/Textures/" + nameLocation;
			string[] files = Directory.GetFiles(text, "*.png");
			List<LanguageFilesTexture> list = new List<LanguageFilesTexture>();
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = files[i].Remove(files[i].Length - 4, 4);
				files[i] = files[i].Remove(0, text.Length + 1);
				list.Add(new LanguageFilesTexture());
				list[i].name = files[i];
				Texture2D texture2D = new Texture2D(20, 12);
				texture2D.LoadImage(File.ReadAllBytes(text + "/" + files[i] + ".png"));
				texture2D.mipMapBias = 0f;
				texture2D.requestedMipmapLevel = 0;
				texture2D.filterMode = FilterMode.Bilinear;
				list[i].texture = texture2D;
			}
			languageTexture = list;
			texturesLoad = true;
		}
		if (destroyObjects != null && destroyObjects.Length != 0)
		{
			for (int j = 0; j < destroyObjects.Length; j++)
			{
				if (destroyObjects[j] != null)
				{
					Object.Destroy(destroyObjects[j]);
				}
			}
		}
		if (destroyColliders != null && destroyColliders.Length != 0)
		{
			for (int k = 0; k < destroyColliders.Length; k++)
			{
				Object.Destroy(destroyColliders[k]);
			}
		}
	}

	private void Start()
	{
		GlobalTag.world = base.gameObject;
		for (int i = 0; i < deactiveObjects.Length; i++)
		{
			if (deactiveObjects[i] != null)
			{
				deactiveObjects[i].SetActive(value: false);
			}
		}
		GlobalTag.gameController.GetComponent<GameController>().StartComponent();
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		if (!GlobalGame.playWorld)
		{
			scrpm.StartComponent();
			scrpm.TeleportPlayer(positionSpawn, rotationSpawn.x, rotationSpawn.y);
			GlobalGame.playWorld = true;
		}
		else
		{
			isContinue = true;
		}
		eventStart.Invoke();
		if (cutsceneStart != null && !GlobalGame.demo)
		{
			cutsceneStart.Play();
		}
	}

	public Texture2D GetTexture2DLanguage(string _name)
	{
		Texture2D result = null;
		for (int i = 0; i < languageTexture.Count; i++)
		{
			if (languageTexture[i].name == _name)
			{
				result = languageTexture[i].texture;
				break;
			}
		}
		return result;
	}

	public void HintLocationChange(int _index)
	{
		indexHint = _index;
		showHint = true;
		if (GlobalGame.hintScreen)
		{
			if (positionsHintScreen != null && positionsHintScreen.Length != 0)
			{
				GlobalTag.gameController.GetComponent<GameController>().ShowHint(GlobalLanguage.GetString("LocationHint " + nameLocation, indexHint - 1), positionsHintScreen[indexPosHintScreen]);
			}
			else
			{
				GlobalTag.gameController.GetComponent<GameController>().ShowHint(GlobalLanguage.GetString("LocationHint " + nameLocation, indexHint - 1), new Vector2(20f, -20f));
			}
		}
	}

	public void HintScreenSetIndexPosition(int x)
	{
		indexPosHintScreen = x;
	}

	public void HintLocationHide()
	{
		showHint = false;
	}
}
