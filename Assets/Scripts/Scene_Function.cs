using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Function : MonoBehaviour
{
	public void NextScene(string nameScene)
	{
		SceneManager.LoadScene(nameScene);
	}
}
