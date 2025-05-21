using UnityEngine;
using UnityEngine.Events;

public class LightMitaTamagothci : MonoBehaviour
{
	public UnityEvent eventReset;

	private void Start()
	{
		GlobalTag.gameOptions.GetComponent<OptionsGame>().AddResetSetting(eventReset, base.gameObject);
		ResetSetting();
	}

	public void ResetSetting()
	{
		Light component = GetComponent<Light>();
		if (GlobalGame.shadow == 3)
		{
			component.shadows = LightShadows.Soft;
			component.renderMode = LightRenderMode.ForcePixel;
		}
		if (GlobalGame.shadow == 2)
		{
			component.shadows = LightShadows.Hard;
			component.renderMode = LightRenderMode.ForcePixel;
		}
		if (GlobalGame.shadow == 1)
		{
			component.shadows = LightShadows.None;
			component.renderMode = LightRenderMode.ForcePixel;
		}
		if (GlobalGame.shadow == 0)
		{
			component.shadows = LightShadows.None;
			component.renderMode = LightRenderMode.ForceVertex;
		}
	}
}
