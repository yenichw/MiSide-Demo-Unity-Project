using UnityEngine;

public class LifeObject_Limb : MonoBehaviour
{
	public LifeObject main;

	public IK_LimbFabrik farbik;

	public void Damage(int _int, float _impactForce, Vector3 _ImpactDirection, Vector3 _ImpactPosition)
	{
		farbik.Shot(_ImpactPosition + _ImpactDirection);
		main.Damage(_int, _impactForce, _ImpactDirection);
	}

	public void Damage(int _int)
	{
		main.Damage(_int);
	}
}
