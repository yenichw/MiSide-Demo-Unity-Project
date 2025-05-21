using EPOOutline;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outlinable))]
public class Location14_QuestInteractive : MonoBehaviour
{
	public Location14_GameQuestPlayer main;

	public Vector3 positionMove;

	public UnityEvent eventFinishMove;

	private bool mouse;

	private Outlinable outline;

	private void Start()
	{
		outline = GetComponent<Outlinable>();
		outline.enabled = false;
	}

	private void Update()
	{
		if (main.objectMouse == base.gameObject)
		{
			if (!mouse)
			{
				mouse = true;
				outline.enabled = true;
			}
		}
		else if (mouse)
		{
			mouse = false;
			outline.enabled = false;
		}
	}

	public void Deactivation()
	{
		outline.enabled = false;
		base.enabled = false;
	}

	public void Activation()
	{
		base.enabled = true;
		mouse = false;
		outline.enabled = false;
	}

	public void DestroyMe()
	{
		Object.Destroy(base.gameObject);
	}
}
