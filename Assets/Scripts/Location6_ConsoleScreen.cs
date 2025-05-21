using UnityEngine;
using UnityEngine.Events;

public class Location6_ConsoleScreen : MonoBehaviour
{
	public UnityEvent eventReady;

	public GameObject[] destroyItemsForHands;

	private bool ready;

	private bool stopActive;

	[Space(20f)]
	public bool playerWhoBusy;

	public bool mitaBusy;

	private void Update()
	{
		if (!ready && stopActive && !playerWhoBusy && !mitaBusy)
		{
			eventReady.Invoke();
			ready = true;
		}
	}

	public void PlayerWhoBusy(bool x)
	{
		playerWhoBusy = x;
	}

	public void MitaBusy(bool x)
	{
		mitaBusy = x;
	}

	public void PlayerClickStop()
	{
		stopActive = true;
	}

	public void DestroyItems()
	{
		for (int i = 0; i < destroyItemsForHands.Length; i++)
		{
			Object.Destroy(destroyItemsForHands[i]);
		}
	}
}
