using System.IO;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Functions/Localization/Localization UI Text")]
public class Localization_UIText : MonoBehaviour
{
	public bool EveryEnable;

	public string NameFile;

	public int StringNumber = 1;

	public bool GrandSymbol = true;

	public bool data;

	public bool dontAutoTranslate;

	public bool dontAutoFont;

	private bool fs;

	public void OnEnable()
	{
		if (!dontAutoTranslate && fs && EveryEnable)
		{
			TextTranslate();
		}
	}

	private void Start()
	{
		if (!dontAutoTranslate)
		{
			fs = true;
			TextTranslate();
		}
	}

	public void TextTranslate()
	{
		fs = true;
		if (GlobalGame.fontUse != null && !dontAutoFont)
		{
			GetComponent<Text>().font = GlobalGame.fontUse;
		}
		string text = null;
		if (!data)
		{
			text = GlobalLanguage.GetString(NameFile, StringNumber - 1);
		}
		if (data)
		{
			text = File.ReadAllLines("Data/" + NameFile + ".txt")[StringNumber - 1];
		}
		if (text != null)
		{
			if (GrandSymbol)
			{
				text = text.ToUpper();
			}
			GetComponent<Text>().text = text;
		}
		else
		{
			GetComponent<Text>().text = "???";
		}
	}

	public void ReString(int x)
	{
		StringNumber = x;
	}

	public void ReFile(string x)
	{
		NameFile = x;
	}

	public void DestroyObject()
	{
		Object.Destroy(base.gameObject);
	}
}
