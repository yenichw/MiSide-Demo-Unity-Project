using UnityEngine;

[AddComponentMenu("Functions/Transform/Transform start random")]
public class Transform_StartRandom : MonoBehaviour
{
	public enum RandomMode
	{
		single = 0,
		massive = 1
	}

	public bool locale;

	public RandomMode modeRandom;

	public Vector3 positionS;

	public Vector3 rotationS;

	public bool rotationXS;

	public bool rotationYS;

	public bool rotationZS;

	public Vector3[] position;

	public Vector3[] rotation;

	public bool[] rotationX;

	public bool[] rotationY;

	public bool[] rotationZ;

	public bool useMesh;

	public float sizeSphere = 0.05f;

	private void Start()
	{
		Object.Destroy(this);
	}

	private void OnDrawGizmosSelected()
	{
	}
}
