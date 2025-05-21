using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location15_ScreenID : MonoBehaviour
{
	public Audio_Data audioInput;

	private string IDplayer;

	[Header("Тексты")]
	public Text[] textScreens;

	public Text textMainScreen;

	private string textEnter;

	private int indexChange;

	private bool play;

	[Header("Управление")]
	public GameObject[] casesNum;

	public RectTransform caseChange;

	public UnityEvent eventReady;

	private void Start()
	{
		textEnter = "";
		for (int i = 0; i < textScreens.Length; i++)
		{
			textScreens[i].text = "ID [" + GlobalLanguage.GetString("Names", Random.Range(9, 47)) + "]:" + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10);
		}
		IDplayer = Random.Range(0, 10).ToString() + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10);
		int num = Random.Range(0, textScreens.Length);
		textScreens[num].text = "ID [" + GlobalGame.namePlayer + "]:" + IDplayer;
		for (int j = 1; j < casesNum.Length - 1; j++)
		{
			casesNum[j] = Object.Instantiate(casesNum[0], casesNum[0].transform.parent);
			casesNum[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(-200 + j * 45, -150f);
			casesNum[j].transform.Find("Text").GetComponent<Text>().text = j.ToString() ?? "";
		}
		casesNum[casesNum.Length - 1] = Object.Instantiate(casesNum[0], casesNum[0].transform.parent);
		casesNum[casesNum.Length - 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, -150f);
		casesNum[casesNum.Length - 1].transform.Find("Text").GetComponent<Text>().text = "R";
		ChangeUpdate();
	}

	private void Update()
	{
		if (!play)
		{
			return;
		}
		if (Input.GetButtonDown("Right"))
		{
			indexChange++;
			ChangeUpdate();
			audioInput.RandomPlayPitch();
		}
		if (Input.GetButtonDown("Left"))
		{
			indexChange--;
			ChangeUpdate();
			audioInput.RandomPlayPitch();
		}
		if (!Input.GetButtonDown("Interactive"))
		{
			return;
		}
		if (indexChange >= 0 && indexChange < 10)
		{
			if (textEnter.Length < 4)
			{
				audioInput.RandomPlayPitch();
				textEnter += indexChange;
				if (textEnter == IDplayer)
				{
					eventReady.Invoke();
				}
			}
		}
		else
		{
			audioInput.RandomPlayPitch();
			textEnter = "";
		}
		ChangeUpdate();
	}

	public void PlayPlayer()
	{
		play = true;
	}

	public void StopPlayer()
	{
		play = false;
	}

	private void ChangeUpdate()
	{
		if (indexChange < 0)
		{
			indexChange = casesNum.Length - 1;
		}
		if (indexChange > casesNum.Length - 1)
		{
			indexChange = 0;
		}
		caseChange.anchoredPosition = casesNum[indexChange].GetComponent<RectTransform>().anchoredPosition;
		textMainScreen.text = "ID: " + textEnter;
	}
}
