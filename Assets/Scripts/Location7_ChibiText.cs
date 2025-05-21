using UnityEngine;
using UnityEngine.UI;

public class Location7_ChibiText : MonoBehaviour
{
	public Text textA;

	public Text textB;

	public AnimationCurve anim;

	private float timeAnim;

	private void OnEnable()
	{
		string @string = GlobalLanguage.GetString("LocationDialogue Location7", Random.Range(125, 130));
		textA.text = @string;
		textB.text = @string;
		base.transform.localScale = Vector3.one * anim.Evaluate(0f);
	}

	private void Update()
	{
		base.transform.localScale = Vector3.one * anim.Evaluate(timeAnim) * 0.002f;
		timeAnim += Time.deltaTime * 0.75f;
		if (timeAnim > 1f)
		{
			timeAnim = 1f;
			Object.Destroy(base.gameObject);
		}
		base.transform.position += base.transform.forward * (Time.deltaTime * 0.1f);
	}
}
