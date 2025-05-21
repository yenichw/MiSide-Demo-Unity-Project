using UnityEngine;

public class Light_OffShadow : MonoBehaviour
{
	private void Start()
	{
		Light component = GetComponent<Light>();
		if (component.type == LightType.Directional)
		{
			component.shadows = LightShadows.None;
		}
	}
}
