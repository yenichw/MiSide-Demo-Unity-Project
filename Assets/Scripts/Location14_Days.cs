using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location14_Days : MonoBehaviour
{
	[SerializeField]
	private int days;

	[SerializeField]
	private float speedDay;

	private bool black;

	[Header("Интерфейс")]
	public Text number1;

	public Text number2;

	public Text number3;

	public Image blackScreen;

	public Animator animFrameDay;

	private int indexEventDay;

	private float timeNextHint;

	private float timeBlackLerp;

	[Header("Дни")]
	public Animator animatorSun;

	public Color colorLightingMorning;

	public Color colorLightingDay;

	public Color colorLightingEvening;

	public UnityEvent eventNewDay;

	public UnityEvent eventStartLastDay;

	public UnityEvent eventStartSkipDay;

	public Location14_DaysPart[] parts;

	public GameObject hintSitBed;

	public GameObject hintStayBed;

	public Location14_PCGames pcGames;

	private float skipDaysTimeNextAnimation;

	private float skipDaysTimeNextAnimationMax;

	private bool playAnimationSkipDays;

	private float timeAnimationSkipDays;

	[Header("Анимация")]
	public AnimationCurve animationSkipDays;

	public GameObject objectHint;

	public GameObject[] objectsHint;

	public AudioSource audioSkipDays;

	[Header("Звуки")]
	public AudioSource[] audiosUsual;

	public GameObject[] audiosObjectUsual;

	public AudioClip soundTickFast;

	public AudioSource audioTick;

	private void Start()
	{
		black = true;
	}

	private void Update()
	{
		if (!playAnimationSkipDays)
		{
			if (timeBlackLerp == 0f)
			{
				if (!black)
				{
					if (blackScreen.color.a > 0f)
					{
						blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a - Time.deltaTime * speedDay);
						if (blackScreen.color.a < 0f)
						{
							blackScreen.color = new Color(0f, 0f, 0f, 0f);
						}
					}
				}
				else if (blackScreen.color.a < 1f)
				{
					blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a + Time.deltaTime * speedDay);
					if (blackScreen.color.a >= 1f)
					{
						blackScreen.color = new Color(0f, 0f, 0f, 1f);
						if (indexEventDay == 15)
						{
							NextDay();
							if (days == 6)
							{
								StartSkipDays();
							}
						}
					}
				}
			}
			else if (timeBlackLerp <= 0.3f && blackScreen.color.a < 1f)
			{
				blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a + Time.deltaTime * 5f);
				if (blackScreen.color.a > 1f)
				{
					timeBlackLerp = 0f;
					blackScreen.color = new Color(0f, 0f, 0f, 1f);
					black = false;
					parts[indexEventDay].eventEnd.Invoke();
					if (days > 3 && days < 10 && !playAnimationSkipDays)
					{
						parts[indexEventDay].eventAfter3Day.Invoke();
					}
				}
			}
			if (timeBlackLerp > 0f)
			{
				timeBlackLerp -= Time.deltaTime;
				if (timeBlackLerp <= 0f)
				{
					timeBlackLerp = -1f;
				}
			}
			if (indexEventDay > 10)
			{
				RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, colorLightingEvening, Time.deltaTime);
				animatorSun.SetFloat("n", Mathf.Lerp(animatorSun.GetFloat("n"), 1f, Time.deltaTime));
			}
			else if (indexEventDay == 10)
			{
				RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, colorLightingEvening, Time.deltaTime * 0.05f * (speedDay * 0.05f));
				animatorSun.SetFloat("n", Mathf.Lerp(animatorSun.GetFloat("n"), 0.5f, Time.deltaTime * 0.05f * (speedDay * 0.05f)));
			}
		}
		else
		{
			if ((double)blackScreen.color.a < 0.6)
			{
				blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a + Time.deltaTime);
				if ((double)blackScreen.color.a > 0.6)
				{
					blackScreen.color = new Color(0f, 0f, 0f, 0.6f);
				}
			}
			if ((double)blackScreen.color.a > 0.6)
			{
				blackScreen.color = new Color(0f, 0f, 0f, blackScreen.color.a - Time.deltaTime);
				if ((double)blackScreen.color.a < 0.6)
				{
					blackScreen.color = new Color(0f, 0f, 0f, 0.6f);
				}
			}
		}
		if (playAnimationSkipDays)
		{
			timeAnimationSkipDays += Time.deltaTime * 0.05f;
			if (timeAnimationSkipDays > 1f)
			{
				audioSkipDays.gameObject.SetActive(value: false);
				blackScreen.color = new Color(0f, 0f, 0f, 1f);
				black = true;
				timeAnimationSkipDays = 1f;
				playAnimationSkipDays = false;
				speedDay = 0.5f;
				days = 998;
				GlobalTag.world.GetComponent<WorldPlayer>().CameraGausianBlur(_x: false);
				NextDay();
				eventStartLastDay.Invoke();
				objectHint.SetActive(value: true);
				for (int i = 0; i < objectsHint.Length; i++)
				{
					if (objectsHint[i].GetComponent<Interface_KeyHint_Key>() != null)
					{
						objectsHint[i].GetComponent<Interface_KeyHint_Key>().Hide(x: true);
					}
					if (objectsHint[i].GetComponent<UI_LookOnCamera>() != null)
					{
						objectsHint[i].GetComponent<UI_LookOnCamera>().Hide(_hide: true);
					}
				}
			}
			else
			{
				audioSkipDays.pitch = 0.5f + (float)days / 998f * 1.5f;
				if (days != Mathf.CeilToInt(Mathf.Lerp(5f, 998f, animationSkipDays.Evaluate(timeAnimationSkipDays))))
				{
					audioTick.Play();
					days = Mathf.CeilToInt(Mathf.Lerp(5f, 998f, animationSkipDays.Evaluate(timeAnimationSkipDays)));
					string text = "";
					if (days < 10)
					{
						text += "0";
					}
					if (days < 100)
					{
						text += "0";
					}
					text += days;
					number1.text = text[2].ToString() ?? "";
					number2.text = text[1].ToString() ?? "";
					number3.text = text[0].ToString() ?? "";
				}
			}
			if ((double)skipDaysTimeNextAnimationMax > 0.05)
			{
				skipDaysTimeNextAnimationMax -= Time.deltaTime * 0.025f;
				if (skipDaysTimeNextAnimationMax < 0.05f)
				{
					skipDaysTimeNextAnimationMax = 0.05f;
				}
			}
			skipDaysTimeNextAnimation -= Time.deltaTime;
			if (skipDaysTimeNextAnimation <= 0f)
			{
				indexEventDay++;
				if (indexEventDay > parts.Length - 1)
				{
					indexEventDay = 0;
				}
				skipDaysTimeNextAnimation = skipDaysTimeNextAnimationMax;
				RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, colorLightingEvening, Time.deltaTime * ((float)indexEventDay / 5f));
				animatorSun.SetFloat("n", Mathf.Lerp(animatorSun.GetFloat("n"), 1f, Time.deltaTime * ((float)indexEventDay / 5f)));
				if (parts[indexEventDay].animationPlayerObject == null)
				{
					while (parts[indexEventDay].animationPlayerObject == null)
					{
						indexEventDay++;
						if (indexEventDay > parts.Length - 1)
						{
							indexEventDay = 0;
							RenderSettings.ambientSkyColor = colorLightingMorning;
							animatorSun.SetFloat("n", 0f);
						}
						if (parts[indexEventDay].animationPlayerObject != null)
						{
							parts[indexEventDay].animationPlayerObject.AnimationPlayFast();
						}
					}
				}
				else
				{
					parts[indexEventDay].animationPlayerObject.AnimationPlayFast();
				}
			}
		}
		if (indexEventDay == 1 && timeNextHint > 0f)
		{
			timeNextHint -= Time.deltaTime;
			if (timeNextHint <= 0f)
			{
				hintSitBed.SetActive(value: true);
				hintSitBed.GetComponent<Interface_KeyHint_Key>().Hide(x: false);
			}
		}
	}

	public void StartGame()
	{
		speedDay = -2f;
		NextDay();
	}

	[ContextMenu("Следующий день")]
	private void NextDay()
	{
		eventNewDay.Invoke();
		indexEventDay = 0;
		days++;
		pcGames.softGame = days;
		if (days < 10)
		{
			number2.text = days.ToString() ?? "";
			animFrameDay.Play("NextDay", -1, 0f);
		}
		else
		{
			string text = days.ToString() ?? "";
			if (days < 100)
			{
				text = "0" + days;
			}
			number1.text = text[2].ToString() ?? "";
			number2.text = text[1].ToString() ?? "";
			number3.text = text[0].ToString() ?? "";
			animFrameDay.Play("InterfaceDay NextLastDay", -1, 0f);
		}
		speedDay += 3f;
		RenderSettings.ambientSkyColor = colorLightingMorning;
		animatorSun.SetFloat("n", 0f);
	}

	[ContextMenu("Запустить ускорение")]
	private void StartSkipDays()
	{
		GlobalTag.world.GetComponent<WorldPlayer>().CameraGausianBlur(_x: true);
		objectHint.SetActive(value: false);
		for (int i = 0; i < objectsHint.Length; i++)
		{
			objectsHint[i].SetActive(value: false);
		}
		playAnimationSkipDays = true;
		animFrameDay.Play("StartSkipDay", -1, 0f);
		skipDaysTimeNextAnimationMax = 0.5f;
		skipDaysTimeNextAnimation = 0.5f;
		audioTick.clip = soundTickFast;
		audioTick.volume = 0.25f;
		audioSkipDays.gameObject.SetActive(value: true);
		eventStartSkipDay.Invoke();
		number1.text = "6";
		number2.text = "0";
		number3.text = "0";
	}

	private void TimeBlackLerp(AnimationClip animationClip)
	{
		timeBlackLerp = animationClip.length - 0.3f - (speedDay - 1f) / 2f;
		if ((double)timeBlackLerp <= 0.3)
		{
			timeBlackLerp = -1f;
		}
	}

	public void AudioActivation(bool x)
	{
		for (int i = 0; i < audiosUsual.Length; i++)
		{
			audiosUsual[i].enabled = x;
			audiosUsual[i].Stop();
		}
		for (int j = 0; j < audiosObjectUsual.Length; j++)
		{
			audiosObjectUsual[j].SetActive(x);
		}
	}

	public void GoStopSleep()
	{
		black = false;
		indexEventDay = 1;
		timeNextHint = 2f / speedDay;
	}

	public void GoSitBed()
	{
		indexEventDay = 2;
		if (days > 3 && days < 10 && !playAnimationSkipDays)
		{
			TimeBlackLerp(parts[2].animationPlayer3Day);
		}
	}

	public void GoStayBed()
	{
		indexEventDay = 3;
	}

	public void GoWashUp()
	{
		indexEventDay = 4;
		TimeBlackLerp(parts[4].animationPlayer);
	}

	public void GoDressMe()
	{
		indexEventDay = 5;
		TimeBlackLerp(parts[5].animationPlayer);
	}

	public void GoCook()
	{
		indexEventDay = 6;
		TimeBlackLerp(parts[6].animationPlayer);
	}

	public void GoSitPC()
	{
		indexEventDay = 7;
		if (days > 3 && days < 10 && !playAnimationSkipDays)
		{
			TimeBlackLerp(parts[7].animationPlayer3Day);
		}
	}

	public void GoOnPC()
	{
		indexEventDay = 8;
	}

	public void GoEat()
	{
		indexEventDay = 9;
		TimeBlackLerp(parts[9].animationPlayer);
	}

	public void GoStopEatStartWork()
	{
		indexEventDay = 10;
	}

	public void GoFinishWork()
	{
		indexEventDay = 11;
	}

	public void GoUndressMe()
	{
		indexEventDay = 12;
		TimeBlackLerp(parts[12].animationPlayer);
	}

	public void GoWashDown()
	{
		indexEventDay = 13;
		TimeBlackLerp(parts[13].animationPlayer);
	}

	public void GoLieDown()
	{
		indexEventDay = 14;
	}

	public void GoSleep()
	{
		indexEventDay = 15;
		black = true;
	}
}
