using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Player/Need item")]
public class Player_NeedItem : MonoBehaviour
{
	public GameObject itemNeed;

	public UnityEvent eventExists;

	public UnityEvent eventMissing;

	private GameController scrgc;

	private bool itemExists;

	private bool fs;

	private void CheckItem()
	{
		if (scrgc.GetKeyItem(itemNeed) != -1)
		{
			if (!itemExists)
			{
				itemExists = true;
				eventExists.Invoke();
			}
		}
		else if (itemExists)
		{
			itemExists = false;
			eventMissing.Invoke();
		}
	}

	private void Start()
	{
		fs = true;
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		if (scrgc.GetKeyItem(itemNeed) == -1)
		{
			itemExists = false;
			eventMissing.Invoke();
		}
		if (scrgc.GetKeyItem(itemNeed) != -1)
		{
			itemExists = true;
			eventExists.Invoke();
		}
		scrgc.eventItemKeyAdd.AddListener(CheckItem);
		scrgc.eventItemKeyRemove.AddListener(CheckItem);
	}

	private void OnEnable()
	{
		if (fs)
		{
			CheckItem();
			scrgc.eventItemKeyAdd.AddListener(CheckItem);
			scrgc.eventItemKeyRemove.AddListener(CheckItem);
		}
	}

	private void OnDisable()
	{
		if (scrgc != null)
		{
			scrgc.eventItemKeyAdd.RemoveListener(CheckItem);
			scrgc.eventItemKeyRemove.RemoveListener(CheckItem);
		}
	}

	private void OnDestroy()
	{
		if (scrgc != null)
		{
			scrgc.eventItemKeyAdd.RemoveListener(CheckItem);
			scrgc.eventItemKeyRemove.RemoveListener(CheckItem);
		}
	}
}
