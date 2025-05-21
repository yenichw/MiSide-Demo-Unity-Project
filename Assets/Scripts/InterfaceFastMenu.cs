using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceFastMenu : MonoBehaviour
{
	public AudioSource musicPause;

	public Text textHint;

	public AnimationCurve clr;

	[Header("Диалоги")]
	public RectTransform rectDialogues;

	public GameObject objectDialogueExample;

	public void StartComponent(GameController _scrgc)
	{
		musicPause.time = Random.Range(0f, musicPause.clip.length);
		musicPause.transform.parent = null;
		World component = GameObject.FindWithTag("World").GetComponent<World>();
		if (component.showHint)
		{
			textHint.text = GlobalLanguage.GetString("LocationHint " + component.nameLocation, component.indexHint - 1);
		}
		else
		{
			Object.Destroy(textHint.transform.parent.gameObject);
			rectDialogues.anchoredPosition = new Vector2(rectDialogues.anchoredPosition.x, -270f);
			rectDialogues.sizeDelta = new Vector2(rectDialogues.sizeDelta.x, 325f);
		}
		if (_scrgc.dialoguesMemory != null && _scrgc.dialoguesMemory.Count > 0)
		{
			int num = -5;
			for (int num2 = _scrgc.dialoguesMemory.Count - 1; num2 >= 0; num2--)
			{
				GameObject gameObject = Object.Instantiate(objectDialogueExample, objectDialogueExample.transform.parent);
				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(gameObject.GetComponent<RectTransform>().anchoredPosition.x, num);
				Text component2 = gameObject.transform.Find("Text").GetComponent<Text>();
				component2.text = _scrgc.dialoguesMemory[num2].text;
				component2.color = _scrgc.dialoguesMemory[num2].clr;
				component2.GetComponent<UIGradient>().offset = _scrgc.dialoguesMemory[num2].offset;
				component2.GetComponent<UIGradient>().color1 = _scrgc.dialoguesMemory[num2].clr1;
				component2.GetComponent<UIGradient>().color2 = _scrgc.dialoguesMemory[num2].clr2;
				num -= 45;
			}
			Object.Destroy(objectDialogueExample);
		}
		else
		{
			Object.Destroy(rectDialogues.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (musicPause != null)
		{
			Object.Destroy(musicPause.gameObject);
		}
	}

	public void CloseMenu()
	{
		GameObject.FindWithTag("GameController").GetComponent<GameController>().FastMenuClose();
		musicPause.GetComponent<Audio_Volume>().DestroySmooth();
	}

	public void ExitMainMenu()
	{
		GameObject.FindWithTag("GameController").GetComponent<GameController>().ExitGame();
	}
}
