using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MenuHelpText : MonoBehaviour
{
	private int ihelp;

	private Text text;

	private void Start()
	{
		ihelp = 2;
		text = GetComponent<Text>();
		NextTip(0);
	}

	public void NextTip(int i)
	{
		ihelp += i;
		if (ihelp > File.ReadAllLines("Data/Languages/" + GlobalGame.Language + "/LoadingTip.txt").Length - 1)
		{
			ihelp = 2;
		}
		if (ihelp < 2)
		{
			ihelp = File.ReadAllLines("Data/Languages/" + GlobalGame.Language + "/LoadingTip.txt").Length - 1;
		}
		text.text = File.ReadAllLines("Data/Languages/" + GlobalGame.Language + "/LoadingTip.txt")[ihelp];
	}
}
