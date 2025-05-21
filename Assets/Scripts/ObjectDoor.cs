using UnityEngine;

public class ObjectDoor : MonoBehaviour
{
	[Header("Settings")]
	public bool lockDoor;

	private Quaternion rotationWas;

	[Header("Анимация")]
	public AnimationCurve animationClose;

	private float animationCloseTime;

	public float animationCloseSpeed = 1f;

	public EventPoint[] eventsAnimation;

	protected Animator anim;

	protected AnimatorOverrideController animatorOverrideController;

	private float timeAnimationStop;

	private int timeClosedDoor;

	private float timeOpenDoor;

	private bool openAngle;

	private float openAngleRotate;

	private float animationTimeAngle;

	private Quaternion rotateAngleWas;

	[Header("Звуки")]
	public AudioClip[] soundsOpen;

	public AudioClip[] soundsClose;

	public AudioSource audioSource;

	private bool offOptimization;

	[Header("Оптимизация")]
	public bool onX;

	public bool onZ;

	public GameObject[] objectsRight;

	public GameObject[] objectsLeft;

	[Header("Дверь (Инфо)")]
	public bool open;

	public float angleNow;

	private Transform playerT;

	private Quaternion rotationOrigin;

	private Vector3 positionOrigin;

	private HingeJoint hj;

	private Rigidbody rb;

	private bool fs;

	private void Start()
	{
		if (!fs)
		{
			fs = true;
			playerT = GlobalTag.player.transform;
			rotationOrigin = base.transform.rotation;
			positionOrigin = base.transform.position;
			hj = GetComponent<HingeJoint>();
			rb = GetComponent<Rigidbody>();
			animationCloseTime = 1f;
			if (anim == null)
			{
				anim = GetComponent<Animator>();
				animatorOverrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
				anim.runtimeAnimatorController = animatorOverrideController;
			}
			if (lockDoor)
			{
				rb.isKinematic = true;
			}
		}
	}

	private void Update()
	{
		if (lockDoor)
		{
			if (animationCloseTime < 1f)
			{
				animationCloseTime += Time.deltaTime * animationCloseSpeed;
				if (animationCloseTime > 1f)
				{
					animationCloseTime = 1f;
				}
			}
			base.transform.rotation = Quaternion.Lerp(rotationWas, rotationOrigin, animationClose.Evaluate(animationCloseTime));
		}
		if (openAngle)
		{
			if (animationTimeAngle < 1f)
			{
				animationTimeAngle += Time.deltaTime;
				if (animationTimeAngle > 1f)
				{
					animationTimeAngle = 1f;
				}
			}
			base.transform.rotation = Quaternion.Lerp(rotateAngleWas, Quaternion.Euler(-90f, 0f, openAngleRotate), animationClose.Evaluate(animationTimeAngle));
		}
		if (timeOpenDoor > 0f)
		{
			timeOpenDoor -= Time.deltaTime;
			if (timeOpenDoor < 0f)
			{
				timeOpenDoor = 0f;
			}
		}
	}

	private void LateUpdate()
	{
		if (timeAnimationStop > 0f)
		{
			timeAnimationStop -= Time.deltaTime;
			if (timeAnimationStop <= 0f)
			{
				timeAnimationStop = 0f;
				AnimationStop();
			}
		}
	}

