using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlackScreen : MonoBehaviour
{
	public float timeOff;

	private Image img;

	private bool On;

	private bool hold;

	private void Awake()
	{
		img = GetComponent<Image>();
		img.color = new Vector4(0f, 0f, 0f, 1f);
	}

	private void Update()
	{
		if (!On)
		{
			if (timeOff == 0f)
			{
				if (!hold)
				{
					if (img.color.a > 0f)
					{
						img.color = new Color(0f, 0f, 0f, img.color.a - Time.unscaledDeltaTime * 3f);
						if (img.color.a < 0f)
						{
							img.color = new Color(0f, 0f, 0f, 0f);
							img.raycastTarget = false;
						}
					}
					AudioListener.volume = Mathf.Lerp(AudioListener.volume, GlobalGame.VolumeGame, Time.unscaledDeltaTime * 3f);
					return;
				}
				if (img.color.a < 1f)
				{
					img.color = new Color(0f, 0f, 0f, img.color.a + Time.unscaledDeltaTime * 3f);
					if (img.color.a > 1f)
					{
						img.color = new Color(0f, 0f, 0f, 1f);
						img.raycastTarget = false;
					}
				}
				AudioListener.volume = Mathf.Lerp(AudioListener.volume, 0f, Time.unscaledDeltaTime * 3f);
			}
			else
			{
				timeOff -= Time.unscaledDeltaTime;
				if (timeOff <= 0f)
				{
					timeOff = 0f;
				}
			}
			return;
		}
		if (img.color.a < 1f)
		{
			img.color = new Color(0f, 0f, 0f, img.color.a + Time.unscaledDeltaTime * 3f);
		}
		if (AudioListener.volume > 0f)
		{
			AudioListener.volume -= Time.unscaledDeltaTime * 3f;
		}
		if (img.color.a >= 1f && AudioListener.volume <= 0f)
		{
			if (GlobalTag.gameController != null)
			{
				GlobalGame.play = false;
				GlobalGame.playWorld = false;
				Object.Destroy(GlobalTag.gameController);
			}
			SceneManager.LoadScene("SceneLoading", LoadSceneMode.Single);
			On = false;
		}
	}

	public void NextLevel(string level)
	{
		GlobalGame.LoadingLevel = level;
		On = true;
		img.raycastTarget = true;
	}

	public void BlackScreenAlphaSound(float x)
	{
		img.color = new Color(0f, 0f, 0f, x);
		if (x > GlobalGame.VolumeGame)
		{
			x = GlobalGame.VolumeGame;
		}
		AudioListener.volume = x;
	}

	public void BlackScreenAlpha(float x)
	{
		img.color = new Color(0f, 0f, 0f, x);
	}

	public void TimeOff(float x)
	{
		timeOff = x;
	}

	public void HoldBlack(bool x, bool sharply)
	{
		img = GetComponent<Image>();
		hold = x;
		if (x)
		{
			img.raycastTarget = true;
		}
		if (sharply)
		{
			if (x)
			{
				img.color = new Color(0f, 0f, 0f, 1f);
				AudioListener.volume = 0f;
			}
			else
			{
				img.color = new Color(0f, 0f, 0f, 0f);
				AudioListener.volume = GlobalGame.VolumeGame;
			}
		}
	}

	public void BlackScreenBlackTime(float _time)
	{
		timeOff = _time;
		img.color = new Color(0f, 0f, 0f, 1f);
		AudioListener.volume = 0f;
	}
}
