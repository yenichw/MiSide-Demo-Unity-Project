using UnityEngine;

public class IK_HandTrigger : MonoBehaviour
{
	[Header("Правая рука")]
	public Transform handPivotRight;

	public PlayerHandIK_Prefab handPoseRight;

	public bool handToWallRight;

	public float distanceHandToWallRight = 0.5f;

	[Space(10f)]
	public bool handFaceRight;

	public Vector3 handFacePositionRight;

	[Header("Левая рука")]
	public Transform handPivotLeft;

	public PlayerHandIK_Prefab handPoseLeft;

	public bool handToWallLeft;

	public float distanceHandToWallLeft = 0.5f;

	[Space(10f)]
	public bool handFaceLeft;

	public Vector3 handFacePositionLeft;

	[Space(30f)]
	[Header("Общее")]
	public bool active = true;

	public bool rotateOnPlayer;

	public Vector3 rotateOnPlayerAdd;

	private PlayerMove scrpm;

	private void Start()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
	}

	private void Update()
	{
		if (scrpm.objectCastHandInteractive != null && (scrpm.objectCastHandInteractive == base.gameObject || (scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>() != null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>().main == this)))
		{
			Quaternion b = Quaternion.LookRotation(base.transform.position - scrpm.transform.position, Vector3.up) * Quaternion.Euler(rotateOnPlayerAdd);
			if (handPivotRight != null)
			{
				handPivotRight.rotation = Quaternion.Lerp(handPivotRight.rotation, b, Time.deltaTime * 10f);
			}
			if (handPivotLeft != null)
			{
				handPivotLeft.rotation = Quaternion.Lerp(handPivotLeft.rotation, b, Time.deltaTime * 10f);
			}
		}
	}

	public void Activation(bool _x)
	{
		active = _x;
	}
}
