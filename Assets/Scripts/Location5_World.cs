using UnityEngine;
using UnityEngine.Events;

public class Location5_World : MonoBehaviour
{
	private bool mitaKnife;

	[Header("Мита")]
	public MitaPerson mita;

	public UnityEvent eventKnifeNearPlayer;

	public AnimationClip animationWalkKnife;

	public Vector3 positionStart;

	public Transform pointMoveScreamer;

	private void Update()
	{
		if (mitaKnife && (double)Vector3.Distance(mita.animMita.transform.position, GlobalTag.player.transform.position) < 0.75)
		{
			eventKnifeNearPlayer.Invoke();
			mitaKnife = false;
		}
	}

	public void MitaKnife()
	{
		mitaKnife = true;
		mita.animMita.transform.position = positionStart;
		mita.animMita.transform.rotation = Quaternion.LookRotation(GlobalAM.DirectionFloor(mita.animMita.transform.position, GlobalTag.player.transform.position), Vector3.up);
		mita.animMita.GetComponent<Animator_FunctionsOverride>().AnimationClipWalk(animationWalkKnife);
		mita.animMita.SetBool("Walk", value: true);
		mita.animMita.Play("Walk", -1, 1f);
		mita.AiWalkToTarget(pointMoveScreamer, null);
		mita.AiSpeedNavigation(2.5f);
	}
}
