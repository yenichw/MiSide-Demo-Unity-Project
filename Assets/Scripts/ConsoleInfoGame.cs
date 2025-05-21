using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConsoleInfoGame : MonoBehaviour
{
	public bool onlyFPS;

	private Text text;

	[HideInInspector]
	public ConsoleCall cm;

	private int fpsFrame;

	private float fpsTimeUpdate;

	private string fpsText;

	private int timeReload;

	private string nameScene;

	private void Start()
	{
		text = GetComponent<Text>();
		if (!onlyFPS)
		{
			ReloadData();
		}
	}

	private void Update()
	{
		fpsFrame++;
		fpsTimeUpdate += Time.deltaTime;
		if (fpsTimeUpdate >= 1f)
		{
			fpsText = fpsFrame.ToString() ?? "";
			fpsFrame = 0;
			fpsTimeUpdate = 0f;
		}
		if (onlyFPS)
		{
			this.text.text = "FPS: " + fpsFrame + " [" + fpsText + "]";
			return;
		}
		timeReload++;
		if (timeReload > 30)
		{
			timeReload = 0;
			ReloadData();
		}
		string text = "";
		text = ((!(ConsoleCall.timeSceneHours < 10f)) ? (text + ConsoleCall.timeSceneHours) : (text + "0" + ConsoleCall.timeSceneHours));
		text = ((!(ConsoleCall.timeSceneMinute < 10f)) ? (text + ":" + ConsoleCall.timeSceneMinute) : (text + ":0" + ConsoleCall.timeSceneMinute));
		text = ((!(ConsoleCall.timeSceneSecond < 10f)) ? (text + ":" + Mathf.Round(ConsoleCall.timeSceneSecond)) : (text + ":0" + Mathf.Round(ConsoleCall.timeSceneSecond)));
		this.text.text = "GAME: " + Application.productName + "\nTIME: " + text + "\nFPS [" + fpsText + "]\nLEVEL: " + nameScene;
	}

	private void ReloadData()
	{
		nameScene = SceneManager.GetActiveScene().name;
	}
}
