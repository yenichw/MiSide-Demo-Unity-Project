using UnityEngine;
using UnityEngine.UI;

public class Tamagotchi_Dialogue : MonoBehaviour
{
	[Header("Мита")]
	public GameObject mitaCamera;

	public Camera mitaCameraCam;

	public AnimationCurve animationCameraClipping;

	public AnimationCurve animationCameraClippingActive;

	public Animator_FunctionsOverride animMitaCamera;

	public MitaPerson mita;

	public Character_Look mitaIkLook;

	[Header("Interface")]
	public Text textDialogue;

	public Text textName;

	public AnimationCurve animationAlpha;

	public Image imageFrame;

	public AudioSource audioStartSound;

	[Space(15f)]
	public Image imgNext;

	public Text textNext;

	private bool play;

	private string stringDialogueNeed;

	private string stringDialogueNow;

	private float timeNextSymbol;

	private Tamagotchi_Dialogue_Mob dialogueRun;

	private int dialogueIndex;

	private float timeDialoueSound;

	private AudioSource audioDialogue;

	private AudioClip[] soundsDialogue;

	[Header("Audio")]
	public AudioSource audioNext;

	private float enableAnimationTime;

	private bool enableDialogue;

	private float timeAnimationMitaCamera;

	private AnimationClip animationIdleAfterMitaCamera;

