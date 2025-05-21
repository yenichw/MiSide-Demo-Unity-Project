using UnityEngine;

public class ConsoleCameraFly : MonoBehaviour
{
	private float speed;

	public float xRot;

	public float yRot;

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.A))
		{
			zero += new Vector3(-1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero += new Vector3(1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.W))
		{
			zero += new Vector3(0f, 0f, 1f);
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero += new Vector3(0f, 0f, -1f);
		}
		if (Input.GetKey(KeyCode.Q))
		{
			zero += new Vector3(0f, 0f, -1f);
		}
		if (Input.GetKey(KeyCode.E))
		{
			zero += new Vector3(0f, 0f, 1f);
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			speed += Time.deltaTime * 5f;
		}
		else
		{
			speed = 1f;
		}
		zero = zero * Time.deltaTime * speed;
		base.transform.position += base.transform.forward * zero.z + base.transform.up * zero.y + base.transform.right * zero.x;
		base.transform.rotation = Quaternion.Euler(0f - yRot, xRot, 0f);
		xRot += Input.GetAxis("Mouse X") / 10f;
		yRot += Input.GetAxis("Mouse Y") / 10f;
		yRot = Mathf.Clamp(yRot, -90f, 90f);
	}
}
