using UnityEngine;

public class Player_Teleport : MonoBehaviour
{
	private PlayerMove scrpm;

	public Vector3 position;

	public bool positionAdd;

	[Header("Вращение")]
	public bool useRotation;

	public float rotation;

	public bool rotationAdd;

	[Header("Голова")]
	public bool useRotationHead;

	[Range(-89f, 89f)]
	public float rotationHead;

	[ContextMenu("Телепортировать")]
	public void Click()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		Vector3 vector = position;
		if (positionAdd)
		{
			vector = scrpm.transform.position + position;
		}
		float num = rotation;
		if (rotationAdd)
		{
			num = scrpm.transform.eulerAngles.y + rotation;
		}
		if (!useRotation)
		{
			scrpm.TeleportPlayer(vector);
		}
		else if (!useRotationHead)
		{
			scrpm.TeleportPlayer(vector, num);
		}
		else
		{
			scrpm.TeleportPlayer(vector, num, rotationHead);
		}
	}
}
