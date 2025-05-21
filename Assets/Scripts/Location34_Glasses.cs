using UnityEngine;
using UnityEngine.Events;

public class Location34_Glasses : MonoBehaviour
{
	public static bool glassesMita;

	public Transform_Magnet glassesMagnet;

	[Header("Локация 4")]
	public GameObject glassesGameCard;

	public bool location4;

	public UnityEvent eventAnimationDropGlass;

	private void Start()
	{
		if (location4 && !glassesMita)
		{
			Object.Destroy(glassesGameCard);
			Object.Destroy(glassesMagnet.gameObject);
			Object.Destroy(this);
		}
	}

	public void TakeGlasses()
	{
		glassesMita = true;
		glassesMagnet.position = new Vector3(0f, 0.08f, 0.125f);
		glassesMagnet.rotation = new Vector3(-87.94f, 0f, 0f);
	}

	public void TakeDropGlasses()
	{
		glassesMita = false;
		glassesMagnet.position = Vector3.zero;
		glassesMagnet.rotation = Vector3.zero;
	}

	public void StartDropGlasses()
	{
		eventAnimationDropGlass.Invoke();
	}
}
