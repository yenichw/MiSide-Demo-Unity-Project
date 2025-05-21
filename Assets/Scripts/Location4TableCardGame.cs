using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location4TableCardGame : MonoBehaviour
{
	[Header("Установки")]
	public Location4TableCardGame_CardMemory[] cardsGeneral;

	public Location4TableCardGame_CardHold[] cardsPlayer;

	public Location4TableCardGame_CardHold[] cardsMita;

	private bool playerHoldCards;

	private bool mitaHoldCards;

	[Header("Механика")]
	public Animator animMita;

	public GameObject fishka;

	public Mesh[] meshFishka;

	public GameObject exampleCard;

	public BoxCollider boxZoom;

	private FullBodyBipedIK scrfbbik;

	private int indexCard;

	private int indexCardThrow;

	private bool showCards;

	private bool canShowCards;

	private Transform elbowRightPlayer;

	[Header("Игрок")]
	public Transform pivotCardsPlayer;

	public Transform rightHandPlayer;

	public ObjectAnimationPlayer animationThrowPlayer;

	public Transform positionThrowPlayer;

	public Transform boneRightItemPlayer;

	private Transform boneRightItemPlayerOrigin;

	private int indexCardMitaThrow;

	private int indexCardMitaTakeForThrow;

	[Header("Мита")]
	public Transform pivotCardsMita;

	public Transform positionThrowMita;

	public Transform mitaBoneLeftItem;

	public Transform mitaBoneLeftItemLate;

	public Transform mitaBoneRightItem;

	public Transform mitaBoneRightItemLate;

	private int _cnt;

	private int typeFishkaNow;

	private int stepGame;

	private int countSteps;

	private float timeStepGame;

	private float StopGameTime;

	private bool gameStop;

	private bool play;

	private int scorePlayer;

	private int scoreMita;

	[Header("Прерывание игры")]
	public UnityEvent eventStopGame;

	public UnityEvent eventCloseScarf;

	private bool cardMitaShow;

	[Header("Интерфейс")]
	public RectTransform rectCardMita;

	public RectTransform rectFishka;

	public RectTransform rectTableScore;

	public Text textScore;

	public Sprite iconHeart;

	public Sprite iconDamage;

	public Sprite iconShield;

	[Header("Кнопки")]
	public Interface_KeyHint_Key buttonShowCards;

	public Interface_KeyHint_Key buttonHideCards;

	public Interface_KeyHint_Key buttonThrowCard;

	public Interface_KeyHint_Key buttonRightCard;

	public Interface_KeyHint_Key buttonLeftCard;

	[Header("Диалоги")]
	public GameObject[] dialogues;

	[Header("Звук")]
	public AudioClip[] soundsCardDrop;

	public AudioClip[] soundsCardTake;

	public AudioClip[] soundsChangeCard;

	public AudioClip soundCardShow;

	public AudioClip soundCardHide;

	public AudioSource audioCardStackPlayer;

	public AudioSource audioCardStackMita;

	private void Awake()
	{
		rectCardMita.anchoredPosition = new Vector2(25f, 400f);
		rectFishka.anchoredPosition = new Vector2(-80f, 30f);
		rectTableScore.anchoredPosition = new Vector2(55f, -50f);
	}

	private void Start()
	{
		boneRightItemPlayerOrigin = GlobalTag.player.GetComponent<PlayerMove>().GetBoneLeftItem();
		indexCardMitaTakeForThrow = -1;
		indexCardMitaThrow = -1;
		indexCardThrow = -1;
		ReChangeFishka();
	}

	private void Update()
	{
		if (!gameStop)
		{
			if (playerHoldCards)
			{
				if (stepGame == 2 && showCards)
				{
					if (Input.GetButtonDown("Left"))
					{
						RightCard();
					}
					if (Input.GetButtonDown("Right"))
					{
						LeftCard();
					}
					if (Input.GetButtonDown("Interactive"))
					{
						PlayerChangeCard();
					}
				}
				if (indexCardThrow != -1)
				{
					cardsPlayer[indexCardThrow].objectCard.transform.localPosition = Vector3.Lerp(cardsPlayer[indexCardThrow].objectCard.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
					cardsPlayer[indexCardThrow].objectCard.transform.localRotation = Quaternion.Lerp(cardsPlayer[indexCardThrow].objectCard.transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 10f);
				}
				if (canShowCards)
				{
					_cnt = 0;
					for (int i = 0; i < cardsPlayer.Length; i++)
					{
						if (i == indexCardThrow)
						{
							continue;
						}
						if (cardsPlayer[i].objectCard != null)
						{
							_cnt++;
						}
						if (stepGame != 2 || !showCards)
						{
							if (cardsPlayer[i].objectCard != null)
							{
								cardsPlayer[i].objectCard.transform.localPosition = Vector3.Lerp(cardsPlayer[i].objectCard.transform.localPosition, new Vector3(-0.02f, -0.04f, 0f) + new Vector3(0.008f * (float)_cnt, 0.005f * (float)_cnt, -0.001f * (float)_cnt), Time.deltaTime * 10f);
							}
						}
						else if (i == indexCard)
						{
							if (cardsPlayer[i].objectCard != null)
							{
								cardsPlayer[i].objectCard.transform.localPosition = Vector3.Lerp(cardsPlayer[i].objectCard.transform.localPosition, new Vector3(-0.02f, -0.08f, 0f) + new Vector3(0.008f * (float)_cnt, 0.005f * (float)_cnt, -0.001f * (float)_cnt), Time.deltaTime * 10f);
							}
						}
						else if (cardsPlayer[i].objectCard != null)
						{
							cardsPlayer[i].objectCard.transform.localPosition = Vector3.Lerp(cardsPlayer[i].objectCard.transform.localPosition, new Vector3(-0.02f, -0.04f, 0f) + new Vector3(0.008f * (float)_cnt, 0.005f * (float)_cnt, -0.001f * (float)_cnt), Time.deltaTime * 10f);
						}
						if (cardsPlayer[i].objectCard != null)
						{
							cardsPlayer[i].objectCard.transform.localRotation = Quaternion.Lerp(cardsPlayer[i].objectCard.transform.localRotation, Quaternion.Euler(new Vector3(0f, 0f, -30f)) * Quaternion.Euler(new Vector3(0f, 0f, 10 * _cnt)), Time.deltaTime * 10f);
						}
					}
				}
				if (canShowCards && StopGameTime == 0f)
				{
					scrfbbik.solver.rightArmMapping.weight = Mathf.Lerp(scrfbbik.solver.rightArmMapping.weight, 1f, Time.deltaTime * 5f);
					if (!showCards)
					{
						rightHandPlayer.localPosition = Vector3.Lerp(rightHandPlayer.localPosition, new Vector3(-0.55f, 0.744f, -0.207f), Time.deltaTime * 5f);
						rightHandPlayer.localRotation = Quaternion.Lerp(rightHandPlayer.localRotation, Quaternion.Euler(new Vector3(12.548f, -218.708f, 54.18f)), Time.deltaTime * 5f);
						if (stepGame == 2 && Input.GetButtonDown("Up"))
						{
							ShowCard(x: true);
							buttonShowCards.Hide(x: true);
							buttonHideCards.Hide(x: false);
							buttonThrowCard.Hide(x: false);
							buttonRightCard.Hide(x: false);
							buttonLeftCard.Hide(x: false);
						}
					}
					else
					{
						rightHandPlayer.localPosition = Vector3.Lerp(rightHandPlayer.localPosition, new Vector3(-0.556f, 0.775f, -0.028f), Time.deltaTime * 5f);
						rightHandPlayer.localRotation = Quaternion.Lerp(rightHandPlayer.localRotation, Quaternion.Euler(new Vector3(21f, -262.342f, 61.007f)), Time.deltaTime * 5f);
						if (stepGame == 2 && Input.GetButtonDown("Down"))
						{
							ShowCard(x: false);
							buttonShowCards.Hide(x: false);
							buttonHideCards.Hide(x: true);
							buttonThrowCard.Hide(x: true);
							buttonRightCard.Hide(x: true);
							buttonLeftCard.Hide(x: true);
						}
					}
					elbowRightPlayer.transform.localPosition = Vector3.Lerp(elbowRightPlayer.transform.localPosition, new Vector3(0.588f, 0.5f, -0.062f), Time.deltaTime * 5f);
				}
				else
				{
					scrfbbik.solver.rightArmMapping.weight = Mathf.Lerp(scrfbbik.solver.rightArmMapping.weight, 0f, Time.deltaTime * 10f);
				}
			}
			if (mitaHoldCards)
			{
				_cnt = 0;
				for (int j = 0; j < cardsMita.Length; j++)
				{
					if (cardsMita[j].objectCard != null && j != indexCardMitaThrow && j != indexCardMitaTakeForThrow)
					{
						_cnt++;
						cardsMita[j].objectCard.transform.localPosition = Vector3.Lerp(cardsMita[j].objectCard.transform.localPosition, new Vector3(0.03f, -0.02f, -0.001f) + new Vector3(-0.01f * (float)_cnt, 0f, 0.001f * (float)_cnt), Time.deltaTime * 10f);
						cardsMita[j].objectCard.transform.localRotation = Quaternion.Lerp(cardsMita[j].objectCard.transform.localRotation, Quaternion.Euler(new Vector3(0f, 180f, -40f)) * Quaternion.Euler(new Vector3(0f, 0f, 8 * _cnt)), Time.deltaTime * 10f);
					}
				}
			}
			if (timeStepGame > 0f)
			{
				timeStepGame -= Time.deltaTime;
				if (timeStepGame <= 0f)
				{
					timeStepGame = 0f;
					if (stepGame == 0)
					{
						MitaStartThrowCard();
					}
					if (stepGame == 1)
					{
						if (countSteps != 4)
						{
							stepGame = 2;
							buttonShowCards.gameObject.SetActive(value: true);
							buttonHideCards.gameObject.SetActive(value: true);
							buttonThrowCard.gameObject.SetActive(value: true);
							buttonRightCard.gameObject.SetActive(value: true);
							buttonLeftCard.gameObject.SetActive(value: true);
							buttonShowCards.Hide(x: false);
						}
						else
						{
							StopGameTime = 1f;
							eventCloseScarf.Invoke();
						}
					}
					if (stepGame == 5)
					{
						stepGame = 0;
						timeStepGame = 0.5f;
					}
					if (stepGame == 4)
					{
						cardMitaShow = false;
						stepGame = 5;
						animMita.SetTrigger("RaChangeFishka");
						timeStepGame = 2f;
						LookResult();
					}
					if (stepGame == 3)
					{
						stepGame = 4;
						animMita.SetTrigger("SheckResult");
						timeStepGame = 3f;
						countSteps++;
					}
				}
			}
			if (indexCardMitaTakeForThrow != -1)
			{
				cardsMita[indexCardMitaTakeForThrow].objectCard.transform.localPosition = Vector3.Lerp(cardsMita[indexCardMitaTakeForThrow].objectCard.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
				cardsMita[indexCardMitaTakeForThrow].objectCard.transform.localRotation = Quaternion.Lerp(cardsMita[indexCardMitaTakeForThrow].objectCard.transform.localRotation, Quaternion.Euler(new Vector3(0f, 180f, 0f)), Time.deltaTime * 10f);
			}
			if (indexCardMitaThrow != -1)
			{
				cardsMita[indexCardMitaThrow].objectCard.transform.localPosition = Vector3.Lerp(cardsMita[indexCardMitaThrow].objectCard.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
				cardsMita[indexCardMitaThrow].objectCard.transform.localRotation = Quaternion.Lerp(cardsMita[indexCardMitaThrow].objectCard.transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 10f);
			}
			if (StopGameTime > 0f)
			{
				StopGameTime -= Time.deltaTime;
				if (StopGameTime <= 0f)
				{
					StopGame();
				}
			}
		}
		if (play)
		{
			if (cardMitaShow)
			{
				rectCardMita.anchoredPosition = Vector2.Lerp(rectCardMita.anchoredPosition, new Vector2(25f, -25f), Time.deltaTime * 5f);
				rectFishka.anchoredPosition = Vector2.Lerp(rectFishka.anchoredPosition, new Vector2(320f, -25f), Time.deltaTime * 5f);
			}
			else
			{
				rectCardMita.anchoredPosition = Vector2.Lerp(rectCardMita.anchoredPosition, new Vector2(25f, 400f), Time.deltaTime * 5f);
				rectFishka.anchoredPosition = Vector2.Lerp(rectFishka.anchoredPosition, new Vector2(75f, -25f), Time.deltaTime * 5f);
			}
			rectTableScore.anchoredPosition = Vector2.Lerp(rectTableScore.anchoredPosition, new Vector2(55f, 30f), Time.deltaTime * 5f);
		}
		else
		{
			rectCardMita.anchoredPosition = Vector2.Lerp(rectCardMita.anchoredPosition, new Vector2(25f, 400f), Time.deltaTime * 10f);
			rectFishka.anchoredPosition = Vector2.Lerp(rectFishka.anchoredPosition, new Vector2(-80f, -25f), Time.deltaTime * 10f);
			rectTableScore.anchoredPosition = Vector2.Lerp(rectTableScore.anchoredPosition, new Vector2(55f, -50f), Time.deltaTime * 10f);
		}
	}

	private void LateUpdate()
	{
		mitaBoneLeftItemLate.SetPositionAndRotation(mitaBoneLeftItem.position, mitaBoneLeftItem.rotation * Quaternion.Euler(new Vector3(0f, 180f, 0f)));
		mitaBoneRightItemLate.SetPositionAndRotation(mitaBoneRightItem.position, mitaBoneRightItem.rotation);
		boneRightItemPlayer.SetPositionAndRotation(boneRightItemPlayerOrigin.position, boneRightItemPlayerOrigin.rotation);
	}

	private void RightCard()
	{
		indexCard++;
		if (indexCard > cardsPlayer.Length - 1)
		{
			indexCard = 0;
		}
		while (cardsPlayer[indexCard].objectCard == null)
		{
			indexCard++;
			if (indexCard > cardsPlayer.Length - 1)
			{
				indexCard = 0;
			}
		}
		audioCardStackPlayer.clip = soundsChangeCard[Random.Range(0, soundsChangeCard.Length)];
		audioCardStackPlayer.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackPlayer.Play();
	}

	private void LeftCard()
	{
		indexCard--;
		if (indexCard < 0)
		{
			indexCard = cardsPlayer.Length - 1;
		}
		while (cardsPlayer[indexCard].objectCard == null)
		{
			indexCard--;
			if (indexCard < 0)
			{
				indexCard = cardsPlayer.Length - 1;
			}
		}
		audioCardStackPlayer.clip = soundsChangeCard[Random.Range(0, soundsChangeCard.Length)];
		audioCardStackPlayer.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackPlayer.Play();
	}

	public void StartGame()
	{
		timeStepGame = 0.25f;
		stepGame = 0;
		play = true;
	}

	private void StopGame()
	{
		StopGameTime = 0f;
		GlobalTag.player.GetComponent<PlayerMove>().scrppik.ActiveOtherControl(x: false);
		eventStopGame.Invoke();
		gameStop = true;
		play = false;
	}

	private void MitaStartThrowCard()
	{
		animMita.SetTrigger("ThrowCard");
	}

	private void PlayerChangeCard()
	{
		canShowCards = false;
		stepGame = 3;
		animationThrowPlayer.AnimationPlay();
		buttonThrowCard.Hide(x: true);
		buttonRightCard.Hide(x: true);
		buttonLeftCard.Hide(x: true);
		buttonShowCards.Hide(x: true);
		buttonHideCards.Hide(x: true);
	}

	private void ShowCard(bool x)
	{
		if (showCards != x)
		{
			showCards = x;
			if (showCards)
			{
				audioCardStackPlayer.clip = soundCardShow;
			}
			else
			{
				audioCardStackPlayer.clip = soundCardHide;
			}
			audioCardStackPlayer.pitch = Random.Range(0.95f, 1.05f);
			audioCardStackPlayer.Play();
		}
		boxZoom.enabled = !x;
	}

	private void LookResult()
	{
		int num = 0;
		if (typeFishkaNow == 0)
		{
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].damage > cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].damage)
			{
				num = 1;
			}
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].damage == cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].damage)
			{
				num = 2;
			}
		}
		if (typeFishkaNow == 1)
		{
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].shield > cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].shield)
			{
				num = 1;
			}
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].shield == cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].shield)
			{
				num = 2;
			}
		}
		if (typeFishkaNow == 2)
		{
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].heart > cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].heart)
			{
				num = 1;
			}
			if (cardsGeneral[cardsMita[indexCardMitaThrow].indexMemory].heart == cardsGeneral[cardsPlayer[indexCardThrow].indexMemory].heart)
			{
				num = 2;
			}
		}
		if (num == 0)
		{
			scorePlayer++;
			Object.Instantiate(dialogues[Random.Range(7, 11)], dialogues[7].transform.parent).SetActive(value: true);
		}
		if (num == 1)
		{
			scoreMita++;
			Object.Instantiate(dialogues[Random.Range(11, 15)], dialogues[11].transform.parent).SetActive(value: true);
		}
		if (num == 2)
		{
			Object.Instantiate(dialogues[Random.Range(15, 19)], dialogues[15].transform.parent).SetActive(value: true);
		}
		textScore.text = scorePlayer + "/" + scoreMita;
		cardsMita[indexCardMitaThrow].objectCard.GetComponent<Location4TableCardGame_Card>().DestroyAlpha();
		cardsMita[indexCardMitaThrow].objectCard = null;
		cardsPlayer[indexCardThrow].objectCard.GetComponent<Location4TableCardGame_Card>().DestroyAlpha();
		cardsPlayer[indexCardThrow].objectCard = null;
		indexCardMitaThrow = -1;
		indexCardMitaTakeForThrow = -1;
		indexCardThrow = -1;
		indexCard = 0;
		while (cardsPlayer[indexCard].objectCard == null)
		{
			indexCard++;
			if (indexCard > cardsPlayer.Length - 1)
			{
				indexCard = 0;
			}
		}
	}

	public void PlayerTakeCard()
	{
		rectCardMita.parent.gameObject.SetActive(value: true);
		pivotCardsPlayer.gameObject.SetActive(value: true);
		pivotCardsPlayer.parent = GlobalTag.player.GetComponent<PlayerMove>().GetBoneRightItem();
		pivotCardsPlayer.localPosition = Vector3.zero;
		pivotCardsPlayer.localRotation = Quaternion.Euler(Vector3.zero);
		playerHoldCards = true;
		for (int i = 0; i < cardsPlayer.Length; i++)
		{
			cardsPlayer[i].objectCard = Object.Instantiate(exampleCard, pivotCardsPlayer);
			cardsPlayer[i].objectCard.transform.localPosition = Vector3.zero;
			cardsPlayer[i].objectCard.transform.localRotation = Quaternion.Euler(Vector3.zero);
			cardsPlayer[i].objectCard.SetActive(value: true);
			cardsPlayer[i].indexMemory = Random.Range(0, cardsGeneral.Length);
			cardsPlayer[i].objectCard.GetComponent<MeshFilter>().mesh = cardsGeneral[cardsPlayer[i].indexMemory].mesh;
			cardsPlayer[i].objectCard.GetComponent<Location4TableCardGame_Card>().StartComponent(cardsGeneral[cardsPlayer[i].indexMemory].heart, cardsGeneral[cardsPlayer[i].indexMemory].damage, cardsGeneral[cardsPlayer[i].indexMemory].shield);
		}
		scrfbbik = GlobalTag.player.transform.Find("Person").GetComponent<FullBodyBipedIK>();
		GlobalTag.player.GetComponent<PlayerMove>().scrppik.ActiveOtherControl(x: true);
		scrfbbik.solver.rightHandEffector.target = rightHandPlayer;
		elbowRightPlayer = GlobalTag.player.transform.Find("Elbow Right");
		canShowCards = true;
		ShowCard(x: true);
	}

	public void MitaTakeCard()
	{
		mitaHoldCards = true;
		for (int i = 0; i < cardsMita.Length; i++)
		{
			cardsMita[i].objectCard = Object.Instantiate(exampleCard, pivotCardsMita);
			cardsMita[i].objectCard.transform.localPosition = Vector3.zero;
			cardsMita[i].objectCard.transform.localRotation = Quaternion.Euler(Vector3.zero);
			cardsMita[i].objectCard.SetActive(value: true);
			cardsMita[i].indexMemory = Random.Range(0, cardsGeneral.Length);
			cardsMita[i].objectCard.GetComponent<MeshFilter>().mesh = cardsGeneral[cardsMita[i].indexMemory].mesh;
			cardsMita[i].objectCard.GetComponent<Location4TableCardGame_Card>().StartComponent(cardsGeneral[cardsMita[i].indexMemory].heart, cardsGeneral[cardsMita[i].indexMemory].damage, cardsGeneral[cardsMita[i].indexMemory].shield);
		}
	}

	public void ReChangeFishka()
	{
		if (typeFishkaNow == 0)
		{
			if (Random.Range(0, 1) == 0)
			{
				typeFishkaNow = 1;
			}
			else
			{
				typeFishkaNow = 2;
			}
		}
		else if (typeFishkaNow == 1)
		{
			if (Random.Range(0, 1) == 0)
			{
				typeFishkaNow = 0;
			}
			else
			{
				typeFishkaNow = 2;
			}
		}
		else if (typeFishkaNow == 2)
		{
			if (Random.Range(0, 1) == 2)
			{
				typeFishkaNow = 0;
			}
			else
			{
				typeFishkaNow = 1;
			}
		}
		fishka.GetComponent<MeshFilter>().mesh = meshFishka[typeFishkaNow];
		if (typeFishkaNow == 0)
		{
			rectFishka.GetComponent<Image>().color = new Color(0.5f, 0.8f, 1f);
			rectFishka.transform.Find("Icon").GetComponent<Image>().sprite = iconDamage;
			if (play)
			{
				Object.Instantiate(dialogues[0], dialogues[0].transform.parent).SetActive(value: true);
			}
		}
		if (typeFishkaNow == 1)
		{
			rectFishka.GetComponent<Image>().color = new Color(1f, 0.8f, 0.5f);
			rectFishka.transform.Find("Icon").GetComponent<Image>().sprite = iconShield;
			if (play)
			{
				Object.Instantiate(dialogues[1], dialogues[1].transform.parent).SetActive(value: true);
			}
		}
		if (typeFishkaNow == 2)
		{
			rectFishka.GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f);
			rectFishka.transform.Find("Icon").GetComponent<Image>().sprite = iconHeart;
			if (play)
			{
				Object.Instantiate(dialogues[2], dialogues[2].transform.parent).SetActive(value: true);
			}
		}
	}

	public void MitaThrowCard()
	{
		cardMitaShow = true;
		rectCardMita.GetComponent<Image>().sprite = cardsGeneral[cardsMita[indexCardMitaTakeForThrow].indexMemory].spriteCard;
		rectCardMita.transform.Find("Heart Int").GetComponent<Text>().text = cardsGeneral[cardsMita[indexCardMitaTakeForThrow].indexMemory].heart.ToString() ?? "";
		rectCardMita.transform.Find("Damage Int").GetComponent<Text>().text = cardsGeneral[cardsMita[indexCardMitaTakeForThrow].indexMemory].damage.ToString() ?? "";
		rectCardMita.transform.Find("Shield Int").GetComponent<Text>().text = cardsGeneral[cardsMita[indexCardMitaTakeForThrow].indexMemory].shield.ToString() ?? "";
		indexCardMitaThrow = indexCardMitaTakeForThrow;
		cardsMita[indexCardMitaTakeForThrow].objectCard.transform.parent = positionThrowMita;
		indexCardMitaTakeForThrow = -1;
		stepGame = 1;
		timeStepGame = 0.25f;
		Object.Instantiate(dialogues[Random.Range(3, 7)], dialogues[3].transform.parent).SetActive(value: true);
		audioCardStackMita.clip = soundsCardDrop[Random.Range(0, soundsCardDrop.Length)];
		audioCardStackMita.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackMita.Play();
	}

	public void MitaTakeLeftHandCard()
	{
		ShowCard(x: false);
		for (int i = 0; i < cardsMita.Length; i++)
		{
			if (cardsMita[i].objectCard != null)
			{
				indexCardMitaTakeForThrow = i;
				cardsMita[indexCardMitaTakeForThrow].objectCard.transform.parent = mitaBoneLeftItemLate;
				break;
			}
		}
		audioCardStackMita.clip = soundsCardTake[Random.Range(0, soundsCardTake.Length)];
		audioCardStackMita.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackMita.Play();
	}

	public void PlayerTakeCardInLeftHand()
	{
		cardsPlayer[indexCard].objectCard.transform.parent = boneRightItemPlayer;
		cardsPlayer[indexCard].objectCard.transform.localPosition = Vector3.zero;
		cardsPlayer[indexCard].objectCard.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		audioCardStackPlayer.clip = soundsCardTake[Random.Range(0, soundsCardTake.Length)];
		audioCardStackPlayer.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackPlayer.Play();
	}

	public void PlayerThrowCard()
	{
		cardsPlayer[indexCard].objectCard.transform.parent = positionThrowPlayer;
		indexCardThrow = indexCard;
		timeStepGame = 0.5f;
		canShowCards = true;
		ShowCard(x: false);
		audioCardStackPlayer.clip = soundsCardDrop[Random.Range(0, soundsCardDrop.Length)];
		audioCardStackPlayer.pitch = Random.Range(0.95f, 1.05f);
		audioCardStackPlayer.Play();
	}
}
