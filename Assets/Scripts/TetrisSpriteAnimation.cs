using UnityEngine;
using UnityEngine.UI;

public class TetrisSpriteAnimation : MonoBehaviour
{
	private int indexSprite;

	private float timeAnimation;

	private Image img;

	[Header("Анимация")]
	public Sprite[] sprites;

	public bool pause;

	private void Start()
	{
		img = GetComponent<Image>();
		AnimationStop();
		pause = false;
	}

	private void Update()
	{
		if (pause)
		{
			return;
		}
		timeAnimation += Time.deltaTime;
		if (timeAnimation > 0.2f)
		{
			timeAnimation = 0f;
			indexSprite++;
			if (indexSprite > sprites.Length - 1)
			{
				indexSprite = 0;
			}
			img.sprite = sprites[indexSprite];
		}
	}

	public void AnimationStop()
	{
		pause = true;
		indexSprite = 0;
		timeAnimation = 0f;
		img.sprite = sprites[0];
	}

	public void AnimationPause()
	{
		pause = true;
	}

	public void AnimationPlay()
	{
		pause = false;
	}
}
