using UnityEngine;
using UnityEngine.UI;

public class Dialogue_Symbol : MonoBehaviour
{
	public Text shadowText;

	public Material materialJump;

	private bool destroy;

	[HideInInspector]
	public float timeLife;

	private Rigidbody rb;

	private RectTransform rect;

	private Vector2 posOrigin;

	private float scaleOriginal;

	private float noiseStart;

	private float noise;

	private Vector2 noisePos;

	private Dialogue_3DText scrd3;

	private float animationStart;

	private Quaternion rotationStart;

	public void StartComponent(Dialogue_3DText _scrd3, float _timeLife, float _noiseStart, float _noise, float _startRotation)
	{
		scrd3 = _scrd3;
		scaleOriginal = base.transform.localScale.x;
		if (_timeLife == -1f)
		{
			timeLife = -1000f;
		}
		else
		{
			timeLife = _timeLife;
		}
		rb = GetComponent<Rigidbody>();
		rect = GetComponent<RectTransform>();
		posOrigin = rect.anchoredPosition;
		noiseStart = _noiseStart;
		noise = _noise;
		animationStart = 1f;
		base.transform.localScale = Vector3.one * 1.5f * scaleOriginal;
		shadowText.color = new Color(shadowText.color.r, shadowText.color.g, shadowText.color.b, 0f);
		GetComponent<Text>().color = new Color(GetComponent<Text>().color.r, GetComponent<Text>().color.g, GetComponent<Text>().color.b, 0f);
		rotationStart = Quaternion.Euler(new Vector3(0f, 0f, Random.Range(0f - _startRotation, _startRotation)));
		base.transform.localRotation = rotationStart;
		shadowText.text = GetComponent<Text>().text;
		shadowText.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 110f);
	}

	private void Update()
	{
		if (timeLife > 0f || timeLife == -1000f)
		{
			if (noiseStart > 0f)
			{
				noiseStart -= Time.deltaTime * 20f;
				if (noiseStart < 0f)
				{
					noiseStart = 0f;
				}
			}
			if (noise > 0f)
			{
				noisePos = Vector2.Lerp(noisePos, new Vector2(Random.Range(0f - noise, noise), Random.Range(0f - noise, noise)), Time.deltaTime * 10f);
			}
			rect.anchoredPosition = posOrigin + noisePos + new Vector2(Random.Range(0f - noiseStart, noiseStart), Random.Range(0f - noiseStart, noiseStart));
		}
		if (timeLife != -1000f)
		{
			timeLife -= Time.deltaTime;
			if (scrd3.stop)
			{
				if (timeLife <= 0f && !destroy)
				{
					Jump();
				}
				if (timeLife < -1f)
				{
					base.transform.localScale -= Vector3.one * (Time.deltaTime * 0.0075f);
					if (base.transform.localScale.x < 0f)
					{
						scrd3.DestroySymbol(base.gameObject);
					}
				}
			}
		}
		if (animationStart > 0f)
		{
			animationStart -= Time.deltaTime * 3f;
			if (animationStart < 0f)
			{
				animationStart = 0f;
			}
			base.transform.localScale = (Vector3.one + Vector3.one * (animationStart * 0.5f)) * scaleOriginal;
			shadowText.color = new Color(shadowText.color.r, shadowText.color.g, shadowText.color.b, 1f - animationStart);
			GetComponent<Text>().color = new Color(GetComponent<Text>().color.r, GetComponent<Text>().color.g, GetComponent<Text>().color.b, 1f - animationStart);
			base.transform.localRotation = Quaternion.Lerp(rotationStart, Quaternion.Euler(Vector3.zero), 1f - animationStart);
		}
	}

	public void Jump()
	{
		base.gameObject.layer = 6;
		GetComponent<BoxCollider>().isTrigger = false;
		timeLife = 0f;
		if (scrd3 != null)
		{
			scrd3.SymbolJump();
		}
		destroy = true;
		rb.isKinematic = false;
		rb.velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.5f, 3f), Random.Range(-0.5f, 0.5f));
		rb.angularVelocity = new Vector3(Random.Range(90, 90), Random.Range(90, 90), Random.Range(90, 90));
		GetComponent<Text>().material = materialJump;
		shadowText.material = materialJump;
	}
}
