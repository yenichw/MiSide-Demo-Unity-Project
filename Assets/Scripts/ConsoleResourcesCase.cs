using UnityEngine;

public class ConsoleResourcesCase : MonoBehaviour
{
	[HideInInspector]
	public GameObject objectResource;

	[HideInInspector]
	public ConsoleInterface console;

	public void Click()
	{
		GameObject gameObject = Object.Instantiate(objectResource);
		console.SoundPlay(0);
		if (console.resourcesTypeCreate == 0 && GameObject.FindWithTag("MainCamera") != null)
		{
			gameObject.transform.position = GameObject.FindWithTag("MainCamera").transform.position + GameObject.FindWithTag("MainCamera").transform.forward * 3f;
		}
		if (console.resourcesTypeCreate == 1 && GameObject.FindWithTag("Player") != null)
		{
			gameObject.transform.position = GameObject.FindWithTag("Player").transform.position;
		}
	}
}
