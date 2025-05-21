using UnityEngine;

public class Trigger_Teleport : MonoBehaviour
{
	public Transform targetTeleport;

	public bool rotationReverse;

	private PlayerMove scrpm;

	private float timeDontTP;

	private Transform tPos;

	private void Start()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		tPos = new GameObject().transform;
		tPos.parent = base.transform;
	}

	private void Update()
	{
		if (timeDontTP > 0f)
		{
			timeDontTP -= Time.deltaTime;
			if (timeDontTP < 0f)
			{
				timeDontTP = 0f;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (timeDontTP == 0f && other.gameObject == scrpm.gameObject)
		{
			timeDontTP = 0.2f;
			tPos.SetPositionAndRotation(scrpm.transform.position, scrpm.transform.rotation);
			scrpm.transform.SetPositionAndRotation(GlobalAM.TransformPivot(targetTeleport, tPos.localPosition), targetTeleport.rotation * tPos.localRotation);
			scrpm.TeleportPlayer(scrpm.transform.position, scrpm.transform.eulerAngles.y);
		}
	}
}
