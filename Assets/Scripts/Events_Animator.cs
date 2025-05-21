using UnityEngine;

public class Events_Animator : MonoBehaviour
{
	[Header("EventAnim (int)")]
	public Events_AnimatorEvent[] events;

	public void EventAnim(int _index)
	{
		events[_index].eventAnim.Invoke();
	}

	public void CopyEvents(Events_Animator _component)
	{
		events = _component.events;
	}
}
