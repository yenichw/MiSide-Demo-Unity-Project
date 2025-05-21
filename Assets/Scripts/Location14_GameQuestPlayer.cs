using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location14_GameQuestPlayer : MonoBehaviour
{
	private int indexAnimation;

	private Vector3 cameraSavePositionOrigin;

	private Quaternion cameraSaveRotationOrigin;

	[Header("Стартовая анимация")]
	public UnityEvent eventFinishStart;

	public Transform cameraPosition;

	public Transform cameraAniamtion;

	public Location14_MonitorScreenLoad loadScreenshots;

	private int indexInside;

	private bool nextDayPlay;

	[Header("Игра")]
	public Location14_MonitorScreenLoad screenMonitor;

	public Location14_QuestInteractive interactiveChair;

	public GameObject[] interactives;

	public UnityEvent eventNewDay;

	public UnityEvent event3Inside;

	public UnityEvent eventBackDayLite;

	public UnityEvent eventBackDayLast;

	public UnityEvent eventLastClickPlayer;

	[Header("Интерфейс")]
	public Image blackScreen;

	public Transform cameraScreenShot;

	private float timeDontClickSkip;

	private Location14_Dialogue componentDialogue;

	private int indexDialogue;

	private int indexDialogueSpeaker;

	private bool dialoguePrint;

	private float timeDialoguePrint;

	private string dialogueTextNeed;

	private bool waitQuest;

	[Header("Диалог")]
	public Image frameDialogue;

	public Text textDialogue;

	public Location14_ButtonChangeDialogue changeDialogueLeft;

	public Location14_ButtonChangeDialogue changeDialogueRight;

	private Camera cameraMain;

	[HideInInspector]
	public bool canObjectMouse;

	[HideInInspector]
	public GameObject objectMouse;

	[Header("Управление игроком")]
	public LayerMask layerMousePlane;

	public Location14_PlayerQuest player;

	[HideInInspector]
	public AudioSource au;

	[Header("Звуки")]
	public AudioSource audioSpeaker;

	public AudioClip soundSpeakMita;

	public AudioClip soundSpeakPlayer;

	public AudioClip soundSpace;

	public AudioClip soundChange;

	public AudioClip soundEnterChange;

	private void Start()
	{
		cameraMain = GlobalTag.cameraPlayer.GetComponent<Camera>();
		canObjectMouse = true;
		cameraSavePositionOrigin = cameraAniamtion.transform.position;
		cameraSaveRotationOrigin = cameraAniamtion.transform.rotation;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (indexAnimation == 0 && blackScreen.color.a < 1f)
		{
			blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a + Time.deltaTime);
			cameraPosition.transform.position += cameraPosition.forward * (Time.deltaTime * 0.025f);
			if (blackScreen.color.a >= 1f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, 1f);
				cameraPosition.transform.parent = cameraAniamtion;
				cameraPosition.transform.SetPositionAndRotation(cameraAniamtion.position, cameraAniamtion.rotation);
				cameraScreenShot.gameObject.SetActive(value: false);
				eventFinishStart.Invoke();
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				loadScreenshots.enabled = true;
			}
		}
		if (indexAnimation == 1)
		{
			if (!nextDayPlay && blackScreen.color.a > 0f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a - Time.deltaTime);
				if (blackScreen.color.a < 0f)
				{
					blackScreen.color = new Color(0f, 0f, 0f, 0f);
				}
			}
			if (canObjectMouse && Input.GetMouseButtonDown(0))
			{
				ClickMouse();
			}
		}
		if (indexAnimation == 2 && blackScreen.color.a < 1f)
		{
			blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a + Time.deltaTime * 4f);
			if (blackScreen.color.a >= 1f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, 1f);
				screenMonitor.LoadScreenPlayerSpeakMita();
				StopQuestGame();
				indexAnimation = 3;
			}
		}
		if (indexAnimation == 4)
		{
			if (blackScreen.color.a > 0f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a - Time.deltaTime);
				if (blackScreen.color.a < 0f)
				{
					blackScreen.color = new Color(0f, 0f, 0f, 0f);
				}
			}
			if (timeDontClickSkip > 0f)
			{
				timeDontClickSkip -= Time.deltaTime;
				if (timeDontClickSkip < 0f)
				{
					timeDontClickSkip = 0f;
				}
			}
			if (timeDontClickSkip == 0f && Input.GetMouseButtonDown(0))
			{
				timeDontClickSkip = -1f;
				eventLastClickPlayer.Invoke();
				indexAnimation = 5;
			}
		}
		if (indexAnimation == 5 && blackScreen.color.a > 0f)
		{
			blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a - Time.deltaTime);
			if (blackScreen.color.a < 0f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, 0f);
			}
		}
		if (componentDialogue != null)
		{
			if (timeDontClickSkip > 0f)
			{
				timeDontClickSkip -= Time.deltaTime;
				if (timeDontClickSkip < 0f)
				{
					timeDontClickSkip = 0f;
				}
			}
			if (!waitQuest && timeDontClickSkip == 0f && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Space")))
			{
				au.clip = soundSpace;
				au.pitch = Random.Range(0.95f, 1.05f);
				au.Play();
				DialogueNext();
			}
			if (frameDialogue.color.a < 1f)
			{
				frameDialogue.color = new Color(1f, 1f, 1f, frameDialogue.color.a + Time.deltaTime * 3f);
				if (frameDialogue.color.a > 1f)
				{
					frameDialogue.color = new Color(1f, 1f, 1f, 1f);
				}
				textDialogue.color = new Color(1f, 1f, 1f, frameDialogue.color.a);
			}
			if (!dialoguePrint)
			{
				return;
			}
			timeDialoguePrint += Time.deltaTime;
			if (!((double)timeDialoguePrint > 0.05))
			{
				return;
			}
			if (textDialogue.text != dialogueTextNeed)
			{
				if (textDialogue.text.Length != dialogueTextNeed.Length)
				{
					textDialogue.text += dialogueTextNeed[textDialogue.text.Length];
				}
				if (!audioSpeaker.isPlaying)
				{
					audioSpeaker.pitch = Random.Range(0.95f, 1.05f);
					audioSpeaker.Play();
				}
			}
			else
			{
				dialoguePrint = false;
				textDialogue.text = dialogueTextNeed;
				PrintFinish();
				componentDialogue.dialogue[indexDialogueSpeaker].text[indexDialogue].eventFinishText.Invoke();
			}
		}
		else if (frameDialogue.color.a > 0f)
		{
			frameDialogue.color = new Color(1f, 1f, 1f, frameDialogue.color.a - Time.deltaTime * 3f);
			if (frameDialogue.color.a < 0f)
			{
				frameDialogue.color = new Color(1f, 1f, 1f, 0f);
				frameDialogue.gameObject.SetActive(value: false);
			}
			textDialogue.color = new Color(1f, 1f, 1f, frameDialogue.color.a);
		}
	}

	private void FixedUpdate()
	{
		if (canObjectMouse)
		{
			if (Physics.Raycast(cameraMain.ScreenPointToRay(Input.mousePosition), out var hitInfo, 100f, layerMousePlane))
			{
				objectMouse = hitInfo.collider.gameObject;
			}
			else
			{
				objectMouse = null;
			}
		}
		else
		{
			objectMouse = null;
		}
	}

	public void ClickMouse()
	{
		if (Physics.Raycast(cameraMain.ScreenPointToRay(Input.mousePosition), out var hitInfo, 100f, layerMousePlane))
		{
			objectMouse = hitInfo.collider.gameObject;
			if (objectMouse.GetComponent<Location14_QuestInteractive>() != null)
			{
				canObjectMouse = false;
				objectMouse.GetComponent<Location14_QuestInteractive>().Deactivation();
				player.Move(objectMouse.GetComponent<Location14_QuestInteractive>().positionMove, objectMouse.GetComponent<Location14_QuestInteractive>().eventFinishMove);
			}
			else
			{
				player.MoveFloor(hitInfo.point);
			}
		}
		else
		{
			objectMouse = null;
		}
	}

	public void DialoguePlay(Location14_Dialogue _dialogue)
	{
		timeDontClickSkip = 0.1f;
		frameDialogue.gameObject.SetActive(value: true);
		canObjectMouse = false;
		dialoguePrint = true;
		timeDialoguePrint = 0f;
		textDialogue.text = "";
		indexDialogue = 0;
		indexDialogueSpeaker = 0;
		componentDialogue = _dialogue;
		dialogueTextNeed = GlobalLanguage.GetString("LocationDialogue Location14", componentDialogue.dialogue[indexDialogueSpeaker].text[indexDialogue].indexFile - 1);
		if (componentDialogue.dialogue[indexDialogueSpeaker].speaker == Location14_Dialogue.Loc14WhioSpeak.mita)
		{
			audioSpeaker.clip = soundSpeakMita;
		}
		if (componentDialogue.dialogue[indexDialogueSpeaker].speaker == Location14_Dialogue.Loc14WhioSpeak.player)
		{
			audioSpeaker.clip = soundSpeakMita;
		}
	}

	private void DialogueNext()
	{
		timeDontClickSkip = 0.1f;
		if (dialoguePrint)
		{
			dialoguePrint = false;
			textDialogue.text = dialogueTextNeed;
			PrintFinish();
			componentDialogue.dialogue[indexDialogueSpeaker].text[indexDialogue].eventFinishText.Invoke();
			return;
		}
		if (indexDialogue != componentDialogue.dialogue[indexDialogueSpeaker].text.Length - 1)
		{
			dialoguePrint = true;
			timeDialoguePrint = 0f;
			textDialogue.text = "";
			indexDialogue++;
			dialogueTextNeed = GlobalLanguage.GetString("LocationDialogue Location14", componentDialogue.dialogue[indexDialogueSpeaker].text[indexDialogue].indexFile - 1);
			return;
		}
		indexDialogue = 0;
		if (indexDialogueSpeaker != componentDialogue.dialogue.Length - 1)
		{
			dialoguePrint = true;
			timeDialoguePrint = 0f;
			textDialogue.text = "";
			indexDialogueSpeaker++;
			dialogueTextNeed = GlobalLanguage.GetString("LocationDialogue Location14", componentDialogue.dialogue[indexDialogueSpeaker].text[indexDialogue].indexFile - 1);
			if (componentDialogue.dialogue[indexDialogueSpeaker].speaker == Location14_Dialogue.Loc14WhioSpeak.mita)
			{
				audioSpeaker.clip = soundSpeakMita;
			}
			if (componentDialogue.dialogue[indexDialogueSpeaker].speaker == Location14_Dialogue.Loc14WhioSpeak.player)
			{
				audioSpeaker.clip = soundSpeakMita;
			}
		}
		else
		{
			DialogueStop();
		}
	}

	private void DialogueStop()
	{
		componentDialogue.eventFinish.Invoke();
		dialoguePrint = false;
		canObjectMouse = true;
		componentDialogue = null;
	}

	public void ChangeDialogue(bool _side)
	{
		Location14_Dialogue location14_Dialogue = componentDialogue;
		changeDialogueLeft.Deactivation();
		changeDialogueRight.Deactivation();
		DialogueStop();
		waitQuest = false;
		au.clip = soundChange;
		au.pitch = Random.Range(0.95f, 1.05f);
		au.Play();
		if (_side)
		{
			location14_Dialogue.eventRight.Invoke();
		}
		else
		{
			location14_Dialogue.eventLeft.Invoke();
		}
	}

	public void PrintFinish()
	{
		if (indexDialogueSpeaker == componentDialogue.dialogue.Length - 1 && indexDialogue == componentDialogue.dialogue[indexDialogueSpeaker].text.Length - 1 && componentDialogue.indexFileRight > 0)
		{
			changeDialogueLeft.Activation(GlobalLanguage.GetString("LocationDialogue Location14", componentDialogue.indexFileLeft - 1));
			changeDialogueRight.Activation(GlobalLanguage.GetString("LocationDialogue Location14", componentDialogue.indexFileRight - 1));
			waitQuest = true;
		}
	}

	public void EnterDialogueChange()
	{
		au.clip = soundEnterChange;
		au.pitch = Random.Range(0.95f, 1.05f);
		au.Play();
	}

	public void NewDay()
	{
		canObjectMouse = true;
		blackScreen.color = new Color(0f, 0f, 0f, 1f);
		nextDayPlay = false;
		eventNewDay.Invoke();
		GetComponent<Animator>().enabled = false;
		cameraAniamtion.transform.SetPositionAndRotation(cameraScreenShot.position, cameraScreenShot.rotation);
		player.TeleportStart();
		indexInside++;
		if (indexAnimation == 0)
		{
			indexAnimation = 1;
		}
		if (indexInside < 4)
		{
			interactiveChair.Activation();
			for (int i = 0; i < interactives.Length; i++)
			{
				interactives[i].GetComponent<Location14_QuestInteractive>().Activation();
			}
		}
		else
		{
			for (int j = 0; j < interactives.Length; j++)
			{
				interactives[j].GetComponent<Location14_QuestInteractive>().DestroyMe();
			}
			event3Inside.Invoke();
		}
		if (indexInside < 3)
		{
			screenMonitor.LoadScreenPlayerOne();
			return;
		}
		if (indexInside == 3)
		{
			screenMonitor.LoadScreenPlayerMita();
		}
		if (indexInside == 4)
		{
			screenMonitor.HideScreen();
		}
	}

	public void StartNextDay()
	{
		nextDayPlay = true;
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().Play("StartNextDay", -1, 0f);
	}

	public void StopQuestGame()
	{
		if (indexAnimation == 1)
		{
			dialoguePrint = false;
			canObjectMouse = false;
			componentDialogue = null;
			indexAnimation = 2;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			return;
		}
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().Play("StartBackDay", -1, 0f);
		indexInside--;
		if (indexInside == 3)
		{
			eventBackDayLite.Invoke();
			screenMonitor.LoadScreenPlayerSit();
		}
		if (indexInside == 0)
		{
			indexAnimation = 4;
			GetComponent<Animator>().enabled = false;
			cameraMain.transform.SetPositionAndRotation(cameraSavePositionOrigin, cameraSaveRotationOrigin);
			eventBackDayLast.Invoke();
			timeDontClickSkip = 0.5f;
			screenMonitor.HideScreen();
		}
	}
}
