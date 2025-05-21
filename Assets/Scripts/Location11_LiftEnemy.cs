using UnityEngine;

public class Location11_LiftEnemy : MonoBehaviour
{
	private Vector3 position;

	private float timeFly;

	[Header("Полет")]
	public AnimationCurve animationFly;

	private int hp;

	private void Start()
	{
		hp = 3;
		position = base.transform.position;
	}

	private void Update()
	{
		timeFly += Time.deltaTime;
		if (timeFly > 1f)
		{
			timeFly = 0f;
		}
		base.transform.position = position + Vector3.up * animationFly.Evaluate(timeFly);
	}

	public void DamageHead()
	{
		Death();
	}

	public void DamageBody()
	{
		hp--;
		if (hp == 0)
		{
			Death();
		}
	}

	private void Death()
	{
		Object.Destroy(base.gameObject);
	}
}
