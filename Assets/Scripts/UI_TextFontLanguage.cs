using UnityEngine;
using UnityEngine.UI;

public class UI_TextFontLanguage : MonoBehaviour
{
	private void Start()
	{
		FontUpdate();
	}

	public void FontUpdate()
	{
		if (GlobalGame.fontUse != null)
		{
			GetComponent<Text>().font = GlobalGame.fontUse;
		}
	}
}
