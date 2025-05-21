using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ObjectAnimationPlayer))]
public class ObjectAnimationPlayerHold : MonoBehaviour
{
	[Header("Animation key")]
	public AnimationClip animationHold;

	[Range(0f, 1f)]
	public float keyForward = 0.1f;

	public float speedForward = 1f;

	public float speedBack = 1f;

	public bool hold;

	public string key;

	public AnimationHoldOtherAnimator[] otherAnimatons;

	public UnityEvent eventReady;

	[Header("Звуки")]
	public AudioSource audioHold;

	[HideInInspector]
	public float animationTimeHold;

	private float animationTimeHoldNow;

	private float timeAddKey;

	private bool ready;

	private PlayerMove scrpm;

	private ObjectAnimationPlayer scroap;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		scroap = GetComponent<ObjectAnimationPlayer>();
		key = key.ToLower();
	}

	private void Update()
	{
		if (!ready)
		{
			if (scrpm.animationRun && scrpm.animationLoopRun && scrpm.scrAnimationNow == scroap)
			{
				if (!hold)
				{
					if (Input.GetKeyDown(key))
					{
						AnimationDown();
					}
				}
				else if (Input.GetKey(key))
				{
					AnimationDown();
				}
				for (int i = 0; i < otherAnimatons.Length; i++)
				{
					otherAnimatons[i].anim.SetFloat(otherAnimatons[i].nameFloat, animationTimeHold);
				}
				if (animationTimeHoldNow > 0f && animationTimeHoldNow < 1f)
				{
					animationTimeHold = Mathf.Lerp(animationTimeHold, animationTimeHoldNow, Time.deltaTime * 8f);
				}
				if (animationTimeHoldNow == 0f)
				{
					animationTimeHold -= Time.deltaTime * speedBack;
					if (animationTimeHold < 0f)
					{
						animationTimeHold = 0f;
					}
				}
				if (animationTimeHoldNow == 1f)
				{
					animationTimeHold += Time.deltaTime * speedForward;
					if (animationTimeHold > 1f)
					{
						animationTimeHold = 1f;
					}
				}
				if (animationTimeHold >= 1f)
				{
					Ready();
				}
			}
			if (!(animationTimeHoldNow < 1f))
			{
				return;
			}
			if (timeAddKey > 0f)
			{
				timeAddKey -= Time.deltaTime * speedForward;
				animationTimeHoldNow += Time.deltaTime * speedForward;
				if (timeAddKey < 0f)
				{
					timeAddKey = 0f;
				}
				if (animationTimeHoldNow >= 1f)
				{
					animationTimeHoldNow = 1f;
				}
				if (audioHold != null && audioHold.volume < 1f)
				{
					audioHold.volume += Time.deltaTime;
					if (audioHold.volume >= 1f)
					{
						audioHold.volume = 1f;
					}
				}
				return;
			}
			if (animationTimeHoldNow > 0f)
			{
				animationTimeHoldNow -= Time.deltaTime * speedBack;
				if (animationTimeHoldNow < 0f)
				{
					animationTimeHoldNow = 0f;
				}
			}
			if (audioHold != null && audioHold.volume > 0f)
			{
				audioHold.volume -= Time.deltaTime;
				if (audioHold.volume <= 0f)
				{
					audioHold.volume = 0f;
				}
			}
		}
		else if (audioHold != null && audioHold.volume > 0f)
		{
			audioHold.volume -= Time.deltaTime;
			if (audioHold.volume <= 0f)
			{
				Object.Destroy(audioHold);
			}
		}
	}

	public void Restart()
	{
		ready = false;
		animationTimeHoldNow = 0f;
		animationTimeHold = 0f;
	}

	private void AnimationDown()
	{
		timeAddKey = keyForward;
		if (audioHold != null && !audioHold.isPlaying)
		{
			audioHold.Play();
			audioHold.time = audioHold.clip.length * animationTimeHold;
		}
	}

	private void Ready()
	{
		eventReady.Invoke();
		ready = true;
	}
}
