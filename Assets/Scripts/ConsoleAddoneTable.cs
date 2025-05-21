using UnityEngine;

public class ConsoleAddoneTable : MonoBehaviour
{
	[HideInInspector]
	public int index;

	[HideInInspector]
	public ConsoleAddone scrca;

	public void Click()
	{
		scrca.ClickAddone(index);
	}
}
