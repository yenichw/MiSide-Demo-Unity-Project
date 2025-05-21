using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GamesCore_Main : MonoBehaviour
{
	private bool play;

	private int typeGame;

	private bool gameWasUse;

	private Animator anim;

	private int indexCaseChange;

	private float timeColorLerp;

	[Header("Выбор Миты")]
	public RectTransform cmChanger;

	public Image cmChangerUp;

	public Image cmChangerDown;

	public GamesCore_CaseSymbol[] casesSymbol;

	public UnityEvent eventChangeMita;

	private float timeCreateSpheres;

	private int indexCreateSphere;

	private int indexChangeSphere;

	[Header("Выбор Мира")]
	public GamesCore_Main_PositionSphere[] positionsSpheres;

	public GameObject sphereExample;

	public UnityEvent eventChangeVersion;

	public RectTransform sphereChange;

	public Text textVersion;

	[Header("Звуки")]
	public AudioSource audioSource;

	public AudioClip soundMoveHorizontal;

	public AudioClip soundMoveVertical;

	public AudioClip soundChangeVersionMita;

	public AudioClip soundChangeSphere;

	private void Start()
	{
		anim = GetComponent<Animator>();
		indexCreateSphere = 1;
		sphereExample.SetActive(value: false);
	}

	private void Update()
	{
		if (play)
		{
			if (typeGame == 0)
			{
				if (Input.GetButtonDown("Right"))
				{
					indexCaseChange++;
					if (indexCaseChange > 4)
					{
						indexCaseChange = 0;
					}
					AudioPlay(soundMoveHorizontal);
					UpdateChangeCaseSymbol();
				}
				if (Input.GetButtonDown("Left"))
				{
					indexCaseChange--;
					if (indexCaseChange < 0)
					{
						indexCaseChange = 4;
					}
					AudioPlay(soundMoveHorizontal);
					UpdateChangeCaseSymbol();
				}
				if (casesSymbol[indexCaseChange].dataSymbols.Length != 0)
				{
					if (Input.GetButtonDown("Up"))
					{
						cmChangerUp.color = new Color(1f, 0f, 0.1f, 1f);
						casesSymbol[indexCaseChange].SymbolUp();
						AudioPlay(soundMoveVertical);
						timeColorLerp = 0.1f;
					}
					if (Input.GetButtonDown("Down"))
					{
						cmChangerDown.color = new Color(1f, 0f, 0.1f, 1f);
						casesSymbol[indexCaseChange].SymbolDown();
						AudioPlay(soundMoveVertical);
						timeColorLerp = 0.1f;
					}
				}
				else if (casesSymbol[0].indexSymbol == 0 && casesSymbol[1].indexSymbol == 9 && casesSymbol[2].indexSymbol == 8 && casesSymbol[3].indexSymbol == 0 && Input.GetButtonDown("Interactive"))
				{
					SwitchChangeWorld();
				}
			}
			else
			{
				if (sphereChange.localScale.x < 1f)
				{
					sphereChange.localScale += Vector3.one * Time.deltaTime;
					if (sphereChange.localScale.x > 1f)
					{
						sphereChange.localScale = Vector3.one;
					}
				}
				sphereChange.GetComponent<UIShiny>().rotation += Time.deltaTime * 180f;
				if (sphereChange.GetComponent<UIShiny>().rotation >= 180f)
				{
					sphereChange.GetComponent<UIShiny>().rotation = -180f;
				}
				sphereChange.anchoredPosition = Vector2.Lerp(sphereChange.anchoredPosition, positionsSpheres[indexChangeSphere].position, Time.deltaTime * 10f);
				if (indexChangeSphere == 0)
				{
					sphereChange.sizeDelta = Vector2.Lerp(sphereChange.sizeDelta, Vector2.one * 35f, Time.deltaTime * 5f);
				}
				else
				{
					sphereChange.sizeDelta = Vector2.Lerp(sphereChange.sizeDelta, Vector2.one * 25f, Time.deltaTime * 5f);
				}
				if (indexCreateSphere < positionsSpheres.Length)
				{
					timeCreateSpheres += Time.deltaTime;
					if ((double)timeCreateSpheres > 0.1)
					{
						timeCreateSpheres = 0f;
						positionsSpheres[indexCreateSphere].sphere = Object.Instantiate(sphereExample, sphereExample.transform.parent);
						positionsSpheres[indexCreateSphere].sphere.GetComponent<RectTransform>().anchoredPosition = positionsSpheres[indexCreateSphere].position;
						positionsSpheres[indexCreateSphere].sphere.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, positionsSpheres[indexCreateSphere].rotation);
						positionsSpheres[indexCreateSphere].sphere.SetActive(value: true);
						if (positionsSpheres[indexCreateSphere].isFinish)
						{
							positionsSpheres[indexCreateSphere].sphere.GetComponent<GamesCore_Sphere>().StartFinish();
						}
						indexCreateSphere++;
						if (indexCreateSphere == positionsSpheres.Length)
						{
							Object.Destroy(sphereExample);
						}
					}
				}
				else
				{
					if (Input.GetButtonDown("Right") && positionsSpheres[indexChangeSphere].indexJoinRight != -1)
					{
						indexChangeSphere = positionsSpheres[indexChangeSphere].indexJoinRight;
						AudioPlay(soundChangeSphere);
						UpdateSpheres();
					}
					if (Input.GetButtonDown("Left") && positionsSpheres[indexChangeSphere].indexJoinLeft != -1)
					{
						indexChangeSphere = positionsSpheres[indexChangeSphere].indexJoinLeft;
						AudioPlay(soundChangeSphere);
						UpdateSpheres();
					}
					if (Input.GetButtonDown("Up") && positionsSpheres[indexChangeSphere].indexJoinUp != -1)
					{
						indexChangeSphere = positionsSpheres[indexChangeSphere].indexJoinUp;
						AudioPlay(soundChangeSphere);
						UpdateSpheres();
					}
					if (Input.GetButtonDown("Down") && positionsSpheres[indexChangeSphere].indexJoinDown != -1)
					{
						indexChangeSphere = positionsSpheres[indexChangeSphere].indexJoinDown;
						AudioPlay(soundChangeSphere);
						UpdateSpheres();
					}
				}
			}
		}
		else if (typeGame == 1)
		{
			sphereChange.sizeDelta = Vector2.Lerp(sphereChange.sizeDelta, Vector2.one * 25f, Time.deltaTime * 5f);
			sphereChange.anchoredPosition = Vector2.Lerp(sphereChange.anchoredPosition, positionsSpheres[indexChangeSphere].position, Time.deltaTime * 10f);
			sphereChange.GetComponent<UIShiny>().rotation += Time.deltaTime * 180f;
			if (sphereChange.GetComponent<UIShiny>().rotation >= 180f)
			{
				sphereChange.GetComponent<UIShiny>().rotation = -180f;
			}
		}
		if (!gameWasUse)
		{
			return;
		}
		if (timeColorLerp == 0f)
		{
			cmChanger.GetComponent<Image>().color = Color.Lerp(cmChanger.GetComponent<Image>().color, new Color(0.7f, 1f, 0f, 1f), Time.deltaTime * 8f);
			if (indexCaseChange < 4)
			{
				cmChangerUp.color = Color.Lerp(cmChangerUp.color, new Color(0.7f, 1f, 0f, 1f), Time.deltaTime * 8f);
				cmChangerDown.color = Color.Lerp(cmChangerDown.color, new Color(0.7f, 1f, 0f, 1f), Time.deltaTime * 8f);
			}
			else
			{
				cmChangerUp.color = Color.Lerp(cmChangerUp.color, new Color(0.7f, 1f, 0f, 0f), Time.deltaTime * 15f);
				cmChangerDown.color = Color.Lerp(cmChangerDown.color, new Color(0.7f, 1f, 0f, 0f), Time.deltaTime * 15f);
			}
		}
		else
		{
			timeColorLerp -= Time.deltaTime;
			if (timeColorLerp < 0f)
			{
				timeColorLerp = 0f;
			}
		}
	}

	public void AudioPlay(AudioClip _sound)
	{
		audioSource.clip = _sound;
		audioSource.pitch = Random.Range(0.95f, 1.05f);
		audioSource.Play();
	}

	private void UpdateChangeCaseSymbol()
	{
		cmChanger.anchoredPosition = new Vector2(casesSymbol[indexCaseChange].GetComponent<RectTransform>().anchoredPosition.x, -1.5f);
		cmChanger.GetComponent<Image>().color = new Color(1f, 0f, 0.1f, 1f);
		timeColorLerp = 0.1f;
	}

	private void UpdateSpheres()
	{
		for (int i = 1; i < positionsSpheres.Length; i++)
		{
			positionsSpheres[i].sphere.GetComponent<GamesCore_Sphere>().ResetColor();
		}
		if (indexChangeSphere != 0)
		{
			positionsSpheres[indexChangeSphere].sphere.GetComponent<GamesCore_Sphere>().Click();
		}
		if (positionsSpheres[indexChangeSphere].isFinish)
		{
			eventChangeVersion.Invoke();
		}
		textVersion.text = "V " + positionsSpheres[indexChangeSphere].version;
	}

	public void StartPlay()
	{
		gameWasUse = true;
		play = true;
	}

	public void StopPlay()
	{
		play = false;
	}

	public void SwitchChangeWorld()
	{
		AudioPlay(soundChangeVersionMita);
		eventChangeMita.Invoke();
		anim.SetBool("ChangeWorld", value: true);
		typeGame = 1;
		textVersion.text = "V 0.0";
	}
}
