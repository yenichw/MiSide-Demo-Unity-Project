using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interface_ChangeScreenButton : MonoBehaviour
{
	public enum TypeChangeScreenButton
	{
		ReturnString = 0,
		ReturnInt = 1
	}

	public GameObject buttonCancel;

	public UnityEvent eventReturn;

	public List<Interface_ChangeScreenButton_Class_ButtonInfo> casesButton;

	public UI_Colors scriptColors;

	public GameObject buttonExample;

	private TypeChangeScreenButton typeChangeScreenButton = TypeChangeScreenButton.ReturnInt;

	[HideInInspector]
	public string returnString;

	[HideInInspector]
	public int returnInt;

	public List<GameObject> buttonsNow;

	public void ClickTable(int index)
	{
		if (typeChangeScreenButton == TypeChangeScreenButton.ReturnInt)
		{
			returnInt = casesButton[index].value_int;
		}
		if (typeChangeScreenButton == TypeChangeScreenButton.ReturnString)
		{
			returnString = casesButton[index].value_string;
		}
		eventReturn.Invoke();
		Close();
	}

	public void Create(string stringNameLocation, List<Interface_ChangeScreenButton_Class_ButtonInfo> buttonInfo, TypeChangeScreenButton typeReturn, GameObject returnLocation, GameObject returnButton)
	{
		if (GetComponent<MenuNextLocation>() != null)
		{
			GetComponent<MenuNextLocation>().nextLocation = returnLocation;
			GetComponent<MenuNextLocation>().changeCase = returnButton;
		}
		typeChangeScreenButton = typeReturn;
		base.transform.Find("TextLocation").GetComponent<Text>().text = stringNameLocation;
		casesButton = buttonInfo;
		int num = 15;
		buttonExample.SetActive(value: true);
		for (int i = 0; i < casesButton.Count; i++)
		{
			GameObject gameObject = Object.Instantiate(buttonExample, base.transform.Find("Scroll View/Viewport/Content"));
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, -num);
			gameObject.GetComponent<Interface_ChangeScreenButton_Button>().scricsb = this;
			gameObject.GetComponent<Interface_ChangeScreenButton_Button>().index = i;
			gameObject.transform.Find("Text").GetComponent<Text>().text = casesButton[i].buttonText;
			if (i == 0)
			{
				gameObject.GetComponent<ButtonMouseClick>().changeUp = buttonCancel.GetComponent<ButtonMouseClick>();
			}
			if (i > 0)
			{
				gameObject.GetComponent<ButtonMouseClick>().changeUp = buttonsNow[i - 1].GetComponent<ButtonMouseClick>();
			}
			if (i == casesButton.Count - 1)
			{
				gameObject.GetComponent<ButtonMouseClick>().changeDown = buttonCancel.GetComponent<ButtonMouseClick>();
			}
			if (i > 0)
			{
				buttonsNow[i - 1].GetComponent<ButtonMouseClick>().changeDown = gameObject.GetComponent<ButtonMouseClick>();
			}
			num += 60;
			buttonsNow.Add(gameObject);
		}
		buttonExample.SetActive(value: false);
		buttonCancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, -num);
		if (buttonsNow[0] != null)
		{
			buttonCancel.GetComponent<ButtonMouseClick>().changeDown = buttonsNow[0].GetComponent<ButtonMouseClick>();
		}
		if (buttonsNow[casesButton.Count - 1] != null)
		{
			buttonCancel.GetComponent<ButtonMouseClick>().changeUp = buttonsNow[casesButton.Count - 1].GetComponent<ButtonMouseClick>();
		}
		base.transform.Find("Scroll View/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(0f, num + 60);
		if (scriptColors != null)
		{
			scriptColors.Reload();
		}
	}

	public void Close()
	{
		for (int i = 0; i < buttonsNow.Count; i++)
		{
			if (buttonsNow[i] != null)
			{
				Object.Destroy(buttonsNow[i]);
			}
		}
		buttonsNow.Clear();
		if (GetComponent<MenuNextLocation>() != null)
		{
			GetComponent<MenuNextLocation>().Click();
		}
	}
}
