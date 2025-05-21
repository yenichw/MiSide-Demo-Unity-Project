using UnityEngine;

public class MenuLocation : MonoBehaviour
{
	private bool active;

	private float countCases;

	private float timeDisable;

	public RectTransform[] objects;

	private void Start()
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].anchoredPosition = new Vector2(-800f, objects[i].anchoredPosition.y);
		}
	}

	private void Update()
	{
		if (active)
		{
			for (int i = 0; i < Mathf.FloorToInt(countCases); i++)
			{
				if (objects[i] != null)
				{
					objects[i].anchoredPosition = Vector2.Lerp(objects[i].anchoredPosition, new Vector2(0f, objects[i].anchoredPosition.y), Time.unscaledDeltaTime * 15f);
				}
			}
		}
		else
		{
			for (int j = 0; j < Mathf.FloorToInt(countCases); j++)
			{
				if (objects[j] != null)
				{
					objects[j].anchoredPosition = Vector2.Lerp(objects[j].anchoredPosition, new Vector2(300f, objects[j].anchoredPosition.y), Time.unscaledDeltaTime * 15f);
					objects[j].GetComponent<UI_Colors>().Hide(x: true, _fast: false);
				}
			}
			if (timeDisable > 0f)
			{
				timeDisable -= Time.deltaTime;
				if (timeDisable <= 0f)
				{
					base.gameObject.SetActive(value: false);
				}
			}
		}
		if (countCases < (float)objects.Length)
		{
			countCases += Time.unscaledDeltaTime * 40f;
			if (countCases > (float)objects.Length)
			{
				countCases = objects.Length;
				timeDisable = 1f;
			}
		}
	}

	public void Active(bool x)
	{
		active = x;
		countCases = 0f;
		if (x)
		{
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] != null)
				{
					objects[i].anchoredPosition = new Vector2(-800f, objects[i].anchoredPosition.y);
					if (!objects[i].GetComponent<UI_Colors>().onEnableInvisible)
					{
						objects[i].GetComponent<UI_Colors>().Hide(x: false, _fast: true);
					}
					else
					{
						objects[i].GetComponent<UI_Colors>().Hide(x: false, _fast: false);
					}
					if (objects[i].GetComponent<ButtonMouseClick>() != null)
					{
						objects[i].GetComponent<ButtonMouseClick>().LockClick(x: false);
					}
				}
			}
			return;
		}
		for (int j = 0; j < objects.Length; j++)
		{
			if (objects[j] != null && objects[j].GetComponent<ButtonMouseClick>() != null)
			{
				objects[j].GetComponent<ButtonMouseClick>().LockClick(x: true);
			}
		}
	}
}
