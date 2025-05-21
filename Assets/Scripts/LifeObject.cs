using UnityEngine;
using UnityEngine.Events;

public class LifeObject : MonoBehaviour
{
	public int healthPoint;

	[Header("Смерть")]
	public UnityEvent eventDead;

	public GameObject createDestruction;

	public GameObject[] activationDestruction;

	public bool destroyDead = true;

	private float impactForce;

	private Vector3 impactDirection;

	public void Damage(int _int, float _impactForce, Vector3 _ImpactDirection)
	{
		impactForce = _impactForce;
		_ImpactDirection = Vector3.Normalize(_ImpactDirection);
		impactDirection = _ImpactDirection;
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().velocity += _ImpactDirection * _impactForce;
		}
		SetDamage(_int);
	}

	public void Damage(int _int)
	{
		SetDamage(_int);
	}

	private void SetDamage(int _int)
	{
		healthPoint -= _int;
		if (healthPoint < 0)
		{
			healthPoint = 0;
		}
		if (healthPoint == 0)
		{
			Dead();
		}
	}

	public void Dead()
	{
		eventDead.Invoke();
		if (createDestruction != null)
		{
			GameObject gameObject = Object.Instantiate(createDestruction);
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = base.transform.rotation;
			if (gameObject.GetComponent<Rigidbody>() != null)
			{
				gameObject.GetComponent<Rigidbody>().velocity = (impactDirection + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f))) * impactForce;
			}
		}
		if (activationDestruction != null && activationDestruction.Length != 0)
		{
			for (int i = 0; i < activationDestruction.Length; i++)
			{
				activationDestruction[i].transform.parent = null;
				activationDestruction[i].SetActive(value: true);
				if (activationDestruction[i].GetComponent<Rigidbody>() != null)
				{
					activationDestruction[i].GetComponent<Rigidbody>().velocity = (impactDirection + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f))) * impactForce;
				}
			}
		}
		if (destroyDead)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetHealthPoint(int _int)
	{
		healthPoint = _int;
	}
}
