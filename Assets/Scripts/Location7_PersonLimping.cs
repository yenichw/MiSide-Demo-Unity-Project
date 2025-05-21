using UnityEngine;
using UnityEngine.Events;

public class Location7_PersonLimping : MonoBehaviour
{
	public UnityEvent eventStartExit;

	public UnityEvent eventContinueWalk;

	private float speed;

	private Animator anim;

	private bool walk;

	private bool exit;

	private void OnEnable()
	{
		anim = GetComponent<Animator>();
		walk = true;
		speed = 0.7f;
	}

	public void Update()
	{
		if (walk)
		{
			speed = Mathf.Lerp(speed, 0.7f, Time.deltaTime * 4f);
			base.transform.position -= new Vector3(Time.deltaTime * speed, 0f, 0f);
			if ((double)base.transform.position.x <= -21.712)
			{
				exit = true;
				base.transform.position = new Vector3(-21.712f, base.transform.position.y, base.transform.position.z);
				anim.SetTrigger("Exit");
				walk = false;
				eventStartExit.Invoke();
			}
		}
		else
		{
			speed = Mathf.Lerp(speed, 0f, Time.deltaTime * 8f);
		}
	}

	public void PlayerNear()
	{
		if (!exit)
		{
			walk = false;
			anim.SetBool("Walk", value: false);
		}
	}

	public void ContinueMove()
	{
		walk = true;
		anim.SetBool("Walk", value: true);
		eventContinueWalk.Invoke();
	}
}
