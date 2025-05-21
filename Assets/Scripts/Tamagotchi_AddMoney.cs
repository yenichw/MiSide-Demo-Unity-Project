using UnityEngine;
using UnityEngine.UI;

public class Tamagotchi_AddMoney : MonoBehaviour
{
	public Tamagotchi_Main main;

	public Text textMoney;

	public RectTransform rect;

	public Image image;

	public Image imageIcon;

	public AnimationCurve animationActivated;

	public ButtonMouseClick buttonCloseMiniGame;

	private float timeAnimation;

	private void Update()
	{
		if (timeAnimation < 1f)
		{
			timeAnimation += Time.deltaTime;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			rect.anchoredPosition = new Vector2(0f, -40f * (1f - animationActivated.Evaluate(timeAnimation)));
			image.color = new Color(1f, 1f, 1f, animationActivated.Evaluate(timeAnimation));
			textMoney.color = new Color(1f, 1f, 1f, animationActivated.Evaluate(timeAnimation));
		}
	}

	public void StartAddMoney(int _countMoney, int _removeEnergy)
	{
		ConsoleMain.ConsolePrintGame("AddMoney (" + _countMoney + ")...");
		buttonCloseMiniGame.ActivationInteractive(x: false);
		timeAnimation = 0f;
		base.gameObject.SetActive(value: true);
		main.energy -= _removeEnergy;
		main.MoneyAdd(_countMoney);
		textMoney.text = "+" + _countMoney;
		GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
		GetComponent<AudioSource>().Play();
		ConsoleMain.ConsolePrintGameAdd("Ready!");
	}
}
