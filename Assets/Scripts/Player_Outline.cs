using UnityEngine;

public class Player_Outline : MonoBehaviour
{
	public Color[] colors;

	public void SetColor(int x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().animPerson.GetComponent<PlayerPerson>().SetColorOutline(colors[x]);
	}
}
