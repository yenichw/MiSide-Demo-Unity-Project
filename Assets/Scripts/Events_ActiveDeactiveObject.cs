using UnityEngine;
using UnityEngine.Events;

public class Events_ActiveDeactiveObject : MonoBehaviour
{
	public bool active = true;

	public UnityEvent eventDisable;

	public UnityEvent eventEnable;

	private void OnEnable()
	{
		if (active)
		{
			eventEnable.Invoke();
		}
	}

	private void OnDisable()
	{
		if (active)
		{
			eventDisable.Invoke();
		}
	}

	public void Activation(bool x)
	{
		active = x;
	}
}
