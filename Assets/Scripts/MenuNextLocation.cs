using UnityEngine;

public class MenuNextLocation : MonoBehaviour
{
	private Menu menu;

	public GameObject nextLocation;

	public GameObject changeCase;

	private void Start()
	{
		if (GameObject.FindWithTag("World").GetComponent<Menu>() != null)
		{
			menu = GameObject.FindWithTag("World").GetComponent<Menu>();
		}
	}

	public void Click()
	{
		menu.OpenNextLocation(nextLocation);
		menu.GetComponent<ButtonMouseMenu>().ChangeCase(changeCase);
	}
}
