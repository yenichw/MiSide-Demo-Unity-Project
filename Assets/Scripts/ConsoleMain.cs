using UnityEngine;

public class ConsoleMain : MonoBehaviour
{
	public static bool active;

	public static bool debugUnity;

	public static string[] console_lastCodes = new string[100];

	public static int console_iLastCode;

	public static bool dev;

	public static string consoleText = "";

	public static string consoleText2 = "";

	public static void ConsolePrintGame(string text)
	{
		if (debugUnity)
		{
			Debug.Log("<color=#abff50>" + text + "</color>");
		}
		ConsoleAddFix("\n" + ConsoleGetTime() + " <color=#abff50>" + text + "</color>");
	}

	public static void ConsolePrintGameAdd(string text)
	{
		ConsoleAddFix("<color=#abff50>" + text + "</color>");
	}

	public static void ConsolePrintWarning(string text)
	{
		if (debugUnity)
		{
			Debug.LogError("<color=#ff2030>" + text + "</color>");
		}
		ConsoleAddFix("\n" + ConsoleGetTime() + " <color=#ff2030>" + text + "</color>");
	}

	public static void ConsolePrintWarningAdd(string text)
	{
		ConsoleAddFix("<color=#ff2030>" + text + "</color>");
	}

	public static void ConsolePrintCheat(string text)
	{
		if (debugUnity)
		{
			Debug.Log("<color=#fb9920>" + text + "</color>");
		}
		ConsoleAddFix("\n" + ConsoleGetTime() + " <color=#fb9920>" + text + "</color>");
	}

	public static void ConsolePrintCheatAdd(string text)
	{
		ConsoleAddFix("<color=#fb9920>" + text + "</color>");
	}

	public static void ConsolePrint(string text)
	{
		text = text.Replace("<", "");
		text = text.Replace(">", "");
		ConsoleAddFix("\n" + text);
	}

	public static void ConsolePrintAdd(string text)
	{
		ConsoleAddFix(text);
	}

	public static void ConsoleAddFix(string text)
	{
		if (consoleText.Length + text.Length >= 8001)
		{
			consoleText2 = consoleText;
			consoleText = text;
		}
		else
		{
			consoleText += text;
		}
	}

	private static string ConsoleGetTime()
	{
		string text = "[";
		text = ((!(ConsoleCall.timeSceneHours < 10f)) ? (text + ConsoleCall.timeSceneHours) : (text + "0" + ConsoleCall.timeSceneHours));
		text = ((!(ConsoleCall.timeSceneMinute < 10f)) ? (text + ":" + ConsoleCall.timeSceneMinute) : (text + ":0" + ConsoleCall.timeSceneMinute));
		text = ((!(Mathf.Round(ConsoleCall.timeSceneSecond) < 10f)) ? (text + ":" + Mathf.Round(ConsoleCall.timeSceneSecond)) : (text + ":0" + Mathf.Round(ConsoleCall.timeSceneSecond)));
		return text + "]";
	}
}
