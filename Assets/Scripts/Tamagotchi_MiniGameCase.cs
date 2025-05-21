using UnityEngine;
using UnityEngine.UI;

public class Tamagotchi_MiniGameCase : MonoBehaviour
{
	public int energy;

	public Tamagotchi_Main main;

	public Tamagotchi_MiniGame miniGame;

	public AudioSource audioNope;

	private float timeRed;

	[Header("Интерфейс")]
	public Image frameEnergy;

	private void Update()
	{
		if ((double)frameEnergy.color.r > 0.1)
		{
			if (timeRed > 0f)
			{
				timeRed -= Time.deltaTime;
			}
			else
			{
				frameEnergy.color = Color.Lerp(frameEnergy.color, new Color(87f / 106f, 0.375534f, 0.6094635f), Time.deltaTime * 5f);
			}
		}
	}

	public void Click()
	{
		if (main.energy >= energy)
		{
			main.MiniGamePlay(miniGame);
			return;
		}
		main.EnergyAlphaAnimation();
		frameEnergy.color = new Color(1f, 0f, 0f);
		timeRed = 0.5f;
		audioNope.pitch = Random.Range(0.95f, 1.05f);
		audioNope.Play();
	}

	private void OnEnable()
	{
		if (main.energy < energy)
		{
			GetComponent<UI_Colors>().SetColorImage(0, new Color(0.6f, 0.6f, 0.6f, 1f));
			GetComponent<ButtonMouseClick>().colorEnter = new Color(0.7f, 0.7f, 0.7f, 1f);
			GetComponent<ButtonMouseClick>().colorExit = new Color(0.6f, 0.6f, 0.6f, 1f);
		}
		else
		{
			GetComponent<UI_Colors>().SetColorImage(0, new Color(1f, 1f, 1f, 1f));
			GetComponent<ButtonMouseClick>().colorEnter = new Color(0.9f, 0.9f, 0.9f, 1f);
			GetComponent<ButtonMouseClick>().colorExit = new Color(1f, 1f, 1f, 1f);
		}
	}
}
