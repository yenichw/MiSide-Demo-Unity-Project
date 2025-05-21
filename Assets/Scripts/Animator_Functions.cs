using UnityEngine;
using UnityEngine.Events;

public class Animator_Functions : MonoBehaviour
{
	[Header("[EventKey] or [EventKeyFloat]")]
	public UnityEvent[] _events;

	public void BoolOn(string x)
	{
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().SetBool(x, value: true);
	}

	public void BoolOff(string x)
	{
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().SetBool(x, value: false);
	}

	public void TriggerClick(string x)
	{
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().SetTrigger(x);
	}

	public void BoolSwitch(string x)
	{
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().SetBool(x, !GetComponent<Animator>().GetBool(x));
	}

	public void EventKey(int x)
	{
		GetComponent<Animator>().enabled = true;
		_events[x].Invoke();
	}

	public void AnimationFinish()
	{
		Object.Destroy(GetComponent<Animator>());
		Object.Destroy(this);
	}

	public void AnimatorRuntime(RuntimeAnimatorController _animatorRuntime)
	{
		GetComponent<Animator>().runtimeAnimatorController = _animatorRuntime;
	}

	public void AnimatorFinish()
	{
		GetComponent<Animator>().enabled = false;
		Object.Destroy(this);
	}

	public void AnimationPlayState(string _nameState)
	{
		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().Play(_nameState, 0, 0f);
	}
}
