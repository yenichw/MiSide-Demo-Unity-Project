using UnityEngine;

public class FlashLightPoint : MonoBehaviour
{
	public bool lightPlayer;

	private void Update()
	{
		if (!lightPlayer)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, Vector3.up * 0.5f, Time.deltaTime * 5f);
		}
		else
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
		}
	}
}
