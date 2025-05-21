using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialogue_3DText : MonoBehaviour
{
	public enum Dialogue3DTheme
	{
		Mita = 0,
		MitaOld = 1,
		MitaNew = 2,
		Player = 3,
		ChibiMita = 4,
		MitaKnow = 5,
		Creepy = 6,
		LittleMita = 7,
		White = 8,
		Limping = 9
	}

	public enum Alignment3DText
	{
		Left = 0,
		Middle = 1
	}

	public float sizeHeight = 0.2f;

	public float sizeWidth = 1.5f;

	public MeshRenderer boxVisible;

	public Font font;

	public int indexString;

	public float noiseStart;

	public float noise;

	public GameObject exampleSymbol;

	public Dialogue3DTheme themeDialogue;

	public Alignment3DText align;

	public float timeShow = 3f;

	private bool wasTimeRandomLook;

	private GameObject wasLookObject;

	[Header("Говорящий")]
	public GameObject speaker;

	public bool lookOnPlayer;

	[Space(20f)]
	[Header("Конец диалога")]
	public float timeFinish;

	public UnityEvent eventFinish;

	private bool eventFinishOk;

	public UnityEvent eventFinishPrint;

	public GameObject nextText;

	private float timeSound;

	public DataValues_Sounds sounds;

	[Header("Настройки")]
	public bool dontSubtitles;

	public bool showSubtitles;

	private GameController scrgc;

	private float timePrint;

	[HideInInspector]
	public bool stop;

	private float xPrint;

	private int indexChar;

	private string textPrint;

	private float sizeSymbol;

	[HideInInspector]
	public string textPrintNow;

	private CharacterInfo ch;

	public List<GameObject> symbolObjects = new List<GameObject>(0);

	private void Start()
	{
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		if (timeFinish == 0f)
		{
			timeFinish = 0.0001f;
		}
		exampleSymbol.SetActive(value: false);
		exampleSymbol.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 110f);
		exampleSymbol.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		sounds.StartComponent();
		textPrint = GlobalLanguage.GetString(sounds.nameFileLocation, indexString - 1);
		textPrint = textPrint.Replace("[player]", GlobalGame.namePlayer);
		if (themeDialogue == Dialogue3DTheme.Mita)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 0.47f, 0.71f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0.515f, 0f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = -0.23f;
		}
		if (themeDialogue == Dialogue3DTheme.Player)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 0.9f, 0.8f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(1f, 0.6f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = -0.23f;
		}
		if (themeDialogue == Dialogue3DTheme.ChibiMita)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 0.08f, 0.4f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0f, 0.5f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = -0.23f;
		}
		if (themeDialogue == Dialogue3DTheme.MitaNew)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 0.2f, 0.2f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0.2f, 0.6f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = 0f;
		}
		if (themeDialogue == Dialogue3DTheme.MitaKnow)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 0.6f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(1f, 0f, 0.75f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = 0.2f;
		}
		if (themeDialogue == Dialogue3DTheme.Creepy)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 0f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0f, 0f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = -0.2f;
			for (int i = 0; i < Random.Range(2, 5); i++)
			{
				int num = Random.Range(0, textPrint.Length - 1);
				if (Random.Range(0, 2) == 1)
				{
					textPrint = textPrint.Insert(num, (textPrint[num].ToString() ?? "").ToUpper());
				}
				else
				{
					textPrint = textPrint.Insert(num, textPrint[num].ToString() ?? "");
				}
			}
		}
		if (themeDialogue == Dialogue3DTheme.LittleMita)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 0f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0f, 0f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = 0f;
		}
		if (themeDialogue == Dialogue3DTheme.White)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(1f, 1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = 0f;
		}
		if (themeDialogue == Dialogue3DTheme.Limping)
		{
			exampleSymbol.GetComponent<Text>().color = new Color(1f, 1f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color1 = new Color(1f, 1f, 0f, 1f);
			exampleSymbol.GetComponent<UIGradient>().color2 = new Color(0f, 0.4f, 0.2f, 1f);
			exampleSymbol.GetComponent<UIGradient>().offset = -0.23f;
		}
		if (GlobalGame.fontUse != null)
		{
			font = GlobalGame.fontUse;
		}
		exampleSymbol.GetComponent<Text>().font = font;
		exampleSymbol.GetComponent<Dialogue_Symbol>().shadowText.GetComponent<Text>().font = font;
		font.RequestCharactersInTexture(textPrint, 72);
		if (!dontSubtitles)
		{
			scrgc.DialogueAdd(this, textPrint);
		}
		if (speaker != null)
		{
			wasTimeRandomLook = speaker.GetComponent<MitaPerson>().lookLife.offTimeLookRandom;
			speaker.GetComponent<MitaPerson>().lookLife.offTimeLookRandom = true;
			if (lookOnPlayer)
			{
				if (speaker.GetComponent<MitaPerson>().lookLife.lookObject != null)
				{
					wasLookObject = speaker.GetComponent<MitaPerson>().lookLife.lookObject.gameObject;
				}
				speaker.GetComponent<MitaPerson>().lookLife.LookOnPlayer();
			}
		}
		float num2 = 0f;
		for (int j = 0; j < textPrint.Length; j++)
		{
			font.GetCharacterInfo(textPrint[j], out ch);
			num2 += (float)ch.advance;
		}
		exampleSymbol.transform.localScale = Vector3.one * (sizeWidth / (0.5f * num2));
		if (exampleSymbol.transform.localScale.x > sizeHeight * 0.02f)
		{
			exampleSymbol.transform.localScale -= Vector3.one * (exampleSymbol.transform.localScale.x - sizeHeight * 0.02f);
		}
		sizeSymbol = exampleSymbol.transform.localScale.x;
		exampleSymbol.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
		exampleSymbol.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		exampleSymbol.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
		exampleSymbol.GetComponent<Dialogue_Symbol>().shadowText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
		font.GetCharacterInfo(textPrint[0], out ch, 72, FontStyle.Normal);
		if (align == Alignment3DText.Middle)
		{
			xPrint -= num2 * sizeSymbol / 2f;
		}
	}

	private void Update()
	{
		if (!stop)
		{
			timePrint -= Time.deltaTime;
			if (timePrint <= 0f)
			{
				if (textPrint[indexChar] != ' ' && !GlobalGame.hideDialogue)
				{
					GameObject gameObject = Object.Instantiate(exampleSymbol, exampleSymbol.transform.parent);
					gameObject.SetActive(value: true);
					gameObject.GetComponent<Text>().text = textPrint[indexChar].ToString() ?? "";
					gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPrint, 0f);
					if (themeDialogue != Dialogue3DTheme.Creepy)
					{
						gameObject.GetComponent<Dialogue_Symbol>().StartComponent(this, timeShow, noiseStart, noise, 20f);
					}
					else
					{
						gameObject.GetComponent<Dialogue_Symbol>().StartComponent(this, timeShow, noiseStart, noise, 240f);
					}
					symbolObjects.Add(gameObject);
				}
				timePrint = 0.05f;
				font.GetCharacterInfo(textPrint[indexChar], out ch, 72, FontStyle.Normal);
				xPrint += (float)ch.advance * sizeSymbol;
				textPrintNow += textPrint[indexChar];
				indexChar++;
				if (indexChar == textPrint.Length)
				{
					Object.Destroy(exampleSymbol);
					stop = true;
					eventFinishPrint.Invoke();
					if (symbolObjects[0].GetComponent<Dialogue_Symbol>().timeLife != -1000f && symbolObjects[0].GetComponent<Dialogue_Symbol>().timeLife <= 0f)
					{
						for (int i = 0; i < symbolObjects.Count; i++)
						{
							symbolObjects[i].GetComponent<Dialogue_Symbol>().timeLife = 0.3f + (float)i * 0.05f;
						}
					}
				}
			}
			if (!GlobalGame.hideDialogue)
			{
				if (timeSound > 0f)
				{
					timeSound -= Time.deltaTime;
				}
				if (sounds != null && textPrint[indexChar - 1] != '.' && textPrint[indexChar - 1] != ',' && textPrint[indexChar - 1] != '!' && textPrint[indexChar - 1] != '?' && textPrint[indexChar - 1] != ')' && textPrint[indexChar - 1] != '(' && timeSound <= 0f)
				{
					timeSound = 0.1f;
					sounds.audioSource.pitch = Random.Range(0.95f, 1.05f);
					if (sounds.soundLoop == null)
					{
						sounds.audioSource.clip = sounds.sounds[Random.Range(0, sounds.sounds.Length)];
						sounds.audioSource.Play();
					}
					else
					{
						sounds.audioSource.time = Random.Range(0.11f, sounds.soundLoop.length - 0.11f);
						sounds.audioSource.Play();
						sounds.audioSource.volume = sounds.volumeSoundLoop;
					}
				}
			}
			if (!dontSubtitles)
			{
				if (themeDialogue != Dialogue3DTheme.Player)
				{
					if (!showSubtitles)
					{
						scrgc.PrintDialogue(this, boxVisible.isVisible);
					}
					else
					{
						scrgc.PrintDialogue(this, _boxVisible: false);
					}
				}
				else
				{
					scrgc.PrintDialogue(this, _boxVisible: true);
				}
			}
		}
		if (!stop)
		{
			return;
		}
		if (timeFinish > 0f)
		{
			timeFinish -= Time.deltaTime;
			if (timeFinish <= 0f)
			{
				timeFinish = 0f;
				if (!eventFinishOk)
				{
					eventFinishOk = true;
					if (speaker != null)
					{
						speaker.GetComponent<MitaPerson>().lookLife.offTimeLookRandom = wasTimeRandomLook;
						if (lookOnPlayer && wasLookObject != null)
						{
							speaker.GetComponent<MitaPerson>().lookLife.LookOnObject(wasLookObject.transform);
						}
					}
					eventFinish.Invoke();
					if (nextText != null)
					{
						nextText.SetActive(base.gameObject);
					}
				}
			}
		}
		if (timeFinish == 0f && symbolObjects.Count == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void DestroySymbol(GameObject _object)
	{
		symbolObjects.Remove(_object);
		Object.Destroy(_object);
	}

	public void SymbolJump()
	{
		base.transform.SetParent(GlobalTag.world.transform);
	}

	public void AllJumps()
	{
		for (int i = 0; i < symbolObjects.Count; i++)
		{
			if (symbolObjects[i] != null)
			{
				symbolObjects[i].GetComponent<Dialogue_Symbol>().Jump();
			}
		}
		stop = true;
	}

	public void StopNextDialogue()
	{
		nextText = null;
	}

	public void StopFastNextDialogue()
	{
		nextText = null;
		if (!base.gameObject.activeSelf || stop)
		{
			return;
		}
		stop = true;
		for (int i = 0; i < symbolObjects.Count; i++)
		{
			if (symbolObjects[i] != null)
			{
				symbolObjects[i].GetComponent<Dialogue_Symbol>().Jump();
			}
		}
	}
}
