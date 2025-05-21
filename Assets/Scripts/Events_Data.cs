using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Event/Event")]
public class Events_Data : MonoBehaviour
{
	[Header("Method [EV(int)]")]
	public bool onStartZeroIndex;

	public UnityEvent[] _event;

	private void Start()
	{
		if (onStartZeroIndex)
		{
			_event[0].Invoke();
		}
	}

	public void EV(int x)
	{
		_event[x].Invoke();
	}

	public void NewEvent(int x)
	{
		_event[x].Invoke();
	}
}
