using UnityEngine;
using UnityEngine.Events;

public class Animator_OneTimeDestroy : MonoBehaviour
{
	public AnimationClip animationTime;

	public bool destroyObject;

	public UnityEvent eventFinish;

	private float timeAnimation;

	private void Update()
	{
		timeAnimation += Time.deltaTime;
		if (timeAnimation >= animationTime.length)
		{
			if (destroyObject)
			{
				Object.Destroy(base.gameObject);
			}
			eventFinish.Invoke();
			GetComponent<Animator>().enabled = false;
			Object.Destroy(this);
			if (GetComponent<Events_Data>() != null)
			{
				Object.Destroy(GetComponent<Events_Data>());
			}
		}
	}

	public void AticeObject()
	{
		GetComponent<Animator>().enabled = true;
		base.enabled = true;
	}

	[ContextMenu("Запустить")]
	public void ActiveObject()
	{
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().enabled = true;
		base.enabled = true;
	}
}
