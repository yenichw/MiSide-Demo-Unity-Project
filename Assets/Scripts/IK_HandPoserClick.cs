using UnityEngine;

public class IK_HandPoserClick : MonoBehaviour
{
	public PlayerHandIK_Prefab handPose;

	public bool useLimbHand = true;

	[Header("Pivot")]
	public Transform handPivot;

	public bool usePosition = true;

	public Vector3 position;

	public bool useRotate;

	public Vector3 rotation;

	private PlayerPersonIK scrppik;

	private void Start()
	{
		scrppik = GameObject.FindWithTag("Player").transform.Find("Person").GetComponent<PlayerPersonIK>();
	}

	public void Click()
	{
		if (handPivot != null)
		{
			if (usePosition)
			{
				handPivot.position = base.transform.position + base.transform.right * position.x + base.transform.up * position.y + base.transform.forward * position.z;
			}
			if (useRotate)
			{
				handPivot.rotation = base.transform.rotation * Quaternion.Euler(rotation);
			}
		}
		scrppik.HandPoseSharplyApply(handPose, useLimbHand, _hold: false);
	}
}
