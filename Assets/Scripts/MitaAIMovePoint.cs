using UnityEngine;
using UnityEngine.Events;

public class MitaAIMovePoint : MonoBehaviour
{
	public MitaPerson mita;

	public UnityEvent eventFinish;

	public bool magnetAfter;

	public AnimationClip animationWalkMita;

	public Transform targetMove;

	public void PlaySharply()
	{
		if (targetMove == null)
		{
			targetMove = base.transform;
		}
		mita.animMita.Play("Walk", 0, 0f);
		mita.AiWalkToTarget(targetMove, eventFinish);
	}

	public void Play()
	{
		if (targetMove == null)
		{
			targetMove = base.transform;
		}
		if (animationWalkMita != null)
		{
			mita.animMita.GetComponent<Animator_FunctionsOverride>().AnimationClipWalk(animationWalkMita);
		}
		mita.MagnetOff();
		mita.AiWalkToTarget(targetMove, eventFinish);
	}

	public void PlayRotateAndWalk()
	{
		if (targetMove == null)
		{
			targetMove = base.transform;
		}
		mita.animMita.GetComponent<Animator_FunctionsOverride>().AnimationClipWalk(animationWalkMita);
		mita.AiWalkToTargetRotate(targetMove, eventFinish);
	}
}
