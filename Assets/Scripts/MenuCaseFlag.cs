using UnityEngine;

public class MenuCaseFlag : MonoBehaviour
{
	public int index;

	public SceneLoading main;

	public void Click()
	{
		base.transform.parent.Find("CaseChange").GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(-6f, 6f);
		main.ClickFlag(index);
	}
}
