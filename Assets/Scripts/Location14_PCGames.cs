using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location14_PCGames : MonoBehaviour
{
	private bool gamePlay;

	private Camera cameraPlayer;

	private bool canMouseMove;

	[Header("Интерфейс")]
	public RectTransform mouse;

	public LayerMask mouseLayer;

	public Transform cameraTargetLerp;

	private bool animationMousePlay;

	private int indexAnimationMouse;

	private float timeAnimaitonMouse;

	private float timeWaitAnimationMouse;

	private Location14_AnimationCursor[] animationMouseClass;

	private Vector3 mousePositionWas;

	[Header("Анимация курсора")]
	public AnimationCurve animationMouse;

	public Location14_AnimationCursor[] animationMouseStartPCPrint;

	public Location14_AnimationCursor[] animationMouseFiles;

	public Location14_AnimationCursor[] animationMouseTree;

	public Location14_AnimationCursor[] animationMouseOffPC;

	public int softGame;

	private int indexGame;

	private int indexKeyPrintNeed;

	private int countNeedText;

	[Header("Написание скрипта")]
	public GameObject windowPrint;

	public Text textPrint;

	public RectTransform iconKey;

	private float timeAnimationKeyScale;

	public AnimationCurve keyAnimationScale;

	public Image keyPrint;

	[TextArea(1, 10)]
	public string code;

	[HideInInspector]
	public int windowFileChange;

	[HideInInspector]
	public List<GameObject> filesRed;

	[HideInInspector]
	public List<GameObject> filesYellow;

	[HideInInspector]
	public List<GameObject> filesGreen;

	[HideInInspector]
	public List<GameObject> filesBlue;

	private GameObject fileHold;

	[Header("Перетаскивание файлов")]
	public GameObject windowFiles;

	public GameObject fileExample;

	public Transform windowRed;

	public Transform windowYellow;

	public Transform windowGreen;

	public Transform windowBlue;

	public GameObject[] windowsReport;

	[Header("Дерево")]
	public GameObject windowTree;

	public Slider treeSliderComponentX;

	public Slider treeSliderComponentY;

	public Slider treeSliderComponentZ;

	public GameObject[] circles;

	private bool otherDayActive;

	private bool screamerNeed;

	[Header("Другой день")]
	public UnityEvent screamerMita;

	public GameObject iconOtherGame;

	[Header("звуки")]
	public AudioSource audioInterface;

	public Audio_Data audioKeySpray;

	public AudioSource audioMouse;

	public AudioClip[] soundsMouseClick;

	public AudioClip[] soundsMouseClickDown;

	public AudioClip[] soundsMouseClickUp;

	public AudioClip soundFileTake;

	public AudioClip soundFileDrop;

	public AudioClip soundCircleApply;

	public AudioClip soundCircleCan;

	public AudioClip soundCircleNoCan;

	public AudioClip soundCircleNotApply;

	private void Start()
	{
		cameraPlayer = GlobalTag.cameraPlayer.GetComponent<Camera>();
		canMouseMove = true;
		textPrint.text = "";
	}

	private void Update()
	{
		if (!(Time.timeScale > 0f))
		{
			return;
		}
		if (canMouseMove)
		{
			if (gamePlay)
			{
				if (timeAnimaitonMouse < 1f)
				{
					timeAnimaitonMouse += Time.deltaTime * 2f;
					if (timeAnimaitonMouse > 1f)
					{
						timeAnimaitonMouse = 1f;
					}
				}
				if (Physics.Raycast(cameraPlayer.ScreenPointToRay(Input.mousePosition), out var hitInfo, 100f, mouseLayer))
				{
					mouse.position = Vector3.Lerp(mousePositionWas, hitInfo.point, animationMouse.Evaluate(timeAnimaitonMouse));
				}
				if (Input.GetMouseButtonDown(0))
				{
					AudioMouseSound(soundsMouseClickDown);
				}
				if (Input.GetMouseButtonUp(0))
				{
					AudioMouseSound(soundsMouseClickUp);
				}
			}
		}
		else
		{
			if (animationMousePlay)
			{
				timeAnimaitonMouse += Time.deltaTime * 4f;
				if (timeAnimaitonMouse >= 1f)
				{
					mouse.position = Vector3.Lerp(mousePositionWas, animationMouseClass[indexAnimationMouse].targetMouse.position + animationMouseClass[indexAnimationMouse].offset, 1f);
					timeAnimaitonMouse = 1f;
					if (timeWaitAnimationMouse == 0f)
					{
						timeWaitAnimationMouse = animationMouseClass[indexAnimationMouse].timeWait;
					}
					timeWaitAnimationMouse -= Time.deltaTime;
					if (timeWaitAnimationMouse <= 0f)
					{
						AudioMouseSound(soundsMouseClick);
						timeWaitAnimationMouse = 0f;
						animationMouseClass[indexAnimationMouse].eventClick.Invoke();
						indexAnimationMouse++;
						if (indexAnimationMouse == animationMouseClass.Length)
						{
							animationMousePlay = false;
							canMouseMove = true;
							mousePositionWas = mouse.position;
							timeAnimaitonMouse = 0f;
						}
						else
						{
							timeAnimaitonMouse = 0f;
							mousePositionWas = mouse.position;
						}
					}
				}
				else
				{
					mouse.position = Vector3.Lerp(mousePositionWas, animationMouseClass[indexAnimationMouse].targetMouse.position + animationMouseClass[indexAnimationMouse].offset, animationMouse.Evaluate(timeAnimaitonMouse));
				}
			}
			mouse.anchoredPosition = new Vector3(mouse.anchoredPosition.x, mouse.anchoredPosition.y, 0f);
		}
		if (animationMousePlay || indexGame != 1)
		{
			return;
		}
		if (timeAnimationKeyScale < 1f)
		{
			timeAnimationKeyScale += Time.deltaTime * 3f;
			if (timeAnimationKeyScale > 1f)
			{
				timeAnimationKeyScale = 1f;
			}
			keyPrint.GetComponent<RectTransform>().localScale = Vector3.one * keyAnimationScale.Evaluate(timeAnimationKeyScale);
		}
		if (indexKeyPrintNeed == 0 && Input.GetAxis("Vertical") < 0f)
		{
			ClickKey();
			audioKeySpray.RandomPlayPitch();
		}
		if (indexKeyPrintNeed == 1 && Input.GetAxis("Vertical") > 0f)
		{
			ClickKey();
			audioKeySpray.RandomPlayPitch();
		}
		if (indexKeyPrintNeed == 2 && Input.GetAxis("Horizontal") < 0f)
		{
			ClickKey();
			audioKeySpray.RandomPlayPitch();
		}
		if (indexKeyPrintNeed == 3 && Input.GetAxis("Horizontal") > 0f)
		{
			ClickKey();
			audioKeySpray.RandomPlayPitch();
		}
	}

	public void OnEnable()
	{
		indexGame = 0;
		mouse.anchoredPosition = Vector2.zero;
	}

	public void StartGame()
	{
		if (!screamerNeed)
		{
			GlobalTag.world.GetComponent<WorldPlayer>().CameraLerpOtherTarget(cameraTargetLerp);
			gamePlay = true;
			GlobalTag.world.GetComponent<WorldPlayer>().ShowMouse(x: false);
			Cursor.lockState = CursorLockMode.None;
			if (!otherDayActive)
			{
				PlayAnimationMouse(animationMouseStartPCPrint);
			}
			else
			{
				canMouseMove = true;
			}
		}
		else
		{
			screamerNeed = false;
			screamerMita.Invoke();
		}
	}

	public void StopGame()
	{
		GlobalTag.world.GetComponent<WorldPlayer>().ShowMouse(x: false);
		Cursor.lockState = CursorLockMode.Locked;
		gamePlay = false;
	}

	private void CloseAllWindows()
	{
		windowPrint.gameObject.SetActive(value: false);
		windowFiles.gameObject.SetActive(value: false);
		windowTree.gameObject.SetActive(value: false);
	}

	private void PlayAnimationMouse(Location14_AnimationCursor[] animation)
	{
		animationMouseClass = animation;
		indexAnimationMouse = 0;
		timeAnimaitonMouse = 0f;
		animationMousePlay = true;
		canMouseMove = false;
		mousePositionWas = mouse.position;
	}

	[ContextMenu("Активировать другой день")]
	public void OtherDayActive()
	{
		screamerNeed = true;
		otherDayActive = true;
		iconOtherGame.gameObject.SetActive(value: true);
	}

	private void AudioMouseSound(AudioClip[] _sounds)
	{
		audioMouse.clip = _sounds[Random.Range(0, _sounds.Length)];
		audioMouse.pitch = Random.Range(0.95f, 1.05f);
		audioMouse.Play();
	}

	public void SoundPlay(AudioClip _sound)
	{
		if (_sound == soundFileTake || _sound == soundFileDrop)
		{
			audioInterface.volume = 0.8f;
		}
		else
		{
			audioInterface.volume = 0.4f;
		}
		audioInterface.clip = _sound;
		audioInterface.pitch = Random.Range(0.95f, 1.05f);
		audioInterface.Play();
	}

	public void OpenPrintGame()
	{
		CloseAllWindows();
		windowPrint.SetActive(value: true);
		indexGame = 1;
		ClickKey();
		countNeedText = 15;
		if (softGame == 1)
		{
			countNeedText = 10;
		}
		if (softGame == 2)
		{
			countNeedText = 8;
		}
		if (softGame == 3)
		{
			countNeedText = 4;
		}
		if (softGame == 4)
		{
			countNeedText = 3;
		}
		if (softGame == 5)
		{
			countNeedText = 2;
		}
		if (softGame == 6)
		{
			countNeedText = 1;
		}
		if (countNeedText < 1)
		{
			countNeedText = 1;
		}
	}

	public void ClickKey()
	{
		keyPrint.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-550, 550), Random.Range(-300, 300));
		keyPrint.GetComponent<RectTransform>().localScale = Vector3.one * 1.2f;
		timeAnimationKeyScale = 0f;
		int num = indexKeyPrintNeed;
		while (indexKeyPrintNeed == num)
		{
			indexKeyPrintNeed = Random.Range(0, 4);
		}
		if (indexKeyPrintNeed == 0)
		{
			iconKey.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
		}
		if (indexKeyPrintNeed == 1)
		{
			iconKey.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
		}
		if (indexKeyPrintNeed == 2)
		{
			iconKey.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
		}
		if (indexKeyPrintNeed == 3)
		{
			iconKey.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
		}
		textPrint.text += code[textPrint.text.Length];
		textPrint.text += code[textPrint.text.Length];
		textPrint.text += code[textPrint.text.Length];
		countNeedText--;
		if (countNeedText == 0)
		{
			PlayAnimationMouse(animationMouseFiles);
		}
	}

	public void OpenFilesGame()
	{
		CloseAllWindows();
		windowFiles.SetActive(value: true);
		indexGame = 2;
		windowRed.parent.parent.gameObject.SetActive(value: true);
		windowYellow.parent.parent.gameObject.SetActive(value: true);
		windowGreen.parent.parent.gameObject.SetActive(value: true);
		windowBlue.parent.parent.gameObject.SetActive(value: true);
		if (filesRed != null && filesRed.Count > 0)
		{
			for (int i = 0; i < filesRed.Count; i++)
			{
				Object.Destroy(filesRed[i]);
			}
			filesRed.Clear();
		}
		if (filesYellow != null && filesYellow.Count > 0)
		{
			for (int j = 0; j < filesYellow.Count; j++)
			{
				Object.Destroy(filesYellow[j]);
			}
			filesYellow.Clear();
		}
		if (filesGreen != null && filesGreen.Count > 0)
		{
			for (int k = 0; k < filesGreen.Count; k++)
			{
				Object.Destroy(filesGreen[k]);
			}
			filesGreen.Clear();
		}
		if (filesBlue != null && filesBlue.Count > 0)
		{
			for (int l = 0; l < filesBlue.Count; l++)
			{
				Object.Destroy(filesBlue[l]);
			}
			filesBlue.Clear();
		}
		fileExample.SetActive(value: true);
		int num = 24;
		if (softGame == 1)
		{
			num = 12;
		}
		if (softGame == 2)
		{
			num = 9;
		}
		if (softGame == 3)
		{
			num = 6;
		}
		if (softGame == 4)
		{
			num = 3;
		}
		if (softGame == 5)
		{
			num = 2;
		}
		if (softGame == 6)
		{
			num = 1;
		}
		if (softGame == 7)
		{
			num = 1;
		}
		for (int m = 0; m < num; m++)
		{
			GameObject gameObject = Object.Instantiate(fileExample, fileExample.transform.parent);
			gameObject.GetComponent<Location14_PCFiles>().StartComponent(Random.Range(0, 4));
			int num2 = Random.Range(0, 4);
			if (num2 == 0)
			{
				filesRed.Add(gameObject);
			}
			if (num2 == 1)
			{
				filesYellow.Add(gameObject);
			}
			if (num2 == 2)
			{
				filesGreen.Add(gameObject);
			}
			if (num2 == 3)
			{
				filesBlue.Add(gameObject);
			}
		}
		fileExample.SetActive(value: false);
		SetPositionsFiles(_sharply: true);
	}

	public void SetPositionsFiles(bool _sharply)
	{
		int num = 75;
		int num2 = -75;
		if (filesRed != null && filesRed.Count > 0)
		{
			for (int i = 0; i < filesRed.Count; i++)
			{
				filesRed[i].GetComponent<Location14_PCFiles>().SetPosition(windowRed, new Vector2(num, num2));
				if (_sharply)
				{
					filesRed[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num2);
				}
				num += 120;
				if (num > 700)
				{
					num = 75;
					num2 -= 115;
				}
			}
		}
		num = 75;
		num2 = -75;
		if (filesYellow != null && filesYellow.Count > 0)
		{
			for (int j = 0; j < filesYellow.Count; j++)
			{
				filesYellow[j].GetComponent<Location14_PCFiles>().SetPosition(windowYellow, new Vector2(num, num2));
				if (_sharply)
				{
					filesYellow[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num2);
				}
				num += 120;
				if (num > 700)
				{
					num = 75;
					num2 -= 115;
				}
			}
		}
		num = 75;
		num2 = -75;
		if (filesGreen != null && filesGreen.Count > 0)
		{
			for (int k = 0; k < filesGreen.Count; k++)
			{
				filesGreen[k].GetComponent<Location14_PCFiles>().SetPosition(windowGreen, new Vector2(num, num2));
				if (_sharply)
				{
					filesGreen[k].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num2);
				}
				num += 120;
				if (num > 700)
				{
					num = 75;
					num2 -= 115;
				}
			}
		}
		num = 75;
		num2 = -75;
		if (filesBlue == null || filesBlue.Count <= 0)
		{
			return;
		}
		for (int l = 0; l < filesBlue.Count; l++)
		{
			filesBlue[l].GetComponent<Location14_PCFiles>().SetPosition(windowBlue, new Vector2(num, num2));
			if (_sharply)
			{
				filesBlue[l].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num2);
			}
			num += 120;
			if (num > 700)
			{
				num = 75;
				num2 -= 115;
			}
		}
	}

	public void FileHold(GameObject _file)
	{
		fileHold = _file;
		for (int i = 0; i < windowsReport.Length; i++)
		{
			windowsReport[i].SetActive(value: true);
		}
	}

	public void FileUnhold()
	{
		for (int i = 0; i < windowsReport.Length; i++)
		{
			windowsReport[i].SetActive(value: false);
		}
		if (windowFileChange != 0)
		{
			filesRed.Remove(fileHold);
			filesYellow.Remove(fileHold);
			filesGreen.Remove(fileHold);
			filesBlue.Remove(fileHold);
			if (windowFileChange == 1)
			{
				filesRed.Add(fileHold);
			}
			if (windowFileChange == 2)
			{
				filesYellow.Add(fileHold);
			}
			if (windowFileChange == 3)
			{
				filesGreen.Add(fileHold);
			}
			if (windowFileChange == 4)
			{
				filesBlue.Add(fileHold);
			}
			windowFileChange = 0;
			SetPositionsFiles(_sharply: false);
		}
		bool flag = true;
		if (filesRed != null && filesRed.Count > 0)
		{
			for (int j = 0; j < filesRed.Count; j++)
			{
				if (filesRed[j].GetComponent<Location14_PCFiles>().indexColor != 0)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag && filesYellow != null && filesYellow.Count > 0)
		{
			for (int k = 0; k < filesYellow.Count; k++)
			{
				if (filesYellow[k].GetComponent<Location14_PCFiles>().indexColor != 3)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag && filesGreen != null && filesGreen.Count > 0)
		{
			for (int l = 0; l < filesGreen.Count; l++)
			{
				if (filesGreen[l].GetComponent<Location14_PCFiles>().indexColor != 1)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag && filesBlue != null && filesBlue.Count > 0)
		{
			for (int m = 0; m < filesBlue.Count; m++)
			{
				if (filesBlue[m].GetComponent<Location14_PCFiles>().indexColor != 2)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			PlayAnimationMouse(animationMouseTree);
		}
	}

	public void OpenTreeGame()
	{
		CloseAllWindows();
		windowTree.SetActive(value: true);
		indexGame = 3;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].GetComponent<Location14_PCTreeSphere>().StartComponent();
		}
		if (softGame > 1)
		{
			circles[0].GetComponent<Location14_PCTreeSphere>().ready = true;
		}
		if (softGame > 2)
		{
			circles[1].GetComponent<Location14_PCTreeSphere>().ready = true;
		}
		if (softGame > 3)
		{
			circles[2].GetComponent<Location14_PCTreeSphere>().ready = true;
			circles[3].GetComponent<Location14_PCTreeSphere>().ready = true;
		}
		if (softGame > 4)
		{
			circles[4].GetComponent<Location14_PCTreeSphere>().ready = true;
		}
		treeSliderComponentX.value = Random.Range(0f, 1f);
		treeSliderComponentY.value = Random.Range(0f, 1f);
		treeSliderComponentZ.value = Random.Range(0f, 1f);
		SliderUpdate();
	}

	public void SliderUpdate()
	{
		bool flag = true;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].GetComponent<Location14_PCTreeSphere>().UpdateCircle();
			if (!circles[i].GetComponent<Location14_PCTreeSphere>().ready)
			{
				flag = false;
			}
		}
		if (flag)
		{
			PlayAnimationMouse(animationMouseOffPC);
		}
	}
}