	private void OnEnable()
	{
		imageFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		imageFrame.color = new Color(1f, 1f, 1f, 0f);
		textDialogue.color = new Color(1f, 1f, 1f, 0f);
		textName.color = new Color(1f, 1f, 1f, 0f);
		imgNext.color = new Color(imgNext.color.r, imgNext.color.g, imgNext.color.b, 0f);
		textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, 0f);
		enableAnimationTime = 1f;
	}

	private void OnDisable()
	{
		CloseDialogue();
		enableDialogue = false;
	}

	private void Start()
	{
		audioDialogue = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (play)
		{
			timeNextSymbol += Time.deltaTime * 50f;
			if (timeNextSymbol > 1f)
			{
				timeNextSymbol = 0f;
				stringDialogueNow += stringDialogueNeed[stringDialogueNow.Length];
				textDialogue.text = stringDialogueNow;
				if (stringDialogueNow.Length == stringDialogueNeed.Length)
				{
					play = false;
				}
			}
			timeDialoueSound += Time.deltaTime;
			if (timeDialoueSound > 0.1f)
			{
				timeDialoueSound = 0f;
				if (stringDialogueNow.Length > 0 && stringDialogueNow[stringDialogueNow.Length - 1] != '.' && stringDialogueNow[stringDialogueNow.Length - 1] != '!' && stringDialogueNow[stringDialogueNow.Length - 1] != '?')
				{
					audioDialogue.clip = soundsDialogue[Random.Range(0, soundsDialogue.Length)];
					audioDialogue.pitch = Random.Range(0.95f, 1.05f);
					audioDialogue.Play();
				}
			}
		}
		if (enableDialogue)
		{
			if (enableAnimationTime < 1f)
			{
				enableAnimationTime += Time.deltaTime * 5f;
				if (enableAnimationTime > 1f)
				{
					enableAnimationTime = 1f;
				}
				mitaCameraCam.farClipPlane = 0.07f + animationCameraClippingActive.Evaluate(enableAnimationTime) * 4.93f;
				imageFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, animationAlpha.Evaluate(enableAnimationTime) * 25f);
				imageFrame.color = new Color(1f, 1f, 1f, animationAlpha.Evaluate(enableAnimationTime) * 0.7f);
				textDialogue.color = new Color(1f, 1f, 1f, animationAlpha.Evaluate(enableAnimationTime));
				textName.color = new Color(1f, 1f, 1f, textDialogue.color.a);
			}
			if (play)
			{
				if (textNext.color.a > 0f)
				{
					textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, textNext.color.a - Time.deltaTime * 5f);
					if (textNext.color.a < 0f)
					{
						textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, 0f);
					}
					imgNext.color = new Color(imgNext.color.r, imgNext.color.g, imgNext.color.b, textNext.color.a);
				}
			}
			else if (textNext.color.a < 1f)
			{
				textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, textNext.color.a + Time.deltaTime * 5f);
				if (textNext.color.a > 1f)
				{
					textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, 1f);
				}
				imgNext.color = new Color(imgNext.color.r, imgNext.color.g, imgNext.color.b, textNext.color.a);
			}
			if (Input.GetButtonDown("Interactive") || Input.GetMouseButtonDown(0))
			{
				KeyNextDialogue();
			}
		}
		else
		{
			if (enableAnimationTime < 1f)
			{
				enableAnimationTime += Time.deltaTime * 5f;
				if (enableAnimationTime > 1f)
				{
					enableAnimationTime = 1f;
					base.gameObject.SetActive(value: false);
					mitaCamera.SetActive(value: false);
				}
				mitaCameraCam.farClipPlane = 5f - animationCameraClipping.Evaluate(enableAnimationTime) * 4.93f;
				imageFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 25f - animationAlpha.Evaluate(enableAnimationTime) * 25f);
				imageFrame.color = new Color(1f, 1f, 1f, 0.7f - animationAlpha.Evaluate(enableAnimationTime) * 0.7f);
				textDialogue.color = new Color(1f, 1f, 1f, 1f - animationAlpha.Evaluate(enableAnimationTime));
				textName.color = new Color(1f, 1f, 1f, textDialogue.color.a);
			}
			if (textNext.color.a > 0f)
			{
				textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, textNext.color.a - Time.deltaTime * 5f);
				if (textNext.color.a < 0f)
				{
					textNext.color = new Color(textNext.color.r, textNext.color.g, textNext.color.b, 0f);
				}
				imgNext.color = new Color(imgNext.color.r, imgNext.color.g, imgNext.color.b, textNext.color.a);
			}
		}
		if (timeAnimationMitaCamera > 0f)
		{
			timeAnimationMitaCamera -= Time.deltaTime;
			if ((double)timeAnimationMitaCamera < 0.1)
			{
				timeAnimationMitaCamera = 0f;
				animMitaCamera.AnimationClipSimpleNext(animationIdleAfterMitaCamera);
			}
		}
	}

	public void StartDialogue(Tamagotchi_Dialogue_Mob _dialogueRun)
	{
		dialogueIndex = -1;
		dialogueRun = _dialogueRun;
		if (dialogueRun.copyPerson == null)
		{
			textName.text = GlobalLanguage.GetString("Names", dialogueRun.nameString);
			soundsDialogue = dialogueRun.sounds;
		}
		else
		{
			textName.text = GlobalLanguage.GetString("Names", dialogueRun.copyPerson.nameString);
			soundsDialogue = dialogueRun.copyPerson.sounds;
		}
		if (!enableDialogue)
		{
			enableDialogue = true;
			base.gameObject.SetActive(value: true);
			enableAnimationTime = 0f;
			mitaCamera.SetActive(value: true);
			mitaCameraCam.farClipPlane = 0.07f;
			if (dialogueRun.dialogue[0].animationPlay != null)
			{
				animMitaCamera.AnimationClipSimple(dialogueRun.dialogue[0].animationPlay);
				timeAnimationMitaCamera = dialogueRun.dialogue[0].animationPlay.length;
				if (dialogueRun.dialogue[0].animationIdle != null)
				{
					animationIdleAfterMitaCamera = dialogueRun.dialogue[0].animationIdle;
				}
			}
			else
			{
				timeAnimationMitaCamera = 0f;
				if (dialogueRun.dialogue[0].animationIdle != null)
				{
					animMitaCamera.AnimationClipSimple(dialogueRun.dialogue[0].animationIdle);
				}
			}
		}
		NextDialogue();
	}

	public void CloseDialogue()
	{
		enableDialogue = false;
		enableAnimationTime = 0f;
	}

	public void FastCloseDialoue()
	{
		timeAnimationMitaCamera = 0f;
		enableDialogue = false;
		enableAnimationTime = 0f;
		base.gameObject.SetActive(value: false);
		mitaCamera.SetActive(value: false);
		mitaCameraCam.farClipPlane = 0.07f;
	}

	public void NextDialogue()
	{
		if (dialogueIndex == dialogueRun.dialogue.Length - 1)
		{
			if (!dialogueRun.demoStop || (dialogueRun.demoStop && !GlobalGame.demo))
			{
				dialogueRun.eventStop.Invoke();
			}
			else
			{
				dialogueRun.eventStopDEMO.Invoke();
			}
			CloseDialogue();
			return;
		}
		play = true;
		timeDialoueSound = 1f;
		dialogueIndex++;
		stringDialogueNow = "";
		textDialogue.text = "";
		if (dialogueRun.copyPerson == null)
		{
			stringDialogueNeed = GlobalLanguage.GetString(dialogueRun.dialogueFile, dialogueRun.dialogue[dialogueIndex].indexString - 1);
		}
		else
		{
			stringDialogueNeed = GlobalLanguage.GetString(dialogueRun.copyPerson.dialogueFile, dialogueRun.dialogue[dialogueIndex].indexString - 1);
		}
		stringDialogueNeed = stringDialogueNeed.Replace("[Player]", GlobalGame.namePlayer);
		dialogueRun.dialogue[dialogueIndex].eventStart.Invoke();
		if (dialogueRun.dialogue[dialogueIndex].startSound != null)
		{
			audioStartSound.clip = dialogueRun.dialogue[dialogueIndex].startSound;
			audioStartSound.Play();
		}
		if (dialogueRun.dialogue[dialogueIndex].emotion != null && dialogueRun.dialogue[dialogueIndex].emotion != "")
		{
			mita.FaceEmotion(dialogueRun.dialogue[dialogueIndex].emotion);
		}
		if (dialogueRun.dialogue[dialogueIndex].animationPlay != null)
		{
			animMitaCamera.AnimationClipSimpleNext(dialogueRun.dialogue[dialogueIndex].animationPlay);
			timeAnimationMitaCamera = dialogueRun.dialogue[dialogueIndex].animationPlay.length;
			if (dialogueRun.dialogue[dialogueIndex].animationIdle != null)
			{
				animationIdleAfterMitaCamera = dialogueRun.dialogue[dialogueIndex].animationIdle;
			}
		}
		else if (dialogueRun.dialogue[dialogueIndex].animationIdle != null)
		{
			animMitaCamera.AnimationClipSimpleNext(dialogueRun.dialogue[dialogueIndex].animationIdle);
		}
		mitaIkLook.IKBodyEnable(dialogueRun.dialogue[dialogueIndex].bodyIKActive);
	}

	public void KeyNextDialogue()
	{
		if (enableDialogue && !play)
		{
			NextDialogue();
			audioNext.pitch = Random.Range(0.95f, 1.05f);
			audioNext.Play();
		}
	}
}
