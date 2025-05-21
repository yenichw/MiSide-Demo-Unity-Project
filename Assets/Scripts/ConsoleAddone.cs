using UnityEngine;
using UnityEngine.UI;

public class ConsoleAddone : MonoBehaviour
{
	public ConsoleAddoneValue[] addone;

	private ConsoleInterface scrci;

	private void Start()
	{
		scrci = base.transform.parent.GetComponent<ConsoleInterface>();
		if (addone != null && addone.Length != 0)
		{
			int num = 5;
			Transform parent = base.transform.parent.Find("MainPanel/Addons");
			GameObject gameObject = base.transform.parent.Find("MainPanel/Addons/Button").gameObject;
			for (int i = 0; i < addone.Length; i++)
			{
				if (addone[i].objectCreatStart != null)
				{
					Object.Instantiate(addone[i].objectCreatStart, base.transform.parent);
				}
				if (addone[i].objectCreateWindow != null)
				{
					GameObject gameObject2 = Object.Instantiate(addone[i].objectCreateWindow, base.transform.parent.Find("MainPanel"));
					gameObject2.SetActive(value: false);
					addone[i].objectWindowNow = gameObject2;
					GameObject obj = Object.Instantiate(gameObject, parent);
					obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num);
					obj.GetComponent<ConsoleAddoneTable>().index = i;
					obj.GetComponent<ConsoleAddoneTable>().scrca = this;
					obj.transform.Find("Text").GetComponent<Text>().text = addone[i].nameWindow;
					num += 35;
				}
			}
			Object.Destroy(gameObject);
			base.transform.parent.Find("MainPanel/Addons").GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 30 + num);
		}
		else
		{
			Object.Destroy(base.transform.parent.Find("MainPanel/Addons").gameObject);
			Object.Destroy(base.transform.parent.Find("MainPanel/UpPanel/ButtonAddons").gameObject);
		}
	}

	public void ClickAddone(int index)
	{
		scrci.AddoneWindowCloseAll();
		addone[index].objectWindowNow.SetActive(value: true);
		base.transform.parent.Find("MainPanel/UpPanel/ButtonAddons/Template").gameObject.SetActive(value: false);
	}

	public void ClickDropAddone()
	{
		base.transform.parent.Find("MainPanel/UpPanel/ButtonAddons/Template").gameObject.SetActive(!base.transform.parent.Find("MainPanel/UpPanel/ButtonAddons/Template").gameObject.activeInHierarchy);
	}
}
