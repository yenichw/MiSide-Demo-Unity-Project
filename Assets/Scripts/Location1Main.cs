using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location1Main : MonoBehaviour
{
	public Text textMyName;

	public Tamagotchi_Main tamagotchi;

	private bool canBuyTelevision;

	[Header("Купи телевизор")]
	public UnityEvent eventBuyTelevision;

	public void SaveName()
	{
		GlobalGame.namePlayer = textMyName.text;
		PlayerPrefs.SetString("NamePlayer", GlobalGame.namePlayer);
	}

	public void CheckMoneyForTelevision()
	{
		if (!canBuyTelevision && tamagotchi.money >= 450)
		{
			ConsoleMain.ConsolePrintGame("Money 450!");
			canBuyTelevision = true;
			eventBuyTelevision.Invoke();
		}
	}
}
