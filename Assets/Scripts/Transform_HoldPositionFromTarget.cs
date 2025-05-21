using UnityEngine;

public class Transform_HoldPositionFromTarget : MonoBehaviour
{
	public Transform Target;

	public bool xHold;

	public float xPosition;

	public bool yHold;

	public float yPosition;

	public bool zHold;

	public float zPosition;

	private void OnEnable()
	{
		if (Target == null)
		{
			Target = GameObject.FindWithTag("Player").gameObject.transform;
		}
		Move();
	}

	private void Update()
	{
		Move();
	}

	private void Move()
	{
		if (xHold)
		{
			base.transform.position = new Vector3(Target.position.x + xPosition, base.transform.position.y, base.transform.position.z);
		}
		if (yHold)
		{
			base.transform.position = new Vector3(base.transform.position.x, Target.position.y + yPosition, base.transform.position.z);
		}
		if (zHold)
		{
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, Target.position.z + zPosition);
		}
	}
}
