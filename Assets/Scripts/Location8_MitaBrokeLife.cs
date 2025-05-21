using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Location8_MitaBrokeLife : MonoBehaviour
{
	public Location8_SlouchLife slouchLife;

	public Transform[] pointsRandomTeleport;

	private PlayerMove scrPlayer;

	private Transform player;

	private float timeDontMolest;

	private float timeCanWalk;

	private float timeMolestStart;

	private bool oneTime;

	private bool canWalk;

	private bool molest;

	[Header("Мита пристаёт")]
	public MitaPerson maitaPerson;

	public NavMeshAgent nma;

	public Character_Look mitaIK;

	public Renderer meshVisibleMita;

	public Interface_KeyHint_Key keyHintDropMolest;

	public ObjectAnimationPlayer animationPlayerMolest;

	public UnityEvent eventMolestStart;

	public UnityEvent eventMolestStop;

	public Transform pointToPlayer;

	[Header("Мита пристаёт первый раз")]
	public UnityEvent eventMolestStartOne;

	public UnityEvent eventMolestStopOne;

	public ObjectAnimationPlayer animationPlayerMolestOneTime;

	[Header("Звуки")]
	public AudioSource audioMolest;

	public AudioSource audioMolestStart;

	private bool fs;

	private void Start()
	{
		if (!fs)
		{
			fs = true;
			scrPlayer = GlobalTag.player.GetComponent<PlayerMove>();
			player = GlobalTag.player.transform;
			keyHintDropMolest.transform.parent.gameObject.SetActive(value: true);
			timeDontMolest = Random.Range(2f, 6f);
		}
	}

	private void Update()
	{
		if (!oneTime)
		{
			if (canWalk)
			{
				if (!meshVisibleMita.isVisible && !scrPlayer.animationRun)
				{
					if (timeDontMolest > 0f)
					{
						timeDontMolest -= Time.deltaTime;
						if (timeDontMolest < 0f)
						{
							timeDontMolest = 0f;
						}
					}
					if ((Vector3.Distance(base.transform.position, player.position) < 2f || Vector3.Dot(base.transform.forward, player.forward) > 0f) && timeDontMolest == 0f && nma.enabled)
					{
						nma.enabled = true;
						if (nma.CalculatePath(player.transform.position - player.transform.forward * 0.5f + player.transform.right * 0.25f, new NavMeshPath()))
						{
							MolestStart();
						}
						else
						{
							timeDontMolest = 0.1f;
						}
					}
				}
				if (Vector3.Distance(base.transform.position, player.position) > 2f)
				{
					if (timeCanWalk == 0f)
					{
						timeCanWalk = 0.1f;
						pointToPlayer.position = player.position + GlobalAM.DirectionFloor(player.position, base.transform.position) * 0.7f;
						if ((double)Vector3.Dot(base.transform.forward, GlobalAM.DirectionFloor(base.transform.position, player.position)) < -0.3)
						{
							maitaPerson.AiWalkToTargetRotate(pointToPlayer, null);
						}
						else
						{
							maitaPerson.AiWalkToTargetTranform(pointToPlayer, null);
						}
					}
					else
					{
						timeCanWalk -= Time.deltaTime;
						if (timeCanWalk < 0f)
						{
							timeCanWalk = 0f;
						}
					}
				}
				else if ((double)Vector3.Distance(base.transform.position, player.position) < 1.5)
				{
					maitaPerson.AiShraplyStop();
				}
			}
			else if (!meshVisibleMita.isVisible)
			{
				nma.enabled = true;
				if (nma.CalculatePath(player.transform.position - player.transform.forward * 0.5f + player.transform.right * 0.25f, new NavMeshPath()))
				{
					MolestStart();
				}
			}
		}
		if (timeMolestStart > 0f)
		{
			base.transform.position = player.transform.position - player.transform.forward * 0.5f + player.transform.right * 0.25f;
			base.transform.rotation = player.transform.rotation;
			timeMolestStart -= Time.deltaTime;
			if (timeMolestStart < 0f)
			{
				timeMolestStart = 0f;
			}
		}
		if (molest)
		{
			if ((double)audioMolest.volume < 0.2)
			{
				audioMolest.volume += Time.deltaTime;
				if ((double)audioMolest.volume > 0.2)
				{
					audioMolest.volume = 0.2f;
				}
			}
		}
		else if (audioMolest.volume > 0f)
		{
			audioMolest.volume -= Time.deltaTime;
			if (audioMolest.volume <= 0f)
			{
				audioMolest.Stop();
				audioMolest.volume = 0f;
			}
		}
	}

	public void MolestStart()
	{
		maitaPerson.AiShraplyStop();
		base.transform.position = player.transform.position - player.transform.forward * 0.5f + player.transform.right * 0.25f;
		base.transform.rotation = player.transform.rotation;
		timeDontMolest = -1f;
		if (!oneTime)
		{
			canWalk = true;
			eventMolestStart.Invoke();
			animationPlayerMolest.transform.SetPositionAndRotation(player.position, player.rotation);
			animationPlayerMolest.AnimationPlay();
			if (slouchLife.gameObject.activeSelf)
			{
				slouchLife.SlowTime();
			}
		}
		else
		{
			eventMolestStartOne.Invoke();
			animationPlayerMolestOneTime.transform.SetPositionAndRotation(player.position, player.rotation);
			animationPlayerMolestOneTime.AnimationPlay();
		}
		mitaIK.IKBodyEnable(x: false);
		mitaIK.ActivationRotateBody(x: false);
		timeMolestStart = 2f;
		molest = true;
		audioMolestStart.Play();
		audioMolest.Play();
	}

	public void MolestPlayerDrop()
	{
		molest = false;
		if (!oneTime)
		{
			eventMolestStop.Invoke();
		}
		else
		{
			eventMolestStopOne.Invoke();
		}
	}

	public void MolestFinish()
	{
		timeDontMolest = Random.Range(3f, 8f);
		mitaIK.IKBodyEnable(x: true);
		mitaIK.ActivationRotateBody(x: true);
	}

	public void MolestOneTime()
	{
		oneTime = true;
		Start();
		MolestStart();
		base.enabled = true;
	}

	public void OffOneTime()
	{
		oneTime = false;
	}

	public void TeleportRandom()
	{
		base.transform.position = pointsRandomTeleport[Random.Range(0, pointsRandomTeleport.Length)].position;
	}

	public void StopWalk()
	{
		maitaPerson.AiShraplyStop();
	}

	public void DontMolestTime()
	{
		timeDontMolest = 8f;
	}
}
