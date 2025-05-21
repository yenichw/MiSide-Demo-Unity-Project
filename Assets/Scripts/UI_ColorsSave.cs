using UnityEngine;
using UnityEngine.UI;

public class UI_ColorsSave : MonoBehaviour
{
	private Color clr;

	public Color GetColor()
	{
		return clr;
	}

	public void SetColor(Color _clr)
	{
		clr = _clr;
	}

	private void OnValidate()
	{
		if (GetComponent<Image>() != null)
		{
			clr = GetComponent<Image>().color;
		}
		if (GetComponent<Text>() != null)
		{
			clr = GetComponent<Text>().color;
		}
	}
}
