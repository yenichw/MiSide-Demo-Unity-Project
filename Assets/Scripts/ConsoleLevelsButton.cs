using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConsoleLevelsButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private int indexLevel;

	public Text textLevelName;

	public void StartComponent(int _indexLevel)
	{
		indexLevel = _indexLevel;
		textLevelName.text = SceneManager.GetSceneByBuildIndex(_indexLevel).name;
	}

	public void Click()
	{
		SceneManager.LoadScene(indexLevel);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Click();
	}
}
