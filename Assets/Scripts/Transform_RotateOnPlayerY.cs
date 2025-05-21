using UnityEngine;

public class Transform_RotateOnPlayerY : MonoBehaviour
{
	private Transform player;

	public Transform copyPosition;

	public Transform copyPositionY;

	private void Start()
	{
		player = GlobalTag.player.transform;
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(player.position - base.transform.position), Vector3.up);
		if (copyPosition != null)
		{
			if (copyPositionY == null)
			{
				base.transform.position = copyPosition.position;
			}
			else
			{
				base.transform.position = new Vector3(copyPosition.position.x, copyPositionY.position.y, copyPosition.position.z);
			}
		}
	}
}
