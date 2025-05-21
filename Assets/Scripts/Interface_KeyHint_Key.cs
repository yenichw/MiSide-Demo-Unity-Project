using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interface_KeyHint_Key : MonoBehaviour
{
	public string nameHint;

	public Text textDescription;

	public Text textKey;

	public Image caseKey;

	public Image frameAddon;

	public RectTransform edgeFrame;

	public RectTransform edgeFrameF;

	[Header("General")]
	public string nameKey = "E";

	public string nameFile;

	public int indexString;

	public bool hide;

	private Transform cameraT;

	[Header("3D")]
	public bool button3D;

	public bool lookOnCamera;

	public float size = 1f;

	public float distanceHide;

	[Space(20f)]
	[Header("Control")]
	public bool keyDown;

	public bool destroyAfter = true;

	public bool downHide;

	public bool downBlind;

	public UnityEvent eventKeyDown;

	private PlayerMove scrpm;

	private bool destroyActive;

	private void Start()
	{
		StartComponent(null, nameKey, nameFile, indexString);
		textDescription.color = new Color(1f, 1f, 1f, 0f);
		textKey.color = new Color(1f, 1f, 1f, 0f);
		caseKey.color = new Color(1f, 1f, 1f, 0f);
		frameAddon.color = new Color(1f, 1f, 1f, 0f);
		if (edgeFrameF != null)
		{
			edgeFrameF.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
			edgeFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
		}
		if (button3D)
		{
			cameraT = GlobalTag.cameraPlayer.transform;
		}
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
	}

	private void Update()
	{
		if (button3D)
		{
			base.transform.localScale = Vector3.one * (size / 800f * Vector3.Distance(base.transform.position, cameraT.position));
			if (lookOnCamera)
			{
				base.transform.rotation = Quaternion.LookRotation(base.transform.position - cameraT.position);
			}
			if (distanceHide > 0f)
			{
				if (Vector3.Distance(base.transform.position, cameraT.position) > distanceHide)
				{
					hide = true;
				}
				else
				{
					hide = false;
				}
			}
		}
		if (!hide && !destroyActive)
		{
			caseKey.color = Color.Lerp(caseKey.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 10f);
			textKey.color = Color.Lerp(textKey.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 10f);
			frameAddon.color = Color.Lerp(frameAddon.color, new Color(1f, 1f, 1f, 0.5f), Time.deltaTime * 10f);
			textDescription.color = Color.Lerp(textDescription.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 10f);
			if (keyDown && scrpm.timeAnimationAfter == 0f && Input.GetKeyDown(nameKey.ToLower()))
			{
				eventKeyDown.Invoke();
				if (destroyAfter)
				{
					BlindAndDestroy();
				}
				if (downHide)
				{
					Hide(x: true);
				}
				if (downBlind)
				{
					Blind();
				}
				if (GetComponent<AudioSource>() != null)
				{
					GetComponent<AudioSource>().Play();
				}
			}
			if (edgeFrame != null)
			{
				edgeFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, edgeFrame.GetComponent<Image>().color.a - Time.deltaTime);
				if (edgeFrame.GetComponent<Image>().color.a <= 1f)
				{
					edgeFrame.sizeDelta += Vector2.one * 10f * Time.deltaTime;
				}
				if (edgeFrame.GetComponent<Image>().color.a <= 0f)
				{
					edgeFrame.sizeDelta = new Vector2(190f, 40f);
					edgeFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, 2f);
				}
			}
			if (edgeFrameF != null && edgeFrameF.GetComponent<Image>().color.a < 1f)
			{
				edgeFrameF.GetComponent<Image>().color = new Color(1f, 1f, 1f, edgeFrameF.GetComponent<Image>().color.a + Time.deltaTime);
				if (edgeFrameF.GetComponent<Image>().color.a > 1f)
				{
					edgeFrameF.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
				}
			}
			return;
		}
		caseKey.color = Color.Lerp(caseKey.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 10f);
		textKey.color = Color.Lerp(textKey.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 10f);
		frameAddon.color = Color.Lerp(frameAddon.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 10f);
		textDescription.color = Color.Lerp(textDescription.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 10f);
		if (destroyActive && (double)textDescription.color.a < 0.001 && !hide)
		{
			Object.Destroy(base.gameObject);
		}
		if (edgeFrame != null && edgeFrame.GetComponent<Image>().color.a > 0f)
		{
			edgeFrame.sizeDelta += Vector2.one * 10f * Time.deltaTime;
			edgeFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, edgeFrame.GetComponent<Image>().color.a - Time.deltaTime);
		}
		if (edgeFrameF != null && edgeFrameF.GetComponent<Image>().color.a > 0f)
		{
			edgeFrameF.GetComponent<Image>().color = new Color(1f, 1f, 1f, edgeFrameF.GetComponent<Image>().color.a - Time.deltaTime);
			if (edgeFrameF.GetComponent<Image>().color.a < 0f)
			{
				edgeFrameF.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
			}
		}
	}

	public void StartComponent(string _name, string _nameKey, string _nameFile, int _stringIndex)
	{
		if (_name != null)
		{
			nameHint = _name;
		}
		textKey.text = _nameKey.ToUpper();
		textDescription.text = GlobalLanguage.GetString(_nameFile, _stringIndex).ToUpper();
	}

	public void Blind()
	{
		caseKey.color = new Color(1f, 1f, 1f, 1f);
		textKey.color = new Color(1f, 1f, 1f, 1f);
	}

	public void SmoothDestroy()
	{
		destroyActive = true;
		hide = true;
	}

	public void BlindAndDestroy()
	{
		caseKey.color = new Color(1f, 1f, 1f, 1f);
		textKey.color = new Color(1f, 1f, 1f, 1f);
		frameAddon.color = new Color(1f, 1f, 1f, 1f);
		textDescription.color = new Color(1f, 1f, 1f, 1f);
		destroyActive = true;
	}

	public void Hide(bool x)
	{
		hide = x;
	}
}
