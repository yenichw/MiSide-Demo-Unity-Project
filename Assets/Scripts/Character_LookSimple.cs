using RootMotion.FinalIK;
using UnityEngine;

public class Character_LookSimple : MonoBehaviour
{
	public bool active = true;

	public LookAtIK lookAtIK;

	public Transform targetLook;

	[Header("Items")]
	public Transform objectFixRightItem;

	public Transform boneRightItem;

	[Space(5f)]
	public Transform objectFixLeftItem;

	public Transform boneLeftItem;

	[Header("Start")]
	public bool startTargetLookCamera;

	private float weightIK;

	private void Start()
	{
		if (startTargetLookCamera)
		{
			TargetLookCamera();
		}
	}

	private void Update()
	{
		if (active && (bool)targetLook)
		{
			if (weightIK < 1f)
			{
				weightIK += Time.deltaTime * 3f;
				if (weightIK > 1f)
				{
					weightIK = 1f;
				}
				lookAtIK.solver.SetLookAtWeight(weightIK);
			}
		}
		else if (weightIK > 0f)
		{
			weightIK -= Time.deltaTime * 3f;
			if (weightIK < 0f)
			{
				weightIK = 0f;
			}
			lookAtIK.solver.SetLookAtWeight(weightIK);
		}
		if (targetLook != null)
		{
			lookAtIK.solver.target.position = Vector3.Lerp(lookAtIK.solver.target.position, targetLook.position, Time.deltaTime * 5f);
		}
	}

	private void LateUpdate()
	{
		if (objectFixRightItem != null)
		{
			objectFixRightItem.SetPositionAndRotation(boneRightItem.position, boneRightItem.rotation);
		}
		if (objectFixLeftItem != null)
		{
			objectFixLeftItem.SetPositionAndRotation(boneLeftItem.position, boneLeftItem.rotation);
		}
	}

	public void Activation(bool x)
	{
		active = x;
	}

	public void TargetLook(Transform _target)
	{
		targetLook = _target;
	}

	public void TargetLookCamera()
	{
		targetLook = GlobalTag.cameraPlayer.transform;
	}
}
