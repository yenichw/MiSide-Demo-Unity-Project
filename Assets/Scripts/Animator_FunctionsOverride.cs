using UnityEngine;
using UnityEngine.Events;

public class Animator_FunctionsOverride : MonoBehaviour
{
	[Header("[EventKey] or [NewEvent]")]
	public UnityEvent[] eventAnimation;

	protected Animator anim;

	[HideInInspector]
	public AnimatorOverrideController animOver;

	[HideInInspector]
	public float timeAnimationNext;

	private int timeAnimOtherFloat;

	private float animatorFloatOtherA;

	private float animatorFloatOtherB;

	private bool order;

	[Header("Settings")]
	public bool simpleAFloatNotExist;

	private bool fs;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (!simpleAFloatNotExist)
		{
			if (timeAnimOtherFloat == 0)
			{
				if (animatorFloatOtherA > 0f)
				{
					animatorFloatOtherA -= Time.deltaTime;
					if (animatorFloatOtherA < 0f)
					{
						animatorFloatOtherA = 0f;
					}
				}
				if (animatorFloatOtherB > 0f)
				{
					animatorFloatOtherB -= Time.deltaTime;
					if (animatorFloatOtherB < 0f)
					{
						animatorFloatOtherB = 0f;
					}
				}
				anim.SetFloat("SimpleAFloat", animatorFloatOtherA);
				anim.SetFloat("SimpleBFloat", animatorFloatOtherB);
			}
			else
			{
				timeAnimOtherFloat--;
			}
		}
		if (timeAnimationNext > 0f)
		{
			timeAnimationNext -= Time.deltaTime;
			if (timeAnimationNext < 0f)
			{
				timeAnimationNext = 0f;
			}
		}
	}

	private void StartComponent()
	{
		if (!fs)
		{
			fs = true;
			anim = GetComponent<Animator>();
			animOver = new AnimatorOverrideController(anim.runtimeAnimatorController);
			anim.runtimeAnimatorController = animOver;
		}
	}

	public void BoolOn(string x)
	{
		if (!fs)
		{
			StartComponent();
		}
		anim.SetBool(x, value: true);
	}

	public void BoolOff(string x)
	{
		if (!fs)
		{
			StartComponent();
		}
		anim.SetBool(x, value: false);
	}

	public void TriggerClick(string x)
	{
		if (!fs)
		{
			StartComponent();
		}
		anim.SetTrigger(x);
	}

	public void BoolSwitch(string x)
	{
		if (!fs)
		{
			StartComponent();
		}
		anim.SetBool(x, !anim.GetBool(x));
	}

	public void EventKey(int x)
	{
		eventAnimation[x].Invoke();
	}

	public void NewEvent(int x)
	{
		eventAnimation[x].Invoke();
	}

	public void ResetOrder()
	{
		order = true;
		anim.ResetTrigger("NextLerp");
		ConsoleMain.ConsolePrintGame("Reset order animation.");
	}

	public void CheckOrder()
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("SimpleA"))
		{
			order = true;
		}
	}

	public void AnimationClipSimple(AnimationClip _animation)
	{
		if (!fs)
		{
			StartComponent();
		}
		animOver["SimpleA"] = _animation;
		animOver["SimpleB"] = null;
		anim.Play("SimpleA", -1, 0f);
		anim.SetBool("Walk", value: false);
		order = false;
		anim.ResetTrigger("NextLerp");
		if (_animation.isLooping)
		{
			timeAnimationNext = 0f;
		}
		else
		{
			timeAnimationNext = _animation.length;
		}
	}

	public void AnimationClipSimpleNext(AnimationClip _animation)
	{
		if (!fs)
		{
			StartComponent();
		}
		if (anim.GetBool("Walk"))
		{
			animOver["SimpleA"] = _animation;
			order = false;
			ConsoleMain.ConsolePrintGame("AnimationClipNext[A] (was Walk)");
		}
		else
		{
			order = !order;
			if (!order)
			{
				animOver["SimpleA"] = _animation;
				ConsoleMain.ConsolePrintGame("AnimationClipNext[A]");
			}
			else
			{
				animOver["SimpleB"] = _animation;
				ConsoleMain.ConsolePrintGame("AnimationClipNext[B]");
			}
		}
		anim.SetTrigger("NextLerp");
		if (_animation.isLooping)
		{
			timeAnimationNext = 0f;
		}
		else
		{
			timeAnimationNext = _animation.length;
		}
	}

	public void AnimationClipWalk(AnimationClip _animation)
	{
		if (!fs)
		{
			StartComponent();
		}
		animOver["Walk"] = _animation;
	}

	public void ReAnimationClip(string _nameOriginAnimationClip, AnimationClip _animationClip)
	{
		animOver[_nameOriginAnimationClip] = _animationClip;
	}

	public void AnimationIdlePlayAfterWalk(AnimationClip _animcationClip)
	{
		animOver["SimpleA"] = _animcationClip;
		animOver["SimpleB"] = null;
		order = false;
		anim.SetBool("Walk", value: false);
		anim.SetTrigger("NextLerp");
	}

	public void AnimationFloatSet(bool ab, float _float)
	{
		if (ab)
		{
			animatorFloatOtherA = Mathf.Lerp(animatorFloatOtherA, _float, Time.deltaTime * 8f);
			anim.SetFloat("SimpleAFloat", animatorFloatOtherA);
		}
		else
		{
			animatorFloatOtherB = Mathf.Lerp(animatorFloatOtherB, _float, Time.deltaTime * 8f);
			anim.SetFloat("SimpleBFloat", animatorFloatOtherB);
		}
		timeAnimOtherFloat = 2;
	}
}
