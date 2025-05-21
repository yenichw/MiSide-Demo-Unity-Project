using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class Mob_Maneken : MonoBehaviour
{
	public float speedNav;

	public SkinnedMeshRenderer rend;

	public LayerMask layerFindPlayer;

	private bool castPlayer;

	private Transform playerT;

	private RaycastHit hit;

	private bool playStart;

	private float timeStartAnimation;

	[Header("Условие для начала")]
	public AnimationClip animationStart;

	private float timeCheckKill;

	private float animationTimeKill;

	[Header("Убийство")]
	public AnimationClip animationKill;

	public UnityEvent eventKillStart;

	public UnityEvent eventKillStop;

	public Transform animationPlayerKill;

	public AudioSource audioKillPlayer;

	public AudioSource audioAttack;

	private bool stop;

	private AudioSource ad;

	private Animator anim;

	private float timeVis;

	private NavMeshAgent nma;

	private Transform player;

	private bool fs;

	private void Start()
	{
		if (!fs)
		{
			fs = true;
			playerT = GlobalTag.player.transform;
			ad = GetComponent<AudioSource>();
			anim = GetComponent<Animator>();
			nma = GetComponent<NavMeshAgent>();
			player = GlobalTag.player.transform;
			rend.updateWhenOffscreen = true;
		}
	}

	private void Update()
	{
		if (animationStart == null && animationTimeKill == 0f && !stop)
		{
			if (!rend.isVisible || !castPlayer)
			{
				if (nma.enabled)
				{
					nma.speed = speedNav;
					nma.SetDestination(player.position);
				}
				anim.speed = 1f;
				ad.volume = Mathf.Lerp(ad.volume, 1f, Time.deltaTime * 15f);
				if (!rend.isVisible)
				{
					timeVis = 0.35f;
				}
				else
				{
					timeVis = 0.01f;
				}
			}
			else if (timeVis > 0f)
			{
				timeVis -= Time.deltaTime;
				if (timeVis < 0f)
				{
					timeVis = 0f;
					if (nma.enabled)
					{
						nma.SetDestination(base.transform.position);
						nma.speed = 0f;
					}
				}
			}
			else
			{
				anim.speed = Mathf.Lerp(anim.speed, 0f, Time.deltaTime * 15f);
				ad.volume = Mathf.Lerp(ad.volume, 0f, Time.deltaTime * 15f);
			}
		}
		if (animationStart != null && playStart)
		{
			timeStartAnimation -= Time.deltaTime;
			if (timeStartAnimation < 0f)
			{
				animationStart = null;
				timeVis = 0.1f;
			}
		}
		if (animationTimeKill > 0f)
		{
			animationTimeKill -= Time.deltaTime;
			if (animationTimeKill <= 0f)
			{
				animationTimeKill = 0f;
				eventKillStop.Invoke();
				audioAttack.Stop();
				audioKillPlayer.Stop();
				GlobalTag.world.GetComponent<WorldPlayer>().CameraGlitch(_x: false);
				GlobalTag.world.GetComponent<WorldPlayer>().ScreenBlackTime(0.5f);
			}
		}
		if (stop || !((double)Vector3.Distance(base.transform.position, playerT.position) < 0.75))
		{
			return;
		}
		timeCheckKill += Time.deltaTime;
		if (timeCheckKill > 0.2f)
		{
			timeCheckKill = 0f;
			rend.shadowCastingMode = ShadowCastingMode.Off;
			if (!rend.isVisible && castPlayer)
			{
				StartKillPlayer();
			}
			rend.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	private void FixedUpdate()
	{
		if (Physics.SphereCast(base.transform.position + Vector3.up * 1.5f, 0.05f, GlobalAM.DirectionFloor(base.transform.position, playerT.position), out hit, 100f, layerFindPlayer))
		{
			if (hit.collider.gameObject.transform == playerT)
			{
				castPlayer = true;
			}
			else
			{
				castPlayer = false;
			}
		}
		else
		{
			castPlayer = false;
		}
	}

	public void Play()
	{
		timeStartAnimation = animationStart.length - 0.1f;
		playStart = true;
		anim.speed = 1f;
	}

	public void StartKillPlayer()
	{
		animationPlayerKill.SetPositionAndRotation(playerT.position, playerT.rotation);
		base.transform.SetPositionAndRotation(playerT.position - playerT.forward, playerT.rotation);
		anim.Play("Kill", 0, 0f);
		eventKillStart.Invoke();
		animationTimeKill = animationKill.length;
		nma.enabled = false;
		animationPlayerKill.GetComponent<ObjectAnimationPlayer>().AnimationPlay();
		stop = true;
		audioAttack.Play();
		audioKillPlayer.Play();
		GlobalTag.world.GetComponent<WorldPlayer>().CameraGlitch(_x: true);
	}

	public void ResetManeken()
	{
		stop = false;
		animationTimeKill = 0f;
		nma.enabled = true;
	}

	public void Stop(bool x)
	{
		if (fs)
		{
			stop = x;
			anim.speed = 0f;
			if (nma.enabled)
			{
				nma.SetDestination(base.transform.position);
			}
		}
	}

	public void DeactivationManeken()
	{
		nma.enabled = false;
		anim.SetTrigger("Stop");
		anim.speed = 1f;
		base.enabled = false;
		ad.Stop();
	}
}
