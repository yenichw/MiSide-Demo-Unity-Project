using UnityEngine;

public class Options_Light : MonoBehaviour
{
	public bool lockNone;

	private void Start()
	{
		StartSeetings();
	}

	public void StartSeetings()
	{
		Light component = GetComponent<Light>();
		if (GlobalGame.shadow == 0)
		{
			if (!lockNone)
			{
				component.shadows = LightShadows.None;
			}
			else
			{
				component.shadows = LightShadows.Hard;
			}
		}
		if (GlobalGame.shadow == 1)
		{
			component.shadows = LightShadows.Hard;
		}
		if (GlobalGame.shadow == 2)
		{
			component.shadows = LightShadows.Soft;
		}
	}
}
