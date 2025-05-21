using UnityEngine;

public class ObjectMaterial : MonoBehaviour
{
	public TypeMaterial typeMaterial;

	public float ladder;

	public bool noiseTransformFoot;

	public bool blood;

	private float timeNoise;

	private float rotationAdd;

	private Vector3 transformOrigin;

	private void Start()
	{
		transformOrigin = base.transform.eulerAngles;
	}

	private void Update()
	{
		if (!noiseTransformFoot || timeNoise == 0f)
		{
			return;
		}
		base.transform.rotation = Quaternion.Euler(transformOrigin.x, transformOrigin.y + rotationAdd, transformOrigin.z);
		rotationAdd += timeNoise;
		if (timeNoise > 0f)
		{
			timeNoise -= Time.deltaTime * 20f;
			if (timeNoise < 0f)
			{
				timeNoise = 0f;
			}
		}
		if (timeNoise < 0f)
		{
			timeNoise += Time.deltaTime * 20f;
			if (timeNoise > 0f)
			{
				timeNoise = 0f;
			}
		}
	}

	public void FootStep()
	{
		if (noiseTransformFoot)
		{
			timeNoise = Random.Range(-3f, 3f);
			if (timeNoise < 0f && timeNoise > -1f)
			{
				timeNoise = -3f;
			}
			if (timeNoise > 0f && timeNoise < 1f)
			{
				timeNoise = 3f;
			}
		}
	}
}
