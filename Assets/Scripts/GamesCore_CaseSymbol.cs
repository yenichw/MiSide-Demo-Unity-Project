using UnityEngine;
using UnityEngine.UI;

public class GamesCore_CaseSymbol : MonoBehaviour
{
	[HideInInspector]
	public int indexSymbol;

	public string[] dataSymbols;

	public Text txt;

	private float timeAnimation;

	[Header("Animation")]
	public AnimationCurve animationScaleSymbol;

	private void Start()
	{
		if (dataSymbols.Length != 0)
		{
			indexSymbol = Random.Range(0, dataSymbols.Length);
			UpdateText();
		}
	}

	private void Update()
	{
		if (timeAnimation < 1f && txt != null)
		{
			timeAnimation += Time.deltaTime * 4f;
			if (timeAnimation > 1f)
			{
				timeAnimation = 1f;
			}
			txt.GetComponent<RectTransform>().localScale = Vector3.one + Vector3.one * animationScaleSymbol.Evaluate(timeAnimation);
		}
	}

	public void SymbolUp()
	{
		indexSymbol++;
		if (indexSymbol > dataSymbols.Length - 1)
		{
			indexSymbol = 0;
		}
		UpdateText();
	}

	public void SymbolDown()
	{
		indexSymbol--;
		if (indexSymbol < 0)
		{
			indexSymbol = dataSymbols.Length - 1;
		}
		UpdateText();
	}

	private void UpdateText()
	{
		timeAnimation = 0f;
		txt.text = dataSymbols[indexSymbol];
	}
}
