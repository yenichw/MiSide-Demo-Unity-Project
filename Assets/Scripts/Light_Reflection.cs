using UnityEngine;

public class Light_Reflection : MonoBehaviour
{
	public Light lightJointBake;

	public Light lightJoint;

	private ReflectionProbe refprobe;

	private void Start()
	{
		refprobe = GetComponent<ReflectionProbe>();
	}

	private void Update()
	{
		if (lightJointBake != null && lightJointBake.gameObject.activeInHierarchy)
		{
			if (!refprobe.enabled)
			{
				refprobe.enabled = true;
			}
		}
		else if (lightJoint.gameObject.activeInHierarchy && lightJoint.enabled)
		{
			refprobe.intensity = Mathf.Clamp(lightJoint.intensity, 0f, 1f);
			if (refprobe.intensity == 0f)
			{
				if (refprobe.enabled)
				{
					refprobe.enabled = false;
				}
			}
			else if (!refprobe.enabled)
			{
				refprobe.enabled = true;
			}
		}
		else if (refprobe.enabled)
		{
			refprobe.enabled = false;
		}
	}
}
