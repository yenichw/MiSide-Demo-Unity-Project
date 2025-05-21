using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(FABRIK))]
public class IK_LimbFabrik : MonoBehaviour
{
	private FABRIK farbik;

	private GameObject objectTargetInertia;

	private void Start()
	{
		farbik = GetComponent<FABRIK>();
		objectTargetInertia = new GameObject();
		objectTargetInertia.transform.parent = base.transform.parent;
		farbik.solver.target = objectTargetInertia.transform;
	}

	private void Update()
	{
		if (farbik.solver.IKPositionWeight > 0f)
		{
			farbik.solver.IKPositionWeight -= Time.deltaTime / 5f;
			if (farbik.solver.IKPositionWeight < 0f)
			{
				farbik.solver.IKPositionWeight = 0f;
			}
		}
	}

	public void Shot(Vector3 _pointShot)
	{
		if ((double)farbik.solver.IKPositionWeight < 0.1)
		{
			farbik.solver.IKPositionWeight += 0.05f;
		}
		objectTargetInertia.transform.position = _pointShot;
	}
}
