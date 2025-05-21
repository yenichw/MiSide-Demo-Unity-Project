using UnityEngine;
using UnityEngine.UI;

public class TextComputerAuto : MonoBehaviour
{
	public string[] stringsMethods;

	public string[] stringMathf;

	public string[] stringCode;

	public string[] stringReady;

	private Text txt;

	private float timePrint;

	private bool playPrint;

	private string textPrint;

	private float timeReady;

	private int timeRestart;

	private void Start()
	{
		txt = GetComponent<Text>();
		RestartText();
	}

	private void Update()
	{
		if (!playPrint)
		{
			return;
		}
		if (txt.text.Length < textPrint.Length)
		{
			timePrint += Time.deltaTime;
			if (timePrint > 0.03f)
			{
				timePrint = 0f;
				txt.text += textPrint[txt.text.Length];
			}
			return;
		}
		timeReady += Time.deltaTime;
		if (timeReady > 1f)
		{
			timeReady = 0f;
			txt.text += " .";
			timeRestart++;
			if (timeRestart > 4)
			{
				RestartText();
			}
		}
	}

	private void RestartText()
	{
		txt.text = "";
		timePrint = 0f;
		playPrint = true;
		textPrint = "";
		timeReady = 0f;
		timeRestart = 0;
		for (int i = 0; i < 3; i++)
		{
			int num = Random.Range(0, 3);
			if (num == 0)
			{
				textPrint = textPrint + stringsMethods[Random.Range(0, stringsMethods.Length)] + ";\n";
			}
			if (num == 1)
			{
				int num2 = Random.Range(0, 5);
				textPrint = textPrint + stringMathf[Random.Range(0, stringMathf.Length)] + " ";
				if (num2 == 0)
				{
					textPrint = textPrint + "+=" + Random.Range(-1000000f, 1000000f) + ";\n";
				}
				if (num2 == 1)
				{
					textPrint = textPrint + "-=" + Random.Range(-1000000f, 1000000f) + ";\n";
				}
				if (num2 == 2)
				{
					textPrint = textPrint + "/=" + Random.Range(-1000000f, 1000000f) + ";\n";
				}
				if (num2 == 3)
				{
					textPrint = textPrint + "*=" + Random.Range(-1000000f, 1000000f) + ";\n";
				}
				if (num2 == 4)
				{
					textPrint = textPrint + "%=" + Random.Range(-1000000f, 1000000f) + ";\n";
				}
			}
			if (num == 2)
			{
				textPrint = textPrint + stringCode[Random.Range(0, stringCode.Length)] + ";\n";
			}
		}
		textPrint += stringReady[Random.Range(0, stringReady.Length)];
	}
}
