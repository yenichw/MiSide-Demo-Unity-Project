using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleHierarchyButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private int indexObject;

	private ConsoleInterface main;

	public void StartComponent(ConsoleInterface _main, int _indexObject)
	{
		main = _main;
		indexObject = _indexObject;
	}

	public void Click()
	{
		main.hierarchyObjectsNormal[indexObject].open = !main.hierarchyObjectsNormal[indexObject].open;
		main.HierarchyUpdate();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Click();
	}
}
