using UnityEngine;

public class DEMO_FinishCanvas : MonoBehaviour
{
	private float timeCanSkip;

	public GameObject buttonContinue;

	private void Update()
	{
		if (timeCanSkip < 6f)
		{
			timeCanSkip += Time.deltaTime;
			if (timeCanSkip > 6f)
			{
				timeCanSkip = 6f;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				GlobalGame.gameEndingMenu = true;
				buttonContinue.SetActive(value: true);
			}
		}
	}

	public void Click()
	{
		if (timeCanSkip >= 6f)
		{
			timeCanSkip = -100f;
			GlobalTag.gameController.GetComponent<GameController>().ExitGame();
		}
	}
}
