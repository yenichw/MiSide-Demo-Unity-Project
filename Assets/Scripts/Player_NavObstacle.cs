using UnityEngine;
using UnityEngine.AI;

public class Player_NavObstacle : MonoBehaviour
{
	private PlayerMove scrpm;

	private NavMeshObstacle scrnmo;

	private void Start()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		scrnmo = GetComponent<NavMeshObstacle>();
		scrnmo.enabled = false;
	}

	private void Update()
	{
		scrnmo.enabled = scrpm.dontMove;
	}
}
