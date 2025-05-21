using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Location16 : MonoBehaviour
{
	private GameController scrgc;

	private bool playStopTime;

	private bool ready;

	private float timeScaleRun;

	private float timeStop;

	private PlayerMove scrpm;

	private void Start()
	{
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		timeScaleRun = 1f;
	}

	private void Update()
	{
		if (!playStopTime || scrgc.isPaused)
		{
			return;
		}
		if ((double)timeScaleRun > 0.05)
		{
			timeScaleRun -= Time.unscaledDeltaTime * 0.05f;
			if ((double)timeScaleRun < 0.05)
			{
				timeScaleRun = 0.05f;
			}
		}
		if (timeScaleRun == 0.05f && timeStop < 1f)
		{
			timeStop += Time.unscaledDeltaTime;
			if (timeStop >= 1f)
			{
				timeStop = 1f;
				Time.timeScale = 1f;
				ready = true;
			}
		}
		Time.timeScale = timeScaleRun;
		scrpm.intensityMouse = timeScaleRun;
	}

	private void LateUpdate()
	{
		if (ready)
		{
			playStopTime = false;
			ready = false;
			StartCoroutine(RecordFrame());
		}
	}

	public void PlayTimeStop()
	{
		playStopTime = true;
	}

	private IEnumerator RecordFrame()
	{
		yield return new WaitForEndOfFrame();
		GlobalGame.screenCapture = ScreenCapture.CaptureScreenshotAsTexture();
		Time.timeScale = 1f;
		GlobalGame.gameEndingMenu = true;
		GlobalGame.playWorld = false;
		Object.Destroy(GlobalTag.gameController);
		SceneManager.LoadScene("SceneMenu");
	}
}
