using UnityEngine;
using UnityEngine.UI;

public class UI_Color : MonoBehaviour
{
	public Color[] colors;

	public void SetColor(int _index)
	{
		GetComponent<Image>().color = colors[_index];
	}
}
