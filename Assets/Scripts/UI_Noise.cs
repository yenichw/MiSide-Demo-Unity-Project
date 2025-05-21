using UnityEngine;

public class UI_Noise : MonoBehaviour
{
	public float noise;

	private RectTransform rect;

	private Vector2 position;

	private void Start()
	{
		rect = GetComponent<RectTransform>();
		position = rect.anchoredPosition;
	}

	private void Update()
	{
		noise = Mathf.Lerp(noise, 0f, Time.deltaTime * 5f);
		rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, position + new Vector2(Random.Range(0f - noise, noise), Random.Range(0f - noise, noise)), Time.deltaTime * 20f);
	}

	public void Noise(float x)
	{
		noise = x;
	}
}
