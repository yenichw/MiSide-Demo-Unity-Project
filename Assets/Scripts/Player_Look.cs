using UnityEngine;
using UnityEngine.Events;

public class Player_Look : MonoBehaviour
{
	public Transform targetLook;

	public float timeLook;

	public UnityEvent eventEnd;

	public bool itHint;

	public void Look()
	{
		GameObject.FindWithTag("Player").GetComponent<PlayerMove>().Look(targetLook, timeLook, eventEnd);
	}
}
