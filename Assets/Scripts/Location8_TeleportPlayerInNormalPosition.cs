using UnityEngine;

public class Location8_TeleportPlayerInNormalPosition : MonoBehaviour
{
	public Transform localTeleportGo;

	public Transform localTeleport;

	public void Click()
	{
		localTeleport.position = GlobalTag.player.transform.position;
		localTeleportGo.localPosition = localTeleport.localPosition;
		GlobalTag.player.transform.SetPositionAndRotation(localTeleportGo.position, Quaternion.Euler(0f, 180f, 0f));
		Object.Destroy(base.gameObject);
	}
}
