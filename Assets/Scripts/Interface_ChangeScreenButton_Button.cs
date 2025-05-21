using UnityEngine;

public class Interface_ChangeScreenButton_Button : MonoBehaviour
{
	public Interface_ChangeScreenButton scricsb;

	public int index;

	public void Click()
	{
		scricsb.ClickTable(index);
	}
}
