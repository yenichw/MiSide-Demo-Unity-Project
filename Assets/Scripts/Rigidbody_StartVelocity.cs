using UnityEngine;

public class Rigidbody_StartVelocity : MonoBehaviour
{
	public bool startVelocity = true;

	public Vector3 velocitySpeed;

	public Vector3 velocitySpeedRandom;

	public Vector3 velocityAngular;

	public Vector3 velocityAngularRandom;

	private void Start()
	{
		if (startVelocity)
		{
			Impulse();
		}
	}

	public void Impulse()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		Vector3 vector = velocitySpeed;
		Vector3 angularVelocity = velocityAngular;
		if (velocitySpeedRandom != Vector3.zero)
		{
			vector += new Vector3(Random.Range(0f - velocitySpeedRandom.x, velocitySpeedRandom.x), Random.Range(0f - velocitySpeedRandom.y, velocitySpeedRandom.y), Random.Range(0f - velocitySpeedRandom.z, velocitySpeedRandom.z));
		}
		if (velocityAngularRandom != Vector3.zero)
		{
			angularVelocity += new Vector3(Random.Range(0f - velocityAngularRandom.x, velocityAngularRandom.x), Random.Range(0f - velocityAngularRandom.y, velocityAngularRandom.y), Random.Range(0f - velocityAngularRandom.z, velocityAngularRandom.z));
		}
		component.velocity = base.transform.forward * vector.z + base.transform.right * vector.x + base.transform.up * vector.y;
		component.angularVelocity = angularVelocity;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + (base.transform.forward * velocitySpeed.z + base.transform.right * velocitySpeed.x + base.transform.up * velocitySpeed.y / 5f));
	}
}
