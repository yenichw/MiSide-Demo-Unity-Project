using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Mob_ChibiMita : MonoBehaviour
{
	private NavMeshAgent nma;

	[HideInInspector]
	public UnityEvent eventWalkToPoint;

	private bool walkGo;

	private Vector3 pointGo;

	public Animator anim;

	public string nameAnimationWalk;

	public string nameAnimationIdle;

	private bool typeAnimation;

	private Mob_ChibiMita_Animation animationScript;

	private float animationTimePlay;

	private bool animationPlay;

	private AnimatorOverrideController animOver;

	private Character_LookSimple chibiLook;

	private float rotateNeed;

	private float timeNeedRotate;

	private void Start()
	{
		nma = GetComponent<NavMeshAgent>();
		animOver = new AnimatorOverrideController(anim.runtimeAnimatorController);
		anim.runtimeAnimatorController = animOver;
		chibiLook = anim.GetComponent<Character_LookSimple>();
	}

	private void Update()
	{
		if (nma.enabled)
		{
			anim.SetFloat("Walk", Mathf.Lerp(anim.GetFloat("Walk"), 1f, Time.deltaTime * 5f));
			if (nma.path.corners.Length > 1 && GlobalAM.DirectionFloor(base.transform.position, nma.path.corners[1]) != Vector3.zero)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(GlobalAM.DirectionFloor(base.transform.position, nma.path.corners[1]), Vector3.up), Time.deltaTime * 8f);
			}
			if (animationScript != null && Vector3.Distance(base.transform.position, animationScript.transform.position) < 0.025f)
			{
				AnimationPlay(animationScript);
			}
			if (animationScript == null && walkGo && Vector3.Distance(base.transform.position, pointGo) < 0.025f)
			{
				walkGo = false;
				chibiLook.Activation(x: true);
				eventWalkToPoint.Invoke();
			}
		}
		else
		{
			anim.SetFloat("Walk", Mathf.Lerp(anim.GetFloat("Walk"), 0f, Time.deltaTime * 5f));
		}
		if (animationPlay)
		{
			UpdateAnimation();
		}
		if (timeNeedRotate > 0f)
		{
			timeNeedRotate -= Time.deltaTime;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(new Vector3(0f, rotateNeed, 0f)), Time.deltaTime * 8f);
		}
	}

	private void UpdateAnimation()
	{
		if (animationScript.animationPositionThis)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, animationScript.transform.position, Time.deltaTime * 8f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, animationScript.transform.rotation, Time.deltaTime * 8f);
		}
		if (!(animationTimePlay > 0f))
		{
			return;
		}
		animationTimePlay -= Time.deltaTime;
		if (!(animationTimePlay <= 0f))
		{
			return;
		}
		animationTimePlay = 0f;
		if (animationScript.animationLoop != null)
		{
			if (!typeAnimation)
			{
				animOver["SimpleA"] = animationScript.animationLoop;
				anim.SetBool("OtherAnimType", value: false);
			}
			else
			{
				animOver["SimpleB"] = animationScript.animationLoop;
				anim.SetBool("OtherAnimType", value: true);
			}
			typeAnimation = !typeAnimation;
			animationScript.animationLoop = null;
			animationScript.eventStartLoop.Invoke();
		}
		else if (animationScript.animationStop == null)
		{
			AnimationStop();
		}
	}

	public void GoWalk(Vector3 _positionWalk, UnityEvent _event)
	{
		eventWalkToPoint = _event;
		walkGo = true;
		MovePlay(_positionWalk);
		pointGo = _positionWalk;
		animationScript = null;
	}

	public void GoMoveAnimation(Mob_ChibiMita_Animation _scrChibiAnimation)
	{
		timeNeedRotate = 0f;
		chibiLook.Activation(x: false);
		MovePlay(_scrChibiAnimation.transform.position);
		animationScript = _scrChibiAnimation;
	}

	public void AnimationPlay(Mob_ChibiMita_Animation _scrChibiAnimation)
	{
		timeNeedRotate = 0f;
		animationScript = _scrChibiAnimation;
		if (!animationScript.holdIKLook)
		{
			chibiLook.Activation(x: false);
		}
		MoveStop();
		if (!typeAnimation)
		{
			animOver["SimpleA"] = _scrChibiAnimation.animationStart;
			anim.SetBool("OtherAnimType", value: false);
		}
		else
		{
			animOver["SimpleB"] = _scrChibiAnimation.animationStart;
			anim.SetBool("OtherAnimType", value: true);
		}
		typeAnimation = !typeAnimation;
		animationPlay = true;
		animationTimePlay = _scrChibiAnimation.animationStart.length - 0.15f;
		animationScript.eventStart.Invoke();
		anim.SetBool("OtherAnimation", value: true);
	}

	public void AnimationPlayFast(Mob_ChibiMita_Animation _scrChibiAnimation)
	{
		AnimationPlay(_scrChibiAnimation);
		if (animationScript.animationPositionThis)
		{
			base.transform.position = animationScript.transform.position;
			base.transform.rotation = animationScript.transform.rotation;
		}
	}

	public void AnimationStopPlay(Mob_ChibiMita_Animation _scrChibiAnimation)
	{
		animationScript = _scrChibiAnimation;
		MoveStop();
		if (!typeAnimation)
		{
			animOver["SimpleA"] = animationScript.animationStop;
			anim.SetBool("OtherAnimType", value: false);
		}
		else
		{
			animOver["SimpleB"] = animationScript.animationStop;
			anim.SetBool("OtherAnimType", value: true);
		}
		typeAnimation = !typeAnimation;
		animationTimePlay = _scrChibiAnimation.animationStop.length - 0.15f;
		anim.SetBool("OtherAnimation", value: true);
		animationScript.animationStop = null;
	}

	public void AnimationStop()
	{
		if (!animationScript.holdIKLook)
		{
			chibiLook.Activation(x: true);
		}
		anim.SetBool("OtherAnimation", value: false);
		animationPlay = false;
		Mob_ChibiMita_Animation mob_ChibiMita_Animation = animationScript;
		animationScript.eventFinish.Invoke();
		if (animationScript == mob_ChibiMita_Animation)
		{
			animationScript = null;
		}
	}

	public void Rotate(float x)
	{
		rotateNeed = x;
		timeNeedRotate = 1f;
	}

	public void TeleportTarget(Transform _target)
	{
		base.transform.SetPositionAndRotation(_target.position, _target.rotation);
	}

	public void MovePlay(Vector3 _pos)
	{
		nma.enabled = true;
		nma.destination = _pos;
	}

	public void MoveStop()
	{
		nma.enabled = false;
	}

	public void ReAnimationMove(AnimationClip _animationClipWalk)
	{
		animOver[nameAnimationWalk] = _animationClipWalk;
	}

	public void ReAnimationMove(AnimationClip _animationClipWalk, AnimationClip _animationClipIdle)
	{
		animOver[nameAnimationWalk] = _animationClipWalk;
		animOver[nameAnimationIdle] = _animationClipIdle;
	}

	public void AnimatorReset()
	{
		anim.Rebind();
		anim.Play(0);
	}
}
