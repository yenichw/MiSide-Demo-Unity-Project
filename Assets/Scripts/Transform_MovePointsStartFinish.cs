using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Transform/Move start-finish")]
public class Transform_MovePointsStartFinish : MonoBehaviour
{
	public bool onStart = true;

	public TransformPositionRotation[] points;

	public bool useRotation;

	public bool local;

	public bool loop;

	private Vector3 positionWas;

	private Quaternion rotationWas;

	private bool play;

	private int indexPoint;

	private float animationMoveTime;

	[Header("Animation")]
	public AnimationCurve animationMove;

	public float speedMove = 1f;

	[Header("Размер")]
	public bool scaleAnimation;

	private AudioSource au;

	[Header("Звук")]
	public bool audioAnimation;

	public float audioAnimationVolume = 0.5f;

	[Header("События")]
	public UnityEvent eventFinish;

	private bool smoothDestroy;

	private void Start()
	{
		if (onStart)
		{
			StartMove();
		}
	}

	private void Update()
	{
		if (play && animationMoveTime < 1f)
		{
			animationMoveTime += Time.deltaTime * speedMove;
			if (animationMoveTime > 1f)
			{
				animationMoveTime = 1f;
			}
			if (!smoothDestroy)
			{
				if (scaleAnimation)
				{
					if (indexPoint == 1)
					{
						base.transform.localScale = Vector3.one * animationMoveTime;
					}
					if (indexPoint == points.Length - 1)
					{
						base.transform.localScale = Vector3.one * (1f - animationMoveTime);
					}
				}
				if (audioAnimation)
				{
					if (indexPoint == 1)
					{
						au.volume = animationMoveTime * audioAnimationVolume;
					}
					if (indexPoint == points.Length - 1)
					{
						au.volume = (1f - animationMoveTime) * audioAnimationVolume;
					}
				}
			}
			if (animationMoveTime >= 1f)
			{
				animationMoveTime = 0f;
				indexPoint++;
				if (!local)
				{
					positionWas = base.transform.position;
				}
				else
				{
					positionWas = base.transform.localPosition;
				}
				if (indexPoint == points.Length)
				{
					play = false;
					indexPoint--;
					eventFinish.Invoke();
					if (loop)
					{
						StartMove();
					}
				}
			}
			if (!local)
			{
				base.transform.position = Vector3.Lerp(positionWas, points[indexPoint].position, animationMove.Evaluate(animationMoveTime));
				if (useRotation)
				{
					base.transform.rotation = Quaternion.Lerp(rotationWas, Quaternion.Euler(points[indexPoint].rotation), animationMove.Evaluate(animationMoveTime));
				}
			}
			else
			{
				base.transform.localPosition = Vector3.Lerp(positionWas, points[indexPoint].rotation, animationMove.Evaluate(animationMoveTime));
				if (useRotation)
				{
					base.transform.localRotation = Quaternion.Lerp(rotationWas, Quaternion.Euler(points[indexPoint].rotation), animationMove.Evaluate(animationMoveTime));
				}
			}
		}
		if (!smoothDestroy)
		{
			return;
		}
		if (scaleAnimation)
		{
			base.transform.localScale -= Vector3.one * Time.deltaTime;
			if (base.transform.localScale.x < 0f)
			{
				base.transform.localScale = Vector3.zero;
				if (!audioAnimation)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
		if (audioAnimation)
		{
			au.volume -= Time.deltaTime;
			if (au.volume < 0f)
			{
				au.volume = 0f;
				if (!scaleAnimation)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
		if (scaleAnimation && audioAnimation && base.transform.localScale.x == 0f && au.volume == 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void StartMove()
	{
		animationMoveTime = 0f;
		indexPoint = 1;
		play = true;
		if (!local)
		{
			base.transform.position = points[0].position;
			if (useRotation)
			{
				base.transform.rotation = Quaternion.Euler(points[0].rotation);
			}
			positionWas = base.transform.position;
			rotationWas = base.transform.rotation;
		}
		else
		{
			base.transform.localPosition = points[0].position;
			if (useRotation)
			{
				base.transform.localRotation = Quaternion.Euler(points[0].rotation);
			}
			positionWas = base.transform.localPosition;
			rotationWas = base.transform.localRotation;
		}
		if (scaleAnimation)
		{
			base.transform.localScale = Vector3.zero;
		}
		if (audioAnimation)
		{
			au = GetComponent<AudioSource>();
			au.volume = 0f;
		}
	}

	public void Loop(bool _x)
	{
		loop = _x;
	}

	public void SmoothDestroy()
	{
		smoothDestroy = true;
	}
}
