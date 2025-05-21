using UnityEngine;
using UnityEngine.Events;

public class Location15_MitaKind_Follow : MonoBehaviour
{
	private Transform mitaT;

	public MitaPerson mita;

	public Transform positionSit;

	public UnityEvent eventGoSit;

	public UnityEvent eventStartSit;

	private bool follow;

	private Transform playerT;

	private Transform positionPlayerLast;

	private float timeUpdatePositionLast;

	private void Update()
	{
		if (follow)
		{
			timeUpdatePositionLast += Time.deltaTime;
			if ((double)timeUpdatePositionLast > 0.3)
			{
				timeUpdatePositionLast = 0f;
				positionPlayerLast.position = playerT.position;
			}
			if (Vector3.Distance(mitaT.position, positionPlayerLast.position) > 1f && (double)Vector3.Distance(playerT.position, positionPlayerLast.position) > 0.5)
			{
				mita.AiWalkToTarget(positionPlayerLast, null);
			}
		}
	}

	private void OnEnable()
	{
		follow = true;
		playerT = GlobalTag.player.transform;
		positionPlayerLast = new GameObject().transform;
		positionPlayerLast.transform.parent = base.transform.parent;
		positionPlayerLast.position = playerT.position;
		mitaT = mita.animMita.transform;
	}

	public void GoSit()
	{
		eventGoSit.Invoke();
		mita.AiWalkToTarget(positionSit, eventStartSit);
		Object.Destroy(this);
	}

	public void FollowStop()
	{
		follow = false;
		positionPlayerLast.position = mita.animMita.transform.position;
	}
}
