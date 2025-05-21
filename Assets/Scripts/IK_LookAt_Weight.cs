using RootMotion.FinalIK;
using UnityEngine;

public class IK_LookAt_Weight : MonoBehaviour
{
	public float speedLerp = 5f;

	public LookAtIK lookAt;

	private float weightNeed;

	private void Start()
	{
		weightNeed = lookAt.solver.IKPositionWeight;
	}

	private void Update()
	{
		lookAt.solver.IKPositionWeight = Mathf.Lerp(lookAt.solver.IKPositionWeight, weightNeed, Time.deltaTime * speedLerp);
	}

	public void WeightLerp(float x)
	{
		weightNeed = x;
	}
}
