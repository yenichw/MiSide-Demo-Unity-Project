using System.IO;
using UnityEngine;

public class MenuCaseLoad : MonoBehaviour
{
	public string fileNameLoad;

	public string nameSceneLoad;

	public Menu menu;

	private void Start()
	{
		GetComponent<ButtonMouseClick>().ActivationInteractive(x: false);
		if (Directory.Exists(Application.persistentDataPath + "/Save") && File.Exists(Application.persistentDataPath + "/Save/" + fileNameLoad))
		{
			GetComponent<ButtonMouseClick>().ActivationInteractive(x: true);
			GetComponent<ButtonMouseClick>().eventClick.AddListener(Click);
		}
	}

	public void Click()
	{
		menu.ButtonLoadScene(nameSceneLoad);
	}
}
