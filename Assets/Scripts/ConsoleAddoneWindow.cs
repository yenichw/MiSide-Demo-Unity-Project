// based on the original game.Yen Chezky(yenichw)
using UnityEngine;
using UnityEngine.Events;

public class ConsoleAddoneWindow : MonoBehaviour
{
	public UnityEvent eventClose;

	public void CloseWindow()
	{
		base.gameObject.SetActive(value: false);
		eventClose.Invoke();
	}
}
