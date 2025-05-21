using UnityEngine;

public class AnitaStatLife : MonoBehaviour
{
	private float timeRotate;

	private float rotationOrigin;

	private void Start()
	{
		rotationOrigin = base.transform.rotation.eulerAngles.y;
	}

	private void Update()
	{
		timeRotate += Time.deltaTime;
		if (timeRotate > 3f)
		{
			timeRotate = 0f;
			base.transform.rotation = Quaternion.Euler(-90f, GlobalAM.RotationFloor(base.transform.position, GlobalTag.player.transform.position), 0f);
		}
	}

	private void OnDisable()
	{
		base.transform.rotation = Quaternion.Euler(-90f, rotationOrigin, 0f);
	}
}
