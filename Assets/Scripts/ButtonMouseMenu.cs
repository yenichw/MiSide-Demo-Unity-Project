using UnityEngine;

public class ButtonMouseMenu : MonoBehaviour
{
	public GameObject startCase;

	public bool keyMove;

	private bool sf;

	[Header("Audio")]
	public AudioSource audioEnter;

	public AudioSource audioClick;

	public AudioSource audioToggle;

	[Space(20f)]
	public GameObject caseChangeNow;

	private void Awake()
	{
		if (audioEnter != null)
		{
			audioEnter.volume = 0f;
		}
		if (audioClick != null)
		{
			audioClick.volume = 0f;
		}
		if (audioToggle != null)
		{
			audioToggle.volume = 0f;
		}
	}

	private void Start()
	{
		ChangeCase(startCase);
	}

	public void ChangeCase(GameObject _case)
	{
		if (caseChangeNow != _case)
		{
			if (caseChangeNow != null)
			{
				caseChangeNow.GetComponent<ButtonMouseClick>().PointerExit();
			}
			caseChangeNow = _case;
			caseChangeNow.GetComponent<ButtonMouseClick>().PointerEnter();
		}
	}

	public void EnterButton()
	{
		if (audioEnter != null)
		{
			audioEnter.pitch = Random.Range(0.95f, 1.05f);
			audioEnter.Play();
			IgnoryFirstSound();
		}
	}

	public void ClickButton()
	{
		if (audioClick != null)
		{
			audioClick.pitch = Random.Range(0.95f, 1.05f);
			audioClick.Play();
			IgnoryFirstSound();
		}
	}

	public void ClickButtonToggle()
	{
		if (audioToggle != null)
		{
			audioToggle.pitch = Random.Range(0.95f, 1.05f);
			audioToggle.Play();
			IgnoryFirstSound();
		}
	}

	public void ActiveKeyMove(bool x)
	{
		keyMove = x;
	}

	private void IgnoryFirstSound()
	{
		if (sf)
		{
			if (audioEnter != null)
			{
				audioEnter.volume = 0.5f;
			}
			if (audioClick != null)
			{
				audioClick.volume = 0.5f;
			}
			if (audioToggle != null)
			{
				audioToggle.volume = 0.5f;
			}
		}
		sf = true;
	}
}
