using UnityEngine;

public class Transform_Move : MonoBehaviour
{
	public enum TypeMove
	{
		Forward = 0,
		Right = 1,
		Up = 2,
		None = 3
	}

	public TypeMove typeMove;

	public Vector3 myMove;

	public float speed;

	public bool active;

	public bool SmoothUse;

	[Range(0f, 1f)]
	public float SmoothSpeed = 0.1f;

	private Vector3 speedMove;

	private float _spd;

	private void FixedUpdate()
	{
		if (!active)
		{
			return;
		}
		if (!SmoothUse)
		{
			if (typeMove == TypeMove.Forward)
			{
				base.transform.position += base.transform.forward * speed;
			}
			if (typeMove == TypeMove.Right)
			{
				base.transform.position += base.transform.right * speed;
			}
			if (typeMove == TypeMove.Up)
			{
				base.transform.position += base.transform.up * speed;
			}
			base.transform.position += myMove;
			return;
		}
		if (typeMove == TypeMove.Forward)
		{
			base.transform.position += base.transform.forward * _spd;
		}
		if (typeMove == TypeMove.Right)
		{
			base.transform.position += base.transform.right * _spd;
		}
		if (typeMove == TypeMove.Up)
		{
			base.transform.position += base.transform.up * _spd;
		}
		base.transform.position += speedMove;
		speedMove = Vector3.Lerp(speedMove, myMove, SmoothSpeed);
		_spd = Mathf.Lerp(_spd, speed, SmoothSpeed);
	}

	public void Activation(bool x)
	{
		active = x;
	}

	public void ActivationOn()
	{
		active = true;
	}
}
