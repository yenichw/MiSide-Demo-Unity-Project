using UnityEngine;

public class TamagotchiGame_Chip_Plita : MonoBehaviour
{
	public TamagotchiGame_Chip main;

	public bool canRight;

	public bool canLeft;

	public bool canUp;

	public bool canDown;

	public void TouchRight()
	{
		GameObject gameObject = null;
		if (!canRight)
		{
			return;
		}
		if (canLeft)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(-0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchRight();
			}
		}
		if (canUp)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, 0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchDown();
			}
		}
		if (canDown)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, -0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchUp();
			}
		}
	}

	public void TouchLeft()
	{
		GameObject gameObject = null;
		if (!canLeft)
		{
			return;
		}
		if (canRight)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchLeft();
			}
		}
		if (canUp)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, 0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchDown();
			}
		}
		if (canDown)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, -0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchUp();
			}
		}
	}

	public void TouchUp()
	{
		GameObject gameObject = null;
		if (!canUp)
		{
			return;
		}
		if (canLeft)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(-0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchRight();
			}
		}
		if (canRight)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchLeft();
			}
		}
		if (canDown)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, -0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchUp();
			}
		}
	}

	public void TouchDown()
	{
		GameObject gameObject = null;
		if (!canDown)
		{
			return;
		}
		if (canLeft)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(-0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchRight();
			}
		}
		if (canRight)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchLeft();
			}
		}
		if (canUp)
		{
			gameObject = main.CheckCast(base.transform.position + new Vector3(0f, 0f, 0.4f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchDown();
			}
		}
	}
}
