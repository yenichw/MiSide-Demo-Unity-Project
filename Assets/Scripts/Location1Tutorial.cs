using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location1Tutorial : MonoBehaviour
{
	private bool requestPlayer;

	private bool play;

	private float timeCanMove;

	private int indexHint;

	public UnityEvent eventYesQuest;

	[Header("Звуки")]
	public AudioSource audioFill;

	public AudioSource audioOkey;

	[Header("Обучение 1")]
	public GameObject objectMouseMove;

	public Image imageTimeMouse;

	public UnityEvent eventReadyMouse;

	[Header("Обучение 2")]
	public GameObject objectMove;

	public Image imageTimeMove;

	public UnityEvent eventReadyMove;

	private void Start()
	{
		imageTimeMouse.fillAmount = 0f;
		imageTimeMove.fillAmount = 0f;
	}

	private void Update()
	{
		if (!(Time.timeScale > 0f) || !play)
		{
			return;
		}
		if (timeCanMove == 0f)
		{
			if (indexHint == 1 && (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f))
			{
				imageTimeMouse.fillAmount += Time.deltaTime * 0.5f;
				if ((double)audioFill.volume < 0.3)
				{
					audioFill.pitch += Time.deltaTime * 0.4f;
					audioFill.volume += Time.deltaTime * 0.6f;
				}
				else
				{
					audioFill.volume = 0.3f;
				}
				if (imageTimeMouse.fillAmount >= 1f)
				{
					audioFill.pitch = 0.9f;
					eventReadyMouse.Invoke();
					indexHint = 2;
					timeCanMove = 0.5f;
					objectMove.gameObject.SetActive(value: true);
					audioOkey.Play();
				}
			}
			if (indexHint == 2 && (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f))
			{
				imageTimeMove.fillAmount += Time.deltaTime * 0.5f;
				if ((double)audioFill.volume < 0.3)
				{
					audioFill.pitch += Time.deltaTime * 0.4f;
					audioFill.volume += Time.deltaTime * 0.6f;
				}
				else
				{
					audioFill.volume = 0.3f;
				}
				if (imageTimeMove.fillAmount >= 1f)
				{
					audioOkey.Play();
					eventReadyMove.Invoke();
					indexHint = 3;
				}
			}
		}
		else
		{
			timeCanMove -= Time.deltaTime;
			if (timeCanMove < 0f)
			{
				timeCanMove = 0f;
			}
		}
		if (audioFill.volume < 0f)
		{
			audioFill.volume = 0f;
		}
		else
		{
			audioFill.volume -= Time.deltaTime * 0.3f;
		}
	}

	public void ChangePlayerQuest(bool _x)
	{
		requestPlayer = _x;
	}

	public void CloseQuestHint()
	{
		if (!requestPlayer)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		audioFill.pitch = 0.9f;
		indexHint = 1;
		eventYesQuest.Invoke();
		timeCanMove = 0.5f;
		play = true;
		objectMouseMove.gameObject.SetActive(value: true);
	}

	public void DestroyMe()
	{
		Object.Destroy(base.gameObject);
	}
}
