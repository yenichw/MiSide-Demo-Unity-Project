using System.Collections.Generic;
using UnityEngine;

public class GlobalLanguage : MonoBehaviour
{
	public static List<LanguageFilesText> languageText;

	public static string GetString(string _name, int _string)
	{
		string result = null;
		for (int i = 0; i < languageText.Count; i++)
		{
			if (languageText[i].name == _name)
			{
				result = languageText[i].strings[_string];
				break;
			}
		}
		return result;
	}
}
