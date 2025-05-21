using UnityEngine;
using UnityEngine.UI;

public class UI_TextResultAnimation : MonoBehaviour
{
	public bool play;

	public bool noScore;

	[Header("Text")]
	public string textNeed;

	public int score;

	public string textAdd;

	public int scoreAddone;

	public string textAddAddone;

	private Text text;

	private int textScore;

	private int textScoreAddone;

	private bool scoreAnimation;

	private void Start()
	{
		text = GetComponent<Text>();
		text.text = "";
	}

	private void Update()
	{
		if (!play)
		{
			return;
		}
		if (!scoreAnimation)
		{
			text.text += textNeed[text.text.Length];
			if (text.text.Length == textNeed.Length)
			{
				scoreAnimation = true;
				if (noScore)
				{
					play = false;
					text.text = textNeed;
				}
			}
			return;
		}
		textScore = Mathf.CeilToInt(Mathf.Lerp(textScore, score, Time.deltaTime * 3f));
		if (textScore - 1 > score)
		{
			textScore = score;
		}
		if (textAddAddone != null && textAddAddone != "")
		{
			textScoreAddone = Mathf.CeilToInt(Mathf.Lerp(textScoreAddone, scoreAddone, Time.deltaTime * 3f));
			if (textScoreAddone - 1 > scoreAddone)
			{
				textScoreAddone = scoreAddone;
			}
			text.text = textNeed + ": " + textScore + textAdd + " " + textScoreAddone + textAddAddone;
		}
		else
		{
			text.text = textNeed + ": " + textScore + textAdd;
		}
		if (textScoreAddone == scoreAddone && textScore == score)
		{
			play = false;
		}
	}

	public void Play()
	{
		play = true;
	}
}
