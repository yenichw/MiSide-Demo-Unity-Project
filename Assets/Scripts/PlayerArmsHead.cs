using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerArmsHead : MonoBehaviour
{
	[HideInInspector]
	public List<UnityEvent> eventsPlayer;

	public void EventKey(int _x)
	{
		eventsPlayer[_x].Invoke();
	}
}
