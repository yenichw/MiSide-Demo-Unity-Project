using UnityEngine;
using UnityEngine.AI;

public class Location7_Chibi : MonoBehaviour
{
	private Animator anim;

	private NavMeshAgent nma;

	private float timeSpeak;

	[Header("Грустный")]
	public Transform targetSit;

	public GameObject textSpeak;

	private float timeCheckPlayer;

	private Transform playerT;

	private float randomPosition;

	private int goEnd;

	private bool readyFinish;

	private float timeVoice;

	[Header("Боится игрока")]
	public LayerMask wall;

	public Transform targetEnd;

	public Location7_DoorChibi componentDoor;

	private void Start()
	{
		base.transform.localScale = Vector3.one * 0.08f;
		anim = GetComponent<Animator>();
		nma = GetComponent<NavMeshAgent>();
		if (targetSit != null)
		{
			Move(targetSit.transform.position);
		}
		playerT = GlobalTag.player.transform;
		if (targetSit == null)
		{
			Move(base.transform.position + base.transform.forward * Random.Range(1f, 2f) + new Vector3(Random.Range(1f, -1f), 0f, Random.Range(1f, -1f)));
			randomPosition = Random.Range(1f, 15f);
			timeVoice = Random.Range(0.5f, 3f);
		}
	}

	private void Update()
	{
		if (nma.enabled)
		{
			if ((double)Vector3.Distance(base.transform.position, nma.destination) > 0.3)
			{
				anim.SetBool("Run", value: true);
			}
			else
			{
				anim.SetBool("Run", value: false);
			}
			if (Vector3.Distance(base.transform.position, nma.destination) < 0.1f)
			{
				nma.enabled = false;
				anim.SetBool("Run", value: false);
				if (targetSit != null)
				{
					anim.SetTrigger("Sit");
					nma.enabled = false;
					GetComponent<AudioSource>().Play();
				}
			}
		}
		if (targetSit != null && !nma.enabled)
		{
			timeSpeak += Time.deltaTime;
			if (timeSpeak > 1.5f)
			{
				timeSpeak = 0f;
				GameObject obj = Object.Instantiate(textSpeak, textSpeak.transform.parent);
				obj.transform.position += GlobalAM.Vector3Random(-0.25f, 0.25f);
				obj.transform.rotation *= Quaternion.Euler(GlobalAM.Vector3Random(-10f, 10f));
				obj.SetActive(value: true);
			}
		}
		if (targetSit == null && goEnd == 0)
		{
			if (timeCheckPlayer == 0f && (double)Vector3.Distance(base.transform.position, playerT.position) < 0.75)
			{
				if (!Physics.Linecast(base.transform.position + Vector3.up * 0.2f, base.transform.position + Vector3.up * 0.2f - GlobalAM.DirectionFloor(base.transform.position, playerT.position) * 0.5f, wall))
				{
					Move(base.transform.position + GlobalAM.DirectionFloor(base.transform.position, playerT.position) * Random.Range(-0.5f, -1f));
					timeCheckPlayer = 0.1f;
				}
				else
				{
					Move(base.transform.position + new Vector3(Random.Range(1f, -1f), 0f, Random.Range(1f, -1f)));
					timeCheckPlayer = 0.7f;
				}
			}
			if (timeCheckPlayer > 0f)
			{
				timeCheckPlayer -= Time.deltaTime;
				if (timeCheckPlayer < 0f)
				{
					timeCheckPlayer = 0f;
				}
			}
			randomPosition -= Time.deltaTime;
			if (randomPosition < 0f)
			{
				randomPosition = Random.Range(1f, 15f);
				Move(base.transform.position + new Vector3(Random.Range(1f, -1f), 0f, Random.Range(1f, -1f)));
			}
			if (Vector3.Distance(base.transform.position, targetEnd.position) < 1f)
			{
				goEnd = 1;
				anim.SetBool("Run", value: true);
				nma.enabled = false;
			}
			timeVoice -= Time.deltaTime;
			if (timeVoice < 0f)
			{
				timeVoice = Random.Range(0.5f, 3f);
				GetComponent<Audio_Data>().RandomPlayPitch();
			}
		}
		if (goEnd == 1)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetEnd.position, Time.deltaTime * 1f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, Quaternion.LookRotation(targetEnd.position - base.transform.position, Vector3.up).eulerAngles.y, 0f), Time.deltaTime * 8f);
			if ((double)Vector3.Distance(base.transform.position, targetEnd.position) < 0.02)
			{
				goEnd = 2;
			}
		}
		if (goEnd == 2)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, new Vector3(-2f, 1f, -32.4f), Time.deltaTime * 1f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, Quaternion.LookRotation(new Vector3(-2f, 1f, -32.4f) - base.transform.position, Vector3.up).eulerAngles.y, 0f), Time.deltaTime * 8f);
			if (!readyFinish && base.transform.position.x > -6f)
			{
				readyFinish = true;
				componentDoor.ChibiReady();
			}
			if (base.transform.position.x > -4f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.one * 0.15f, Time.deltaTime * 3f);
	}

	public void Move(Vector3 _point)
	{
		nma.enabled = true;
		NavMeshPath path = new NavMeshPath();
		if (nma.CalculatePath(_point, path))
		{
			nma.enabled = true;
			nma.SetDestination(_point);
		}
		else
		{
			nma.enabled = false;
		}
	}
}
