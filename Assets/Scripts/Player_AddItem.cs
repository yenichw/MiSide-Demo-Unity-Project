using UnityEngine;

public class Player_AddItem : MonoBehaviour
{
	public void AddItemKey(GameObject _object)
	{
		GameObject.FindWithTag("GameController").GetComponent<GameController>().AddKeyItem(_object);
	}

	public void RemoveItemKey(GameObject _object)
	{
		GameObject.FindWithTag("GameController").GetComponent<GameController>().RemoveKeyItem(_object);
	}
}
