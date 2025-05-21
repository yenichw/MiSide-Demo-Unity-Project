using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputKeyboard_Main : MonoBehaviour
{
	public UnityEvent eventEnter;

	public char[] charsEng;

	public char[] charsRus;

	public GameObject[] keys;

	public InputField inpField;

	public int maxCount;

	public Image imgInput;

	[Header("Пароль")]
	public bool needPassword;

	public string password;

	[Header("Звуки")]
	public AudioClip[] soundsTaps;

	public AudioSource audioSource;

	private char[] changeChars;

	private bool upper;

	private void Start()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}
		GameObject gameObject = keys[0];
		keys[0] = null;
		changeChars = charsEng;
		if (!needPassword && GlobalGame.Language == "Russian")
		{
			changeChars = charsRus;
		}
		int num = 5;
		int num2 = -65;
		for (int i = 0; i < changeChars.Length; i++)
		{
			keys[i] = Object.Instantiate(gameObject, gameObject.transform.parent);
			keys[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(num, num2);
			keys[i].GetComponent<InputKeyboard_Key>().ReText(changeChars[i].ToString());
			num += 45;
			if (num > 455)
			{
				num = 5;
				num2 -= 45;
			}
		}
		if (!needPassword)
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 113 + -(65 + num2));
		}
		Object.Destroy(gameObject);
	}

	private void Update()
	{
		if (imgInput.color != Color.white)
		{
			imgInput.color = Color.Lerp(imgInput.color, Color.white, Time.deltaTime * 5f);
		}
	}

	public void ButtonUpper()
	{
		upper = !upper;
		for (int i = 0; i < changeChars.Length; i++)
		{
			if (upper)
			{
				keys[i].GetComponent<InputKeyboard_Key>().ReText(changeChars[i].ToString().ToUpper());
			}
			else
			{
				keys[i].GetComponent<InputKeyboard_Key>().ReText(changeChars[i].ToString());
			}
		}
		AudioPlayTap();
	}

	public void ButtonEnter()
	{
		if (!needPassword)
		{
			if (inpField.text.Length >= 3)
			{
				eventEnter.Invoke();
			}
			else
			{
				imgInput.color = new Color(1f, 0.6f, 0f);
				if (inpField.text.Length == 0)
				{
					inpField.text += "AAA";
				}
				if (inpField.text.Length == 1)
				{
					inpField.text += "AA";
				}
				if (inpField.text.Length == 2)
				{
					inpField.text += "A";
				}
			}
		}
		else if (inpField.text == password)
		{
			eventEnter.Invoke();
		}
		AudioPlayTap();
	}

	public void AddKey(string _key)
	{
		if (inpField.text.Length < maxCount)
		{
			inpField.text += _key;
			AudioPlayTap();
		}
	}

	public void RemoveKey()
	{
		if (inpField.text.Length > 0)
		{
			inpField.text = inpField.text.Remove(inpField.text.Length - 1, 1);
			AudioPlayTap();
		}
	}

	public void AudioPlayTap()
	{
		audioSource.clip = soundsTaps[Random.Range(0, soundsTaps.Length)];
		audioSource.pitch = Random.Range(0.95f, 1.05f);
		audioSource.Play();
	}
}
