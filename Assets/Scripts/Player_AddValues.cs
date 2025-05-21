using UnityEngine;

public class Player_AddValues : MonoBehaviour
{
	public string nameValue;

	public int score;

	public void AddValue()
	{
		GameObject.FindWithTag("GameController").GetComponent<ValuesPlayer>().AddValueInt(nameValue, score);
	}

	public void SetValue()
	{
		GameObject.FindWithTag("GameController").GetComponent<ValuesPlayer>().SetValueInt(nameValue, score);
	}
}
