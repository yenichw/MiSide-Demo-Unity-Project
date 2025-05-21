using UnityEngine;
using UnityEngine.UI;

public class InputKeyboard_Key : MonoBehaviour
{
	public InputKeyboard_Main main;

	public Text text;

	public void ReText(string _s)
	{
		text.text = _s;
	}

	public void Click()
	{
		main.AddKey(text.text);
	}
}
