using UnityEngine;

public class Location5_MitaFollowPlayer : MonoBehaviour
{
	private Transform target;

	private Animator anim;

	private float speedWalk;

	private float timeRotate;

	private float distancePlayer;

	public Character_Look characterLook;

	private void Start()
	{
		target = GlobalTag.player.transform;
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		distancePlayer = Vector3.Distance(base.transform.position, target.position);
		if (timeRotate == 0f)
		{
			if ((double)distancePlayer > 1.1)
			{
				speedWalk = Mathf.Lerp(speedWalk, 1f, Time.deltaTime * 5f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(target.position - base.transform.position, Vector3.up), Time.deltaTime * 8f);
			}
			if ((double)distancePlayer < 0.8)
			{
				speedWalk = Mathf.Lerp(speedWalk, -1f, Time.deltaTime * 5f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(target.position - base.transform.position, Vector3.up), Time.deltaTime * 8f);
			}
			if ((double)distancePlayer >= 0.8 && (double)distancePlayer <= 1.1)
			{
				speedWalk = Mathf.Lerp(speedWalk, 0f, Time.deltaTime * 5f);
			}
			if ((double)Vector3.Dot(base.transform.forward, Vector3.Normalize(new Vector3(target.position.x, base.transform.position.y, target.position.z) - base.transform.position)) < 0.45)
			{
				characterLook.StartRotate(target.position);
				timeRotate = 1.2f;
			}
		}
		else
		{
			speedWalk = Mathf.Lerp(speedWalk, 0f, Time.deltaTime * 5f);
		}
		if (timeRotate > 0f)
		{
			timeRotate -= Time.deltaTime;
			if (timeRotate < 0f)
			{
				timeRotate = 0f;
			}
		}
		anim.SetFloat("Walk", speedWalk);
		base.transform.position += base.transform.forward * (speedWalk * Time.deltaTime * 0.6f);
	}

	public void ReTarget(Transform _target)
	{
		target = _target;
	}
}
