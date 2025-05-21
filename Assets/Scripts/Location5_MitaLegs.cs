using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Location5_MitaLegs : MonoBehaviour
{
	private Animator anim;

	private Transform player;

	private NavMeshAgent nma;

	private Transform targetMove;

	private bool eventOk;

	[Header("Navigation")]
	public Transform positionFirst;

	public Transform positionSecond;

	public UnityEvent eventStartPlayer;

	private bool ending;

	[Header("Ending")]
	public Transform positionAnimation;

	private void Start()
	{
		anim = GetComponent<Animator>();
		player = GlobalTag.player.transform;
		nma = GetComponent<NavMeshAgent>();
		targetMove = positionFirst;
	}

	private void Update()
	{
		if (!ending)
		{
			if (Vector3.Distance(base.transform.position, player.position) < 3f)
			{
				if (!nma.enabled)
				{
					nma.enabled = true;
					nma.destination = targetMove.position;
				}
				anim.SetFloat("w", Mathf.Lerp(anim.GetFloat("w"), 1f, Time.deltaTime * 5f));
				if (!eventOk)
				{
					eventOk = true;
					eventStartPlayer.Invoke();
				}
			}
			else
			{
				nma.enabled = false;
				anim.SetFloat("w", Mathf.Lerp(anim.GetFloat("w"), 0f, Time.deltaTime * 5f));
			}
			if (nma.path.corners.Length > 1 && GlobalAM.DirectionFloor(base.transform.position, nma.path.corners[1]) != Vector3.zero)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(GlobalAM.DirectionFloor(base.transform.position, nma.path.corners[1]), Vector3.up), Time.deltaTime * 8f);
			}
			if (targetMove == positionFirst && (double)Vector3.Distance(base.transform.position, targetMove.position) < 0.6)
			{
				targetMove = positionSecond;
				if (nma.enabled)
				{
					nma.destination = targetMove.position;
				}
			}
			if (targetMove == positionSecond && (double)Vector3.Distance(base.transform.position, targetMove.position) < 0.6)
			{
				targetMove = positionAnimation;
				if (nma.enabled)
				{
					nma.destination = targetMove.position;
				}
			}
			if (targetMove == positionAnimation && (double)Vector3.Distance(base.transform.position, targetMove.position) < 0.2)
			{
				ending = true;
				anim.SetTrigger("n");
			}
		}
		else
		{
			base.transform.SetPositionAndRotation(Vector3.Lerp(base.transform.position, positionAnimation.position, Time.deltaTime * 8f), Quaternion.Lerp(base.transform.rotation, positionAnimation.rotation, Time.deltaTime * 8f));
		}
	}
}
