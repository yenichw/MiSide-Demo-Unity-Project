using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tamagotchi_BuyCase : MonoBehaviour
{
	public bool close;

	public int money;

	public Tamagotchi_Main main;

	public UnityEvent eventBuy;

	public Image frameEnergy;

	public AudioSource audioNope;

	private float timeRed;

	private void Start()
	{
		GetComponent<ButtonMouseClick>().eventClick.AddListener(Click);
	}

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
		if (!close)
		{
			if (main.money >= money)
			{
				main.MoneyAdd(-money);
				main.UpdateCasesBuy();
				eventBuy.Invoke();
			}
			else
			{
				frameEnergy.color = new Color(1f, 0f, 0f);
				timeRed = 0.5f;
				audioNope.pitch = Random.Range(0.95f, 1.05f);
				audioNope.Play();
			}
		}
	}

	private void OnEnable()
	{
		UpdateColorBuy();
	}

	public void UpdateColorBuy()
	{
		if (main.money < money)
		{
			GetComponent<ButtonMouseClick>().colorEnter = new Color(0.9f, 0.9f, 0.9f, 1f);
			GetComponent<ButtonMouseClick>().colorExit = new Color(0.8f, 0.8f, 0.8f, 1f);
		}
		else
		{
			GetComponent<ButtonMouseClick>().colorEnter = new Color(1f, 0.7f, 0.8f, 1f);
			GetComponent<ButtonMouseClick>().colorExit = new Color(1f, 1f, 1f, 1f);
		}
	}
}
