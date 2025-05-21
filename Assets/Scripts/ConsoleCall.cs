using UnityEngine;

public class ConsoleCall : MonoBehaviour
{
	[Header("CONSOLE")]
	public GameObject consoleObject;

	public GameObject consoleObjectInformation;

	[Header("Управление временем")]
	public float slowTime = 0.1f;

	public float slowTimeX2 = 0.01f;

	public float fastTime = 2f;

	private float timeHoldForFastTime;

	private GameObject cosnoleInfoNow;

	public static float timeSceneSecond;

	public static float timeSceneMinute;

	public static float timeSceneHours;

	private GameObject consoleObj;

	private void Start()
	{
		ConsoleMain.active = false;
		timeSceneSecond = 0f;
		timeSceneMinute = 0f;
		timeSceneHours = 0f;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.BackQuote) && consoleObj == null)
		{
			consoleObj = Object.Instantiate(consoleObject);
			consoleObj.GetComponent<ConsoleInterface>().objectCallMe = base.gameObject;
			ConsoleMain.active = true;
		}
		if (ConsoleMain.dev)
		{
			if (Input.GetMouseButtonDown(4) || Input.GetKeyDown(KeyCode.Keypad0))
			{
				if (Time.timeScale != slowTime)
				{
					Time.timeScale = slowTime;
				}
				else
				{
					Time.timeScale = slowTimeX2;
				}
			}
			if (Input.GetMouseButtonDown(3) || Input.GetKeyDown(KeyCode.Keypad1))
			{
				Time.timeScale = 1f;
				timeHoldForFastTime = 0f;
			}
			if (Input.GetMouseButton(3) || Input.GetKey(KeyCode.Keypad1))
			{
				timeHoldForFastTime += Time.unscaledDeltaTime;
				if (timeHoldForFastTime > 0.5f)
				{
					Time.timeScale = fastTime;
				}
			}
		}
		timeSceneSecond += Time.unscaledDeltaTime;
		if (timeSceneSecond >= 60f)
		{
			timeSceneSecond -= 60f;
			timeSceneMinute += 1f;
		}
		if (timeSceneMinute >= 60f)
		{
			timeSceneMinute -= 60f;
			timeSceneHours += 1f;
		}
	}

	public void ShowInformation()
	{
		if (cosnoleInfoNow == null)
		{
			cosnoleInfoNow = Object.Instantiate(consoleObjectInformation);
			cosnoleInfoNow.transform.Find("Frame/Text").GetComponent<ConsoleInfoGame>().cm = this;
		}
		else
		{
			Object.Destroy(cosnoleInfoNow);
		}
	}
}
