using UnityEngine;

public class Events_Distance : MonoBehaviour
{
	public Transform target;

	public EventDistance[] events;

	private void Start()
	{
		if (target == null)
		{
			target = GameObject.FindWithTag("Player").transform;
		}
	}

	private void Update()
	{
		for (int i = 0; i < events.Length; i++)
		{
			if (events[i].position != -1 && Vector3.Distance(base.transform.position, target.position) < events[i].distance)
			{
				events[i].eventLess.Invoke();
				events[i].position = -1;
			}
			if (events[i].position != 1 && Vector3.Distance(base.transform.position, target.position) >= events[i].distance)
			{
				events[i].eventMore.Invoke();
				events[i].position = 1;
			}
		}
	}
}
