using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location7_DoorChibi : MonoBehaviour
{
	public Text textNow;

	public GameObject[] chibis;

	public Transform pointDoor;

	public Animator animDoor;

	public UnityEvent eventReady;

	private int chibiCountNow;

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < chibis.Length; i++)
		{
			if (chibis[i] != null && Vector3.Distance(chibis[i].transform.position, pointDoor.position) < 1f)
			{
				flag = true;
			}
		}
		if (animDoor.GetBool("Open") != flag)
		{
			animDoor.SetBool("Open", flag);
		}
	}

	public void ChibiReady()
	{
		textNow.GetComponent<Audio_Data>().RandomPlayPitch();
		chibiCountNow++;
		textNow.text = chibiCountNow.ToString() ?? "";
		if (chibiCountNow == 5)
		{
			eventReady.Invoke();
			animDoor.SetBool("Open", value: false);
			Object.Destroy(this);
		}
	}
}
