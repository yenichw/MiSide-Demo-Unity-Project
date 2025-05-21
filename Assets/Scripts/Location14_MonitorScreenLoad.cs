using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location14_MonitorScreenLoad : MonoBehaviour
{
	public Image screenImage;

	public Sprite spritePlayerOne;

	public Sprite spritePlayerMita;

	public Sprite spritePlayerSit;

	public Sprite spritePlayerSpeakMita;

	public Camera cameraRender;

	private int indexLoad;

	private float timeLoad;

	private float timeScreen;

	[Header("Заготовка")]
	public UnityEvent eventPlayerOne;

	public UnityEvent eventPlayerMita;

	public UnityEvent eventPlayerSit;

	public UnityEvent eventPlayerSpeakMita;

	public UnityEvent eventLoadReady;

	private void Update()
	{
		if (timeScreen == 0f)
		{
			timeLoad += Time.deltaTime;
			if ((double)timeLoad > 0.3)
			{
				timeLoad = 0f;
				indexLoad++;
				if (indexLoad == 1)
				{
					eventPlayerOne.Invoke();
					timeScreen = 0.1f;
				}
				if (indexLoad == 2)
				{
					eventPlayerMita.Invoke();
					timeScreen = 0.1f;
				}
				if (indexLoad == 3)
				{
					eventPlayerSit.Invoke();
					timeScreen = 0.1f;
				}
				if (indexLoad == 4)
				{
					eventPlayerSpeakMita.Invoke();
					timeScreen = 0.1f;
				}
				if (indexLoad == 5)
				{
					eventLoadReady.Invoke();
					base.enabled = false;
				}
			}
		}
		if (timeScreen > 0f)
		{
			timeScreen -= Time.deltaTime;
			if (timeScreen <= 0f)
			{
				timeScreen = -1f;
				StartCoroutine(Screenload());
			}
		}
	}

	private IEnumerator Screenload()
	{
		yield return new WaitForEndOfFrame();
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = cameraRender.targetTexture;
		cameraRender.Render();
		Texture2D texture2D = new Texture2D(cameraRender.targetTexture.width, cameraRender.targetTexture.height);
		texture2D.ReadPixels(new Rect(0f, 0f, cameraRender.targetTexture.width, cameraRender.targetTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		timeScreen = 0f;
		if (indexLoad == 1)
		{
			spritePlayerOne = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
		if (indexLoad == 2)
		{
			spritePlayerMita = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
		if (indexLoad == 3)
		{
			spritePlayerSit = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
		if (indexLoad == 4)
		{
			spritePlayerSpeakMita = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
	}

	public void LoadScreenPlayerOne()
	{
		screenImage.gameObject.SetActive(value: true);
		screenImage.sprite = spritePlayerOne;
	}

	public void LoadScreenPlayerMita()
	{
		screenImage.gameObject.SetActive(value: true);
		screenImage.sprite = spritePlayerMita;
	}

	public void LoadScreenPlayerSpeakMita()
	{
		screenImage.gameObject.SetActive(value: true);
		screenImage.sprite = spritePlayerMita;
	}

	public void LoadScreenPlayerSit()
	{
		screenImage.gameObject.SetActive(value: true);
		screenImage.sprite = spritePlayerSit;
	}

	public void HideScreen()
	{
		screenImage.gameObject.SetActive(value: false);
	}
}