	private void FixedUpdate()
	{
		angleNow = hj.angle;
		if (open && timeOpenDoor == 0f && angleNow < 1.5f && angleNow > -1.5f)
		{
			DoorClosed();
		}
		if (!open && (angleNow < -0.2f || angleNow > 0.2f))
		{
			timeOpenDoor = 0.2f;
			DoorOpened();
		}
		if (timeClosedDoor > 0)
		{
			timeClosedDoor--;
			if (timeClosedDoor == 0 && !anim.enabled && !lockDoor)
			{
				rb.isKinematic = false;
			}
		}
		base.transform.rotation = Quaternion.Euler(-90f, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
		if (lockDoor || anim.enabled)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, playerT.position) < 1.5f)
		{
			if (hj.angle <= hj.limits.min)
			{
				rb.isKinematic = true;
				base.transform.position = positionOrigin;
			}
			if (hj.angle >= hj.limits.max)
			{
				rb.isKinematic = true;
				base.transform.position = positionOrigin;
			}
		}
		else if (rb.isKinematic)
		{
			rb.isKinematic = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject == GlobalTag.player && openAngle)
		{
			rb.isKinematic = false;
			openAngle = false;
		}
	}

	private void DoorClosed()
	{
		open = false;
		timeClosedDoor = 2;
		audioSource.clip = soundsClose[Random.Range(0, soundsClose.Length)];
		audioSource.pitch = Random.Range(0.95f, 1.05f);
		audioSource.Play();
		angleNow = 0f;
		base.transform.rotation = rotationOrigin;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = Vector3.zero;
		rb.isKinematic = true;
		if (offOptimization)
		{
			return;
		}
		bool flag = true;
		if (onX && playerT.position.x > base.transform.position.x)
		{
			flag = false;
		}
		if (onZ && playerT.position.z > base.transform.position.z)
		{
			flag = false;
		}
		if (objectsRight != null && objectsRight.Length != 0)
		{
			for (int i = 0; i < objectsRight.Length; i++)
			{
				if (objectsRight[i] != null)
				{
					objectsRight[i].SetActive(flag);
				}
			}
		}
		if (objectsLeft == null || objectsLeft.Length == 0)
		{
			return;
		}
		for (int j = 0; j < objectsLeft.Length; j++)
		{
			if (objectsLeft[j] != null)
			{
				objectsLeft[j].SetActive(!flag);
			}
		}
	}

	private void DoorOpened()
	{
		open = true;
		audioSource.clip = soundsOpen[Random.Range(0, soundsOpen.Length)];
		audioSource.pitch = Random.Range(0.95f, 1.05f);
		audioSource.Play();
		if (!offOptimization)
		{
			OptimizationObjectsActivation();
		}
	}

	public void Lock(bool x)
	{
		lockDoor = x;
		rb.isKinematic = x;
		if (x)
		{
			animationCloseTime = 0f;
			rotationWas = base.transform.rotation;
		}
	}

	public void LockSharply()
	{
		if (!fs)
		{
			Start();
		}
		lockDoor = true;
		animationCloseTime = 1f;
		rb.isKinematic = true;
		base.transform.rotation = rotationOrigin;
	}

	public void AnimationPlay(AnimationClip _clip)
	{
		rb.isKinematic = true;
		anim.enabled = true;
		animatorOverrideController["SimpleA"] = _clip;
		anim.Play("SimpleA", 0, 0f);
	}

	public void AnimationPlayTime(AnimationClip _clip)
	{
		rb.isKinematic = true;
		anim.enabled = true;
		animatorOverrideController["SimpleA"] = _clip;
		anim.Play("SimpleA", 0, 0f);
		timeAnimationStop = _clip.length;
	}

	public void AnimationStop()
	{
		anim.enabled = false;
		if (!lockDoor)
		{
			rb.isKinematic = false;
		}
	}

	public void OpenAngle(float _angle)
	{
		rb.isKinematic = true;
		openAngle = true;
		openAngleRotate = _angle;
		animationTimeAngle = 0f;
		rotateAngleWas = base.transform.localRotation;
	}

	public void OpenAngleStop()
	{
		rb.isKinematic = false;
		openAngle = false;
	}

	public void EV(int _index)
	{
		eventsAnimation[_index]._event.Invoke();
	}

	public void NewEvent(int _index)
	{
		EV(_index);
	}

	public void OptimizationActive(bool x)
	{
		offOptimization = !x;
	}

	public void OptimizationObjectsActivation()
	{
		if (objectsRight != null && objectsRight.Length != 0)
		{
			for (int i = 0; i < objectsRight.Length; i++)
			{
				if (objectsRight[i] != null)
				{
					objectsRight[i].SetActive(value: true);
				}
			}
		}
		if (objectsLeft == null || objectsLeft.Length == 0)
		{
			return;
		}
		for (int j = 0; j < objectsLeft.Length; j++)
		{
			if (objectsLeft[j] != null)
			{
				objectsLeft[j].SetActive(value: true);
			}
		}
	}

	public void ResetOriginPosition()
	{
		positionOrigin = base.transform.position;
	}
}
