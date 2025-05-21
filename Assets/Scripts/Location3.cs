using UnityEngine;
using UnityEngine.UI;

public class Location3 : MonoBehaviour
{
	public Text[] textesName;

	public void Start()
	{
		for (int i = 0; i < textesName.Length; i++)
		{
			textesName[i].text = GlobalLanguage.GetString("Names", Random.Range(9, 47));
		}
	}
}
