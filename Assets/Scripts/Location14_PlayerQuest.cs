using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Location14_PlayerQuest : MonoBehaviour
{
	private Animator anim;

	private Transform targetAnimation;

	private NavMeshAgent nma;

	private UnityEvent eventMoveFinish;

	private Transform targetRotate;

	[Header("Шаги")]
	public AudioSource audioStep;

	public AudioClip[] soundsStep;

	[Header("Сюжетное")]
	public Transform targetSit;

	public Transform targetSpeak;

	public Transform targetStartSpawn;

	private void Start()
	{
		anim = GetComponent<Animator>();
		nma = GetComponent<NavMeshAgent>();
	}

	private void Update()
	{
		if (targetAnimation != null)
		{
			base.transform.SetPositionAndRotation(Vector3.Lerp(base.transform.position, targetAnimation.position, Time.deltaTime * 5f), Quaternion.Lerp(base.transform.rotation, targetAnimation.rotation, Time.deltaTime * 5f));
		}
		if (nma.enabled)
		{
			anim.SetFloat("f", Mathf.Lerp(anim.GetFloat("f"), 1f, Time.deltaTime * 5f));
			if ((double)Vector3.Distance(base.transform.position, nma.destination) < 0.2)
			{
				MoveFinish();
			}
		}
		else
		{
			anim.SetFloat("f", Mathf.Lerp(anim.GetFloat("f"), 0f, Time.deltaTime * 5f));
		}
		if (targetRotate != null)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, Quaternion.LookRotation(targetRotate.position - base.transform.position, Vector3.up).eulerAngles.y, 0f), Time.deltaTime * 5f);
		}
	}

	public void Move(Vector3 _pos, UnityEvent _event)
	{
		eventMoveFinish = _event;
		nma.enabled = true;
		nma.SetDestination(_pos);
		targetRotate = null;
	}

	public void MoveFloor(Vector3 _pos)
	{
		nma.enabled = true;
		NavMeshPath path = new NavMeshPath();
		if (nma.CalculatePath(_pos, path))
		{
			eventMoveFinish = null;
			nma.SetDestination(_pos);
			targetRotate = null;
		}
		else
		{
			nma.enabled = false;
		}
	}

	private void MoveFinish()
	{
		nma.enabled = false;
		if (eventMoveFinish != null)
		{
			eventMoveFinish.Invoke();
		}
	}

	public void RotateOnTarget(Transform _lookTarget)
	{
		if (!nma.enabled)
		{
			targetRotate = _lookTarget;
		}
	}

	public void Step()
	{
		if (anim.GetFloat("f") > 0.1f)
		{
			audioStep.clip = soundsStep[Random.Range(0, soundsStep.Length)];
			audioStep.pitch = Random.Range(0.95f, 1.05f);
			audioStep.Play();
		}
	}

	public void StepAnim()
	{
		audioStep.clip = soundsStep[Random.Range(0, soundsStep.Length)];
		audioStep.pitch = Random.Range(0.95f, 1.05f);
		audioStep.Play();
	}

	public void AnimationPlay(Transform _targetAnimation, string _nameTrigger)
	{
		targetAnimation = _targetAnimation;
		anim.SetTrigger(_nameTrigger);
	}

	private void Teleport()
	{
		nma.enabled = false;
		targetRotate = null;
		anim.SetFloat("f", 0f);
	}

	public void TeleportStart()
	{
		targetAnimation = null;
		Teleport();
		anim.Play("Move", -1, 0f);
		base.transform.SetPositionAndRotation(targetStartSpawn.position, targetStartSpawn.rotation);
	}

	public void TeleportSit()
	{
		Teleport();
		anim.Play("Sit", -1, 0f);
		base.transform.SetPositionAndRotation(targetSit.position, targetSit.rotation);
	}

	public void TeleportSpeak()
	{
		targetAnimation = null;
		Teleport();
		anim.Play("Move", -1, 0f);
		base.transform.SetPositionAndRotation(targetSpeak.position, targetSpeak.rotation);
	}
}
