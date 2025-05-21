using UnityEngine;
using UnityEngine.UI;

public class SmartphoneTime : MonoBehaviour
{
	public int timeHour;

	public int timeMinute;

	public Text textTime;

	private float timemil;

	private void Update()
	{
		timemil += Time.deltaTime;
		if (timemil >= 60f)
		{
			timemil -= 60f;
			timeMinute++;
			if (timeMinute == 60)
			{
				timeMinute = 0;
				timeHour++;
			}
			UpdateTime();
		}
	}

	public void SetHour(int _hour)
	{
		timeHour = _hour;
		UpdateTime();
	}

	public void SetMinute(int _minute)
	{
		timeMinute = _minute;
		UpdateTime();
	}

	private void UpdateTime()
	{
		string text = timeHour + ":";
		text = ((timeMinute >= 10) ? (text + timeMinute) : (text + "0" + timeMinute));
		textTime.text = text;
	}
}
