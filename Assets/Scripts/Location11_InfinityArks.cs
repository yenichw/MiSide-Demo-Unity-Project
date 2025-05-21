using UnityEngine;

public class Location11_InfinityArks : MonoBehaviour
{
	private Transform playerT;

	private void Start()
	{
		playerT = GlobalTag.player.transform;
	}

	public void LookPitcure()
	{
		playerT.position -= base.transform.position;
		base.transform.position = Vector3.zero;
	}
}
