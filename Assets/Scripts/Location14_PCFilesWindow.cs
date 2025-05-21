using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Location14_PCFilesWindow : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Location14_PCGames main;

	public int indexWindow;

	private void OnEnable()
	{
		GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		main.windowFileChange = indexWindow;
		GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.4f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		main.windowFileChange = 0;
		GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
	}
}
