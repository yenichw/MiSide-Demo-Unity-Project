using UnityEngine;

public class Player_AutoSit : MonoBehaviour
{
	public float radius = 1f;

	private PlayerMove scrpm;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
	}

	private void Update()
	{
		if (Vector3.Distance(scrpm.transform.position, base.transform.position) < radius)
		{
			scrpm.SitHold();
		}
	}
}
