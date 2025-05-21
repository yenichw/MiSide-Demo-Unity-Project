using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("Functions/Scene/Scene load async")]
public class Scene_Load : MonoBehaviour
{
	public UnityEvent eventLoadFinish;

	public string nameSceneLoad;

	public string nameSceneUnload;

	public bool nextOnReadyProgress;

	public LoadSceneMode modeLoad;

	[Header("Сохранение")]
	public GameObject objectSaveInterface;

	public string nameSceneContinue;

	public int stringFileNamePart;

	public string fileSave;

	[Space(20f)]
	public bool demoFinish;

	private bool loadPause;

	private bool loadFinish;

	private AsyncOperation asyncLoad;

	private void Update()
	{
		if (asyncLoad != null && !loadFinish && asyncLoad.progress >= 0.9f)
		{
			loadFinish = true;
			eventLoadFinish.Invoke();
			ConsoleMain.ConsolePrintGame("Scene_Load.asyncLoad.ready;");
			GlobalTag.gameController.GetComponent<GameController>().SceneLoadAsyncReady();
			if (nextOnReadyProgress)
			{
				GoScene();
			}
			if (loadPause)
			{
				GoSceneAfterPause();
			}
		}
	}

	[ContextMenu("Начать загрузку сцены")]
	public void StartLoad()
	{
		if (!demoFinish && Application.isPlaying)
		{
			asyncLoad = SceneManager.LoadSceneAsync(nameSceneLoad, modeLoad);
			asyncLoad.allowSceneActivation = false;
			GlobalTag.gameController.GetComponent<GameController>().StartSceneLoadAsync(asyncLoad);
			Debug.Log("Start load scene.");
		}
	}

	public void GoScene()
	{
		Debug.Log("GoScene.");
		if (!demoFinish)
		{
			if (asyncLoad.progress >= 0.9f)
			{
				asyncLoad.allowSceneActivation = true;
				GlobalTag.gameController.GetComponent<GameController>().FinishSceneLoadAsync();
			}
			else
			{
				loadPause = true;
				GlobalTag.gameController.GetComponent<GameController>().Pause(x: true, _isLoading: true);
			}
		}
		else
		{
			eventLoadFinish.Invoke();
		}
	}

	private void GoSceneAfterPause()
	{
		loadPause = false;
		asyncLoad.allowSceneActivation = true;
		GlobalTag.gameController.GetComponent<GameController>().FinishSceneLoadAsync();
		GlobalTag.gameController.GetComponent<GameController>().Pause(x: false, _isLoading: true);
	}

	public void UnloadScene(string _nameScene)
	{
		if (SceneManager.sceneCount > 1)
		{
			SceneManager.UnloadSceneAsync(_nameScene);
		}
	}

	public void SaveGame()
	{
		if (GetComponent<World>().isContinue)
		{
			Object.Instantiate(objectSaveInterface).transform.Find("Frame/Text").GetComponent<Text>().text = GlobalLanguage.GetString("Menu", stringFileNamePart - 1);
			GlobalAM.SaveData(fileSave, "");
			GlobalAM.SaveData("Continue", nameSceneContinue + "\n" + Mathf.RoundToInt(GlobalGame.timeS) + "\n" + GlobalGame.timeM + "\n" + GlobalGame.timeH);
		}
	}
}
