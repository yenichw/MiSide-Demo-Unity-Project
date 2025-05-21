using UnityEngine;

[AddComponentMenu("Functions/Transform/Lerp Scale")]
public class Transform_LerpScale : MonoBehaviour
{
	[Range(0f, 1f)]
	public float speed = 0.1f;

	private Vector3 scale;

	private void Start()
	{
		scale = base.transform.localScale;
		base.transform.localScale = Vector3.zero;
	}

	private void Update()
	{
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, scale, speed);
		if (Vector3.Distance(base.transform.localScale, scale) < 0.01f)
		{
			Object.Destroy(this);
		}
	}
}
